using MindPlaceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EntityFramework.Exceptions.SqlServer;

namespace MindPlaceApi.Data {
    public class IdentityAppContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, ApplicationUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>> {
        public IdentityAppContext (DbContextOptions<IdentityAppContext> options) : base (options) {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseExceptionProcessor();
        }

        protected override void OnModelCreating (ModelBuilder modelBuilder) {
            base.OnModelCreating (modelBuilder);

            modelBuilder.Entity<AppUser>(b => {
                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
                b.HasIndex(e => e.UserName).IsUnique();

                //b.HasData(
                //    new AppUser
                //    {
                //        FirstName = "Professional",
                //        LastName = "One",
                //        Gender = "Male",
                //        Email = "prof1@mindplace.com",
                //        NormalizedEmail = "PROF1@MINDPLACE.COM",
                //        UserName = "prof_1",
                //        NormalizedUserName = "PROF_1",
                //        PasswordHash = "",
                //        PhoneNumber = "08182257523",
                //        Qualification = "B.sc",
                //        Employment = "Employed",
                //        State = "California",
                //        Country = "USA",
                //        DOB = new DateTime(1990, 10, 12),
                //        UserRoles = new List<ApplicationUserRole>() { new ApplicationUserRole { RoleId = 3} },
                //    },
                //    new AppUser
                //    {
                //        FirstName = "Professional",
                //        LastName = "Two",
                //        Gender = "Male",
                //        Email = "prof2@mindplace.com",
                //        NormalizedEmail = "PROF2@MINDPLACE.COM",
                //        UserName = "prof_2",
                //        NormalizedUserName = "PROF_2"
                //        PasswordHash = "",
                //        PhoneNumber = "08184477523",
                //        Qualification = "B.sc",
                //        Employment = "UnEmployed",
                //        State = "Lagos",
                //        Country = "Nigeria",
                //        DOB = new DateTime(1996, 6, 24),
                //        UserRoles = new List<ApplicationUserRole>() { new ApplicationUserRole { RoleId = 3 } }
                //    },
                //    new AppUser
                //    {
                //        FirstName = "Professional",
                //        LastName = "Three",
                //        Gender = "Female",
                //        Email = "prof3@mindplace.com",
                //        NormalizedEmail = "PROF3@MINDPLACE.COM",
                //        UserName = "prof_3",
                //        NormalizedUserName = "PROF_3"
                //        PasswordHash = "",
                //        PhoneNumber = "08188250023",
                //        Qualification = "B.sc",
                //        Employment = "Employed",
                //        State = "New York",
                //        Country = "USA",
                //        DOB = new DateTime(1985, 1, 31),
                //        UserRoles = new List<ApplicationUserRole>() { new ApplicationUserRole { RoleId = 3 } }
                //    }
                //);
            });

            modelBuilder.Entity<AppRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                b.HasData(
                    new AppRole
                    {
                        Id = 1,
                        Name = "Admin",
                        NormalizedName = "ADMIN"
                    },
                    new AppRole
                    {
                        Id = 2,
                        Name = "Moderator",
                        NormalizedName = "MODERATOR"
                    },
                    new AppRole
                    {
                        Id = 3,
                        Name = "Professional",
                        NormalizedName = "PROFESSIONAL"
                    },
                    new AppRole
                    {
                        Id = 4,
                        Name = "Patient",
                        NormalizedName = "PATIENT"
                    }
                );
            });

            modelBuilder.Entity<Referral>()
                .HasKey(r => new { r.ReferrerId, r.ReferredUserId });

            modelBuilder.Entity<Follow>().HasIndex(f => f.Status);
            modelBuilder.Entity<Follow>()
               .HasIndex(f => new { f.PatientId, f.ProfessionalId }).IsUnique();

            // modelBuilder.Entity<Follow> ()
            //     .HasOne (f => f.Mentee)
            //     .WithMany (a => a.Mentees)
            //     .HasForeignKey (f => f.MenteeId);

            // modelBuilder.Entity<Follow> ()
            //     .HasOne (f => f.Mentor)
            //     .WithMany (a => a.Mentors)
            //     .HasForeignKey (f => f.MentorId);

            modelBuilder.Entity<Qualification>()
                .HasIndex(Q => new { Q.UserId, Q.SchoolName, Q.QualificationType, Q.Major }).IsUnique();

            modelBuilder.Entity<Tag>().HasIndex(T => new { T.Name }).IsUnique();
            modelBuilder.Entity<Tag>(f =>
            {
                f.HasData(
                    new Tag
                    {
                        Id = 1,
                        Name = "Kasina",
                        CreatedBy = "admin@mindplace.com",
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow
                    },
                    new Tag
                    {
                        Id = 2,
                        Name = "Sirius",
                        CreatedBy = "admin@mindplace.com",
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow
                    }
                );
            });

            modelBuilder.Entity<QuestionLike>()
                        .HasIndex(sc => new { sc.QuestionId, sc.UserId })
                        .IsUnique();
            modelBuilder.Entity<QuestionTag>().HasKey(sc => new { sc.QuestionId, sc.TagId });

            modelBuilder.Entity<WorkExperience>()
                .HasIndex(W => new { W.UserId, W.CompanyName, W.JobTitle, W.StartYear }).IsUnique();
        }

        
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Followings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Qualification> Qualifications { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionLike> QuestionLikes { get; set; }
        public DbSet<QuestionTag> QuestionTags { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WorkExperience> WorkExperiences { get; set; }


        //public override int SaveChanges(bool acceptAllChangesOnSuccess)
        //{
        //    OnBeforeSaving();
        //    return base.SaveChanges(acceptAllChangesOnSuccess);
        //}

        public override int SaveChanges()
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return (await base.SaveChangesAsync(cancellationToken));
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedOn = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedOn = DateTime.UtcNow;
                }
            }
        }
    }
}