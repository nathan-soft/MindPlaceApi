using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MindPlaceApi.Codes;
using MindPlaceApi.Data.Repositories;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MindPlaceApi.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<string>> NewUserAsync(NewUserDto newUser);
        Task<ServiceResponse<string>> ConfirmEmail(EmailConfirmationDto confirmationDetails);
        Task<ServiceResponse<string>> ChangePassword(string username, ChangePasswordRequest passwordRequest);
        Task DeleteUserRelatingDataAsync(AppUser user);
        //ServiceResponse<string> DeleteUsers(string[] userEmails);
        //Task DeleteUsersAsync(string[] userEmails);
        Task<ServiceResponse<string>> DisableUserAsync(string userEmail);
        Task<ServiceResponse<List<UserResponseDto>>> GetAdministrativeUsersAsync();
        Task<ServiceResponse<List<string>>> GetNonAdministrativeUserEmailsAsync();
        Task<ServiceResponse<List<UserResponseDto>>> GetNonAdministrativeUsersAsync(string filterString);
        Task<ServiceResponse<UserResponseDto>> GetUserAsync(string username);
        Task<ServiceResponse<List<UserResponseDto>>> GetProfessionalsAsync();
        Task<ServiceResponse<string>> SendEmailConfirmationTokenAsync(string username);
        //Task<ServiceResponse<string>> SendMailAsync(SendBroadcastMailDto mailInfo);
        Task<ServiceResponse<UserResponseDto>> UpdateUserAsync(string username, EditUserDto userCreds);
        Task<ServiceResponse<List<SubscriptionResponseDto>>> GetUserSubscriptionRequestsAsync(string username);
        Task<ServiceResponse<List<AbbrvUser>>> GetSuggestedProfessionalsAsync();
        Task<ServiceResponse<List<QuestionResponseDto>>> GetUserQuestionsAsync(string username);
        Task<ServiceResponse<string>> SendPasswordResetTokenAsync(string email);
        Task<ServiceResponse<string>> ResetPassword(ResetPasswordDto userDetails);
        Task<ServiceResponse<string>> ChangeProfilePictureAsync(string username, IFormFile profilePhoto);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        private readonly IRepositoryWrapper _repositoryWrapper;

        private readonly IBlobService _blobService;
        private readonly IEmailService _emailService;

        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        private readonly IBackgroundJobClient _backgroundJobClient;
        public UserService(UserManager<AppUser> userManager,
                           RoleManager<AppRole> roleManager,
                           IRepositoryWrapper repositoryWrapper,
                           IEmailService emailService,
                           IBlobService blobService,
                           IMapper mapper,
                           IHttpContextAccessor httpContextAccessor,
                           IBackgroundJobClient backgroundJobClient,
                           IConfiguration configuration)
        {
            _emailService = emailService;
            _blobService = blobService;

            _userManager = userManager;
            _roleManager = roleManager;
            _repositoryWrapper = repositoryWrapper;
            
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _backgroundJobClient = backgroundJobClient;
        }
        
        public async Task<ServiceResponse<UserResponseDto>> GetUserAsync(string username)
        {
            var sr = new ServiceResponse<UserResponseDto>();
            //get user
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser != null)
            {
                //the user exist in db.
                var userDto = _mapper.Map<UserResponseDto>(foundUser);
                sr.Code = 200;
                sr.Success = true;
                sr.Data = userDto;
            }
            else
            {
                //user does not exist.
                sr.HelperMethod(404, $"Unable to load user with username '{username}'", false);
            }
            return sr;
        }

        public async Task<ServiceResponse<List<UserResponseDto>>> GetProfessionalsAsync()
        {
            var sr = new ServiceResponse<List<UserResponseDto>> ();
            //get Professionals
            var professionals = await _userManager.Users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Professional")).ToListAsync();

            sr.Code = 200;
            sr.Success = true;
            sr.Data = _mapper.Map<List<UserResponseDto>>(professionals);
            return sr;
        }

        public async Task<ServiceResponse<List<AbbrvUser>>> GetSuggestedProfessionalsAsync()
        {
            var sr = new ServiceResponse<List<AbbrvUser>>();
            //get current user username
            var currUser = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            //get Professionals
            var professionals = await _userManager.Users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Professional") 
                                                                && !u.RelationshipWithPatients.Any(f => f.Patient.UserName == currUser)
                                                                ).ToListAsync();


            sr.Code = 200;
            sr.Success = true;
            sr.Data = _mapper.Map<List<AbbrvUser>>(professionals);
            return sr;
        }

        public async Task<ServiceResponse<string>> NewUserAsync(NewUserDto newUser)
        {
            //convert to AppUser
            var convertedUser = _mapper.Map<AppUser>(newUser);
            return await CreateUserAsync(convertedUser, newUser.Password, newUser.Role);
        }

        private async Task<ServiceResponse<string>> CreateUserAsync(AppUser newUser, string password, params string[] roles)
        {
            var sr = new ServiceResponse<string>();

            if ((DateTime.Now.Year - newUser.DOB.Year) < 16) {
                return sr.HelperMethod(400, "You have to be at least 16 years old to register on Mindplace.", false);
            }

            if (!string.IsNullOrWhiteSpace(newUser.TimeZone))
            {
                //validate time zone entered.
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(newUser.TimeZone);
                }
                catch
                {
                    return sr.HelperMethod(400, "Please select a valid time zone.", false);
                }

            }

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    string message = "Role does not exist.";
                    if (roles.Count() > 1)
                    {
                        message = "One of the roles does not exist.";
                    }

                    return sr.HelperMethod(400, message, false);
                }
            }

            var foundUser = await _userManager.FindByEmailAsync(newUser.Email);

            if (foundUser != null)
            {
                //the email exist in db.
                return sr.HelperMethod(409, "email address already exist.", false);
            }

            if (newUser.Email.Contains("mindplace.com"))
            {
                //mark email as confirmed for internally created users.
                newUser.EmailConfirmed = true;
            }

            //create the user.
            newUser.IsActive = true;
            newUser.CreatedOn = DateTime.UtcNow;
            newUser.UpdatedOn = DateTime.UtcNow;

            //GET THE SITE DOMAIN
            var host = _configuration.GetSection("ClientDomainAddress").Value;

            if (roles.Contains("Patient"))
            {
                //create referral code for patients only.
                newUser.ReferralCode = $"{newUser.UserName}/{ConvertToTimestamp(newUser.CreatedOn)}";
            }
            
            var result = await _userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                //associate the user with specified role(s)
                var rolesAdded = await _userManager.AddToRolesAsync(newUser, roles);
                if (!rolesAdded.Succeeded)
                {
                    throw new AggregateException(rolesAdded.Errors.Select(err => new Exception(err.Description)));
                }

                if (newUser.Email.Contains("mindplace.com"))
                {
                    //don't send confirmation mail for internally created users.
                    return sr.HelperMethod(200, "Registered Successfully.", true);
                }
                
                //get confirmation token
                string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                string encodedToken = WebUtility.UrlEncode(confirmationToken);
                //get confirmation link
                string confirmationLink = $"{host}/public/login?userId={newUser.UserName}&token={encodedToken}";

                //send confirmation mail
                await _emailService.SendConfirmationMailAsync(newUser, confirmationLink);

                return sr.HelperMethod(200, "Registered Successfully. kindly check your mailbox or spam folder to verify your mail.", true);
            }
            else
            {
                throw new AggregateException(result.Errors.Select(err => new Exception(err.Description)));
            }
        }

        public async Task<ServiceResponse<string>> SendEmailConfirmationTokenAsync(string username)
        {
            var sr = new ServiceResponse<string>();
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {
                //user does not exist.
                sr.HelperMethod(404, $"Unable to load user with username '{username}'", false);
            }

            if (foundUser.EmailConfirmed)
            {
                //email already confirmed.
                sr.HelperMethod(400, $"The email associated with this account has already been confirmed.", false);
            }

            //get confirmation token
            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(foundUser);
            string encodedToken = WebUtility.UrlEncode(confirmationToken);
            
            //this is wrong, do not return the token
            sr.Data = encodedToken;
            return sr;
        }

        public async Task<ServiceResponse<string>> SendPasswordResetTokenAsync(string email)
        {
            var sr = new ServiceResponse<string>();
            var errMsg = "There was an error while resetting your password.";

            var foundUser = await _userManager.FindByEmailAsync(email);
            if (foundUser == null)
            {
                //user does not exist.
                // Don't reveal that the user does not exist or is not confirmed
                return sr.HelperMethod(400, errMsg, false);
            }

            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(foundUser);

            if (!isEmailConfirmed)
            {
                //user does not exist.
                // Don't reveal that the user does not exist or is not confirmed
                return sr.HelperMethod(400, errMsg, false);
            }


            //GET THE SITE DOMAIN
            var host = _configuration.GetSection("ClientDomainAddress").Value;

            //get confirmation token
            string confirmationToken = await _userManager.GeneratePasswordResetTokenAsync(foundUser);
            //url encode token.
            string confirmationTokenEncoded = WebUtility.UrlEncode(confirmationToken);

            //get confirmation link
            string passwordResetLink = $"{host}/public/ResetPassword?userId={foundUser.UserName}&token={confirmationTokenEncoded}";

            //send confirmation mail
            await _emailService.SendPasswordResetMailAsync(foundUser.FirstName, foundUser.LastName, foundUser.Email, passwordResetLink);

            sr.Data = "Password reset link has been sent to your email address.";
            return sr;
        }

        public async Task<ServiceResponse<string>> ConfirmEmail(EmailConfirmationDto confirmationDetails)
        {
            var sr = new ServiceResponse<string>();
            var foundUser = await _repositoryWrapper.User
                                                    .Find(u => u.UserName == confirmationDetails.Username)
                                                    .Include(u => u.Referrals)
                                                        .ThenInclude(ur => ur.Referrer)
                                                        .ThenInclude(r => r.Wallet)
                                                    .FirstOrDefaultAsync();

            if (foundUser == null)
            {
                return sr.HelperMethod(404, $"Unable to load user with username '{confirmationDetails.Username}'", false);
            }

            if (foundUser.EmailConfirmed)
            {
                return sr.HelperMethod(400, $"Your email has been confirmed already.", false);
            }

            if (!confirmationDetails.Token.Contains('+'))
            {
                //decode the token as it's still url encoded;
                confirmationDetails.Token = WebUtility.UrlDecode(confirmationDetails.Token);
            }

            //set updated column
            //foundUser.UpdatedOn = DateTime.UtcNow;
            //get referrals
            var userReferrals = foundUser.Referrals;
            //check if the user was referred...
            if (userReferrals.Any(ur => ur.ReferredUserId == foundUser.Id))
            {
                //get information about the referral
                var referralInfo = userReferrals.Where(r => r.ReferredUserId == foundUser.Id).SingleOrDefault();
                //mark the referral process as completed.
                referralInfo.CompletedOn = DateTime.UtcNow;

                //get the user(patient) that referred this user...
                var referrer = referralInfo.Referrer;

                if(referrer.Wallet != null)
                {
                    //credit the referrer with 5 credits...
                    referrer.Wallet.Balance += 5;
                    await _repositoryWrapper.Transaction.InsertAsync(new Transaction
                    {
                        WalletId = referrer.Wallet.Id,
                        Units = 5,
                        Description = "Referral bonus credit for '" + foundUser.Email + "'",
                        Type = TransactionType.CREDIT.ToString()
                    });
                }
                else
                {
                    //open a new wallet for the referrer
                    //credit the referrer with 5 credits...
                    var wallet = new Wallet
                    {
                        Balance = 5,
                        UserId = referrer.Id,
                        Transactions = new Collection<Transaction>
                        {
                           new Transaction
                            {
                                Units = 5,
                                Description = "Referral bonus credit for '" + foundUser.Email + "'",
                                Type = TransactionType.CREDIT.ToString()
                            }
                        }
                    };
                    await _repositoryWrapper.Wallet.InsertAsync(wallet);
                }
                

                //create Notification for the referrer.
                await _repositoryWrapper.Notification.InsertAsync(new Notification
                {
                    CreatedForId = referrer.Id,
                    Message = "Your have been credited with 5 units on your wallet, please check your transaction history for more detials.",
                    Type = NotificationType.WALLETTRANSACTION.ToString()
                });
            }

            var result = await _userManager.ConfirmEmailAsync(foundUser, confirmationDetails.Token);
            if (!result.Succeeded)
            {
                return sr.HelperMethod(400, "The token is invalid or has expired.", false);
            }

            return sr.HelperMethod(200, "You have successfully confirmed your email.", true);

        }

        public async Task<ServiceResponse<string>> ResetPassword(ResetPasswordDto userDetails)
        {
            var sr = new ServiceResponse<string>();

            var user = await _userManager.FindByNameAsync(userDetails.Username);

            if(user == null)
            {
                //user does not exist.
                // Don't reveal that the user does not exist.
                return sr.HelperMethod(400, "There was an error while resetting your password", false);
            }

            //set updated column
            user.UpdatedOn = DateTime.UtcNow;

            if (!userDetails.Token.Contains('+'))
            {
                //decode the token as it's still url encoded;
                userDetails.Token = WebUtility.UrlDecode(userDetails.Token);
            }

            var result = await _userManager.ResetPasswordAsync(user, userDetails.Token, userDetails.Password);

            if (!result.Succeeded)
            {
                var message = string.Empty;
                foreach (var error in result.Errors)
                {
                    message += $"{error.Description} ";
                }

                return sr.HelperMethod(400, message, false);
            }

            sr.Data = "Password reset successful!";
            return sr;
        }

        public async Task<ServiceResponse<string>> ChangePassword(string username, ChangePasswordRequest passwordRequest)
        {
            var sr = new ServiceResponse<string>();
            if (username != _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser())
            {
                //make sure the username matches.
                return sr.HelperMethod(403, "The action is forbidden", false);
            }

            var user = await _userManager.FindByNameAsync(username);

            //set updated column
            user.UpdatedOn = DateTime.UtcNow;

            var result = await _userManager.ChangePasswordAsync(user, passwordRequest.OldPassword, passwordRequest.NewPassword);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(err => err.Description.Contains("Incorrect password.")))
                {
                    return sr.HelperMethod(400, "The 'Old Password' you entered is invalid.", false);
                }
                else
                {
                    var message = "New password must be between 6 to 100 characters long, consisting of at least 1 upper-case, 1 digit and 1 non-alphanumeric character";
                    return sr.HelperMethod(400, message, false);
                }

            }

            return sr.HelperMethod(200, "Password change successful.", true);
        }

        public async Task<ServiceResponse<string>> ChangeProfilePictureAsync(string username, IFormFile profilePhoto)
        {
            var sr = new ServiceResponse<string>();
            //VALIDATIONS
            //validate uploaded Image

            var imgValidationResult = ValidateImage(profilePhoto);
            if (imgValidationResult != "Valid")
            {
                return sr.HelperMethod(400, imgValidationResult, false);
            }

            //create random name
            var newFileName = GetUniqueFileName(profilePhoto.FileName);

            //get user
            var currUser = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            var user = await _userManager.FindByNameAsync(currUser);

            if (user == null)
            {
                return sr.HelperMethod(401, "You need to be authenticated to perform this action.", false);
            }

            if (user.UserName != username)
            {
                return sr.HelperMethod(403, "The action is forbidden.", false);
            }

            //get container name
            var containerName = _configuration.GetSection("Storage:ContainerName").Value;

            //delete previous profile photo if any
            if (!string.IsNullOrWhiteSpace(user.ImageUrl))
            {
                var blobName = user.ImageUrl.Split('/')[4];
                _backgroundJobClient.Enqueue(() => _blobService.DeleteBlobAsync(containerName, blobName));
            }

            //upload image to azure storage.
            var imageUrl = await _blobService.UploadFileBlobAsync(containerName,
                                                                  profilePhoto.OpenReadStream(),
                                                                  newFileName);

            //Update user.
            user.ImageUrl = imageUrl;
            user.UpdatedOn = DateTime.UtcNow;
            await _repositoryWrapper.SaveChangesAsync();

            sr.Data = "Image Upload was Successful.";
            return sr;
        }


        /// <summary>
        /// Gets susbscription requests for a user.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// For a patient, returns a list of subscription request made by the patient. For a professional, Returns  list of subscription requests made to the professional.
        /// </returns>
        public async Task<ServiceResponse<List<SubscriptionResponseDto>>> GetUserSubscriptionRequestsAsync(string username)
        {
            var sr = new ServiceResponse<List<SubscriptionResponseDto>>();

            if (username != _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser())
            {
                //make sure the username matches.
                return sr.HelperMethod(403, "You're not allowed to view other people's subscription requests.", false);
            }

            var user = await _userManager.FindByNameAsync(username);

            List<Follow> query = new List<Follow>();
            List<SubscriptionResponseDto> subscriptionRequests = new List<SubscriptionResponseDto>();

            if (user.UserRoles.Any(r => r.Role.Name == "Patient"))
            {
                query =  await _repositoryWrapper.Follow.GetPatientSubscriptionRequestsAsync(user.Id);

                //project to DTO.
                subscriptionRequests = query.Select(sr => new SubscriptionResponseDto
                {
                    Id = sr.Id,
                    Status = sr.Status,
                    DateCreated = sr.CreatedOn,
                    User = new AbbrvUser
                    {
                        FullName = $"{sr.Professional.FirstName} {sr.Professional.LastName}",
                        Username = sr.Professional.UserName,
                        ImageUrl = sr.Professional.ImageUrl
                    }
                }).ToList();
            }
            else
            {
                query = await _repositoryWrapper.Follow.GetSubscriptionRequestsForProfessionalAsync(user.Id);

                //project to DTO.
                subscriptionRequests = query.Select(sr => new SubscriptionResponseDto
                {
                    Id = sr.Id,
                    Status = sr.Status,
                    DateCreated = sr.CreatedOn,
                    User = new AbbrvUser
                    {
                        FullName = $"{sr.Patient.FirstName} {sr.Patient.LastName}",
                        Username = sr.Patient.UserName,
                        ImageUrl = sr.Patient.ImageUrl
                    }
                }).ToList();
            }

            sr.Code = 200;
            sr.Success = true;
            sr.Data = subscriptionRequests;
            return sr;
        }

        public async Task<ServiceResponse<List<QuestionResponseDto>>> GetUserQuestionsAsync(string username)
        {
            var sr = new ServiceResponse<List<QuestionResponseDto>>();

            var foundUser = await _userManager.FindByNameAsync(username);

            if (foundUser == null)
            {
                sr.HelperMethod(404, $"Unable to load user with username '{username}'", false);
            }

            List<Question> question = new List<Question>();

            //get question from db;
            if (foundUser.UserRoles.Any(r => r.Role.Name == "Patient"))
            {
                question = await _repositoryWrapper.Question.GetPatientQuestions(foundUser.Id)
                                                            .Include(q => q.User)
                                                            .OrderByDescending(q => q.UpdatedOn)
                                                            .ToListAsync();
            }
            else
            {
                question = await _repositoryWrapper.Question.GetQuestionsForProfessional(foundUser.Id)
                                                            .Include(q => q.User)
                                                            .OrderByDescending(q => q)
                                                            .ToListAsync();
            }

            sr.Data = _mapper.Map<List<QuestionResponseDto>>(question);
            sr.Code = 200;
            sr.Success = true;
            return sr;
        }



        private static long ConvertToTimestamp(DateTime value)
        {
            DateTime unixStart = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
            long epoch = (long)Math.Floor((value.ToUniversalTime() - unixStart).TotalSeconds);
            return epoch;
        }

        private bool ImageGreaterThan2Mb(long imgSize)
        {
            //1 Kb = 1024 Byte
            //1 Mb = 1024 Kb
            //1 Mb = 1024x1024 Byte
            //1 Mb = 1 048 576 Byte

            if (imgSize > (1048576 * 2))
                return true;
            else
                return false;
        }

        private string ValidateImage(IFormFile uploadedImage)
        {
            //valid extensions.
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };

            var ext = Path.GetExtension(uploadedImage.FileName).ToLowerInvariant();
            //check valid extensions.
            if (!permittedExtensions.Contains(ext))
            {
                return "Only accepting '.jpeg, '.jpg' or '.png' images.";
            }
            //check content type and others...
            if (!uploadedImage.IsImage())
            {
                return "Please upload an image.";
            }
            //check image size.
            if (ImageGreaterThan2Mb(uploadedImage.Length))
            {
                return "Maximum allowed file size is 2MB.";
            }
            return "Valid";
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }










        //////////////////////////////////////////// METHODS NOT YET IN USE ///////////////////////////////////////////
        public async Task<ServiceResponse<List<UserResponseDto>>> GetAdministrativeUsersAsync()
        {
            var users = await _repositoryWrapper.User.GetAdministrativeUsersAsync();
            var sr = new ServiceResponse<List<UserResponseDto>>();

            sr.Code = 200;
            sr.Success = true;
            sr.Data = _mapper.Map<List<UserResponseDto>>(users);
            return sr;
        }

        public async Task<ServiceResponse<List<UserResponseDto>>> GetNonAdministrativeUsersAsync(string filterString)
        {
            var users = await _repositoryWrapper.User.GetNonAdministrativeUsersAsync(filterString);
            var sr = new ServiceResponse<List<UserResponseDto>>();

            sr.Code = 200;
            sr.Success = true;
            sr.Data = _mapper.Map<List<UserResponseDto>>(users);
            return sr;
        }

        public async Task<ServiceResponse<List<string>>> GetNonAdministrativeUserEmailsAsync()
        {
            var users = await _repositoryWrapper.User.GetNonAdministrativeUserEmailAsync();
            var sr = new ServiceResponse<List<string>>();

            sr.Code = 200;
            sr.Success = true;
            sr.Data = users;
            return sr;
        }

        public async Task<ServiceResponse<UserResponseDto>> UpdateUserAsync(string username, EditUserDto userCreds)
        {
            //get user
            var foundUser = await _userManager.FindByNameAsync(username);
            var sr = new ServiceResponse<UserResponseDto>();
            //check that user exist
            if (foundUser == null)
            {
                //user does not exist.
                sr.HelperMethod(404, "user not found.", false);
            }
            else
            {
                //var convertedUser = _mapper.Map<AppUser>(userCreds);
                //update the user info.
                foundUser.FirstName = userCreds.FirstName;
                foundUser.LastName = userCreds.LastName;
                //foundUser.Email = userCreds.Email;
                foundUser.PhoneNumber = userCreds.PhoneNumber;
                foundUser.Address = userCreds.Address;
                foundUser.City = userCreds.City;
                foundUser.State = userCreds.State;
                foundUser.Country = userCreds.Country;
                foundUser.UpdatedOn = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(foundUser);
                if (result.Succeeded)
                {
                    var userDto = _mapper.Map<UserResponseDto>(foundUser);

                    //return userInfo
                    sr.Code = 200;
                    sr.Data = userDto;
                    sr.Success = true;
                }
            }
            return sr;
        }

        public async Task<ServiceResponse<string>> DisableUserAsync(string userEmail)
        {
            var sr = new ServiceResponse<string>();

            //VALIDATE
            if (_httpContextAccessor.HttpContext.GetUsernameOfCurrentUser() != userEmail)
            {
                //error
                //a user is trying to delete another user
                return sr.HelperMethod(403, "you do not have the permission to perform this action.", false);
            }

            //get user.
            var user = await _userManager.FindByNameAsync(userEmail);
            //check exsitence of user.
            if (user == null)
            {
                //error
                return sr.HelperMethod(404, "User not found.", false);
            }

            //For users that are mentors or mentees, make sure to delete every other data relating to them.
            //Mentors with active mentorship shouldn't b able to delete their accounts.
            //Disable user.
            user.IsActive = false;
            user.UpdatedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            sr.Code = 200;
            sr.Message = "User disabled successfully.";
            sr.Success = true;
            return sr;
        }

        //public ServiceResponse<string> DeleteUsers(string[] userEmails)
        //{
        //    var sr = new ServiceResponse<string>();

        //    _backgroundJobClient.Enqueue(() => DeleteUsersAsync(userEmails));

        //    return sr.HelperMethod(message: "Users Removed.");
        //}

        public async Task DeleteUserRelatingDataAsync(AppUser user)
        {
            var userNotifications = new List<Notification>();
            var userFollowers = new List<Follow>();

            if (user.UserRoles.Any(ur => ur.Role.Name == "Professional"))
            {
                //GET MENTOR FOLLOWERS
                //userFollowers.AddRange(user.RelationshipWithMentees);

            }
            else if (user.UserRoles.Any(ur => ur.Role.Name == "Patient"))
            {
                //GET MENTEE FOLLOWERS
                //userFollowers.AddRange(user.RelationshipWithMentors);
            }

            //GET ALL NOTIFICATIONS INVOLVING THE USER.
            userNotifications.AddRange(user.CreatedByNotifications);
            userNotifications.AddRange(user.CreatedForNotifications);

            //DELETE RELATIONSHIPS...
            _repositoryWrapper.Notification.DeleteRange(userNotifications);
            //_repositoryWrapper.Follow.DeleteRange(userFollowers);
            await _repositoryWrapper.SaveChangesAsync();
        }

        //public async Task DeleteUsersAsync(string[] userEmails)
        //{
        //    //get users to be deleted.
        //    var usersToBeDeleted = _userManager.Users.Where(u => userEmails.Contains(u.Email)).ToList();

        //    //var getUserTasks = new List<Task>();
        //    //var t = Task.WhenAll(getUserTasks);
        //    //will only throw the first exception.
        //    //await t;

        //    try
        //    {
        //        foreach (var user in usersToBeDeleted)
        //        {
        //            //getUserTasks.Add(DeleteUserRelatingDataAsync(user));

        //            //using await here ensures that the dbcontext don't run on multiple thread which could cause errors
        //            //(e.g data corruption).
        //            //reference: https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#avoiding-dbcontext-threading-issues
        //            await DeleteUserRelatingDataAsync(user);
        //        }

        //        //Delete users.
        //        _repositoryWrapper.User.DeleteUsers(usersToBeDeleted);
        //        await _repositoryWrapper.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        var mailUser = new SendBroadcastMailDto()
        //        {
        //            Recipients = new List<string>() { "nathan@idevworks.com" },
        //            Subject = "Unable To Delete User(s)",
        //            Message = ex.Message
        //        };

        //        await SendMailAsync(mailUser);
        //    }
        //}

        //public async Task<ServiceResponse<string>> SendMailAsync(SendBroadcastMailDto mailInfo)
        //{
        //    var sr = new ServiceResponse<string>();

        //    //count number of recipients in mail
        //    if (mailInfo.Recipients.Count() == 1)
        //    {
        //        var mailStatus = await SendMailToUserAsync(mailInfo.Recipients.First(), mailInfo.Subject, mailInfo.Message);
        //        if (mailStatus)
        //        {
        //            return sr.HelperMethod(message: "Mail sent successfully.");
        //        }
        //        else
        //        {
        //            //error
        //            return sr.HelperMethod(404, "The email address provided isn't a valid user email address.", false);
        //        }
        //    }
        //    else
        //    {
        //        //the number of recipients the mail needs to be sent to, is more than one.
        //        return SendBroadcastMailToUsers(mailInfo.Subject, mailInfo.Message, mailInfo.Recipients.ToList());
        //    }

        //}

        /// <summary>
        /// Sends a personalized mail to a user
        /// </summary>
        /// <param name="userEmail">The email address of the user</param>
        /// <param name="subject">The subject of the mail</param>
        /// <param name="message">The content/body of the mail</param>
        /// <returns>a task that resolves to a boolean. True if the mail was sent successfully, otherwise false.</returns>
        private async Task<bool> SendMailToUserAsync(string userEmail, string subject, string message)
        {
            var sr = new ServiceResponse<string>();

            //get qualification.
            var user = await _userManager.FindByEmailAsync(userEmail);
            //check exsitence of user.
            if (user == null)
            {
                //error
                return false;
            }

            //Attempt to send mail
            await _emailService.SendMailToUserAsync($"{user.FirstName} {user.LastName}", user.Email, subject, message);

            return true;
        }

        /// <summary>
        /// Sends the same mail to list of users.
        /// </summary>
        /// <param name="subject">The subject of the mail</param>
        /// <param name="message">The content/body of the mail</param>
        /// <param name="emailAddresses">The email addresses of the users the mail is to be sent to.</param>
        /// <returns>
        /// a string contains a message infroming thath the mail have been queued for sending.
        /// </returns>
        //private ServiceResponse<string> SendBroadcastMailToUsers(string subject, string message, List<string> emailAddresses)
        //{
        //    var sr = new ServiceResponse<string>();

        //    //filter mail addresses.
        //    //get unique
        //    emailAddresses = emailAddresses.Distinct().ToList();

        //    //Add to background job queue.
        //    _backgroundJobClient.Enqueue(() => _emailService.SendBroadcastMailAsync(subject, message, emailAddresses, Guid.NewGuid()));

        //    return sr.HelperMethod(message: "Your Mail have been added to the queue.");
        //}

    }
}