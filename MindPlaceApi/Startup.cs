using Azure.Storage.Blobs;
using Hangfire;
using Hangfire.SqlServer;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MindPlaceApi.Data;
using MindPlaceApi.Data.Repositories;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using MindPlaceApi.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace MindPlaceApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProblemDetails();

            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBlobService, BlobService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFollowService, FollowService>();
            services.AddScoped<IQualificationService, QualificationService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWorkExperienceService, WorkExperienceService>();

            services.AddIdentity<AppUser, AppRole>(options => {
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<IdentityAppContext>()
                .AddDefaultTokenProviders();

            services.AddDbContext<IdentityAppContext>(options => {
                options.UseLazyLoadingProxies().UseSqlServer(Configuration.GetConnectionString("DefaultConnec"));
            });

            services.AddControllers().AddNewtonsoftJson();

            services.Configure<IdentityOptions>(options => {
                // Password settings.
                // options.Password.RequireDigit = true;
                // options.Password.RequireLowercase = true;
                // options.Password.RequireNonAlphanumeric = true;
                // options.Password.RequireUppercase = true;
                // options.Password.RequiredLength = 6;
                // options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                // options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = false;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._$";
                //options.User.RequireUniqueEmail = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(s => {
                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                s.CustomOperationIds(e =>
                {
                    //var Id = $"{e.ActionDescriptor.RouteValues["controller"]}_{e.HttpMethod}";
                    var controllerAction = (ControllerActionDescriptor)e.ActionDescriptor;

                    //var Id = ControllerAction.ControllerName + ControllerAction.ActionName;

                    return controllerAction.ActionName;
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options => {
                options.Events = new JwtBearerEvents()
                {
                    OnChallenge = context => {
                        context.HandleResponse();
                        var code = StatusCodes.Status401Unauthorized;
                        var response = new { Code = code.ToString(), Message = "Unauthorized access" };
                        context.Response.StatusCode = code;
                        context.HttpContext.Response.Headers.Append("www-authenticate", "Bearer");
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                    },
                    OnForbidden = context =>
                    {
                        var code = StatusCodes.Status403Forbidden;
                        var response = new { Code = code.ToString(), Message = "You do not have the permission to access the resource." };
                        context.Response.StatusCode = code;
                        context.HttpContext.Response.Headers.Append("www-authenticate", "Bearer");
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidAudience = Configuration.GetSection("Jwt").GetSection("Audience").Value,
                    ValidIssuer = Configuration.GetSection("Jwt").GetSection("Issuer").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                    .GetBytes(Configuration.GetSection("Jwt").GetSection("Key").Value)),
                    ClockSkew = TimeSpan.Zero
                };
            });
            services.AddSingleton(x => new BlobServiceClient(Configuration.GetSection("Storage:ConnectionString").Value));
            
            // Add Hangfire services.
            services.AddHangfire(c => c.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnec"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseProblemDetails();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mind API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard();
            });
        }
    }
}
