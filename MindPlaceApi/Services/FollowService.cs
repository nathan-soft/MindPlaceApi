using AutoMapper;
using MindPlaceApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MindPlaceApi.Data.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MindPlaceApi.Codes;
using static MindPlaceApi.Codes.AppHelper;
using Hangfire;

namespace MindPlaceApi.Services
{
    public interface IFollowService
    {
        Task<ServiceResponse<string>> ConfirmSubscriptionRequestAsync(int id, string patientUsername);
        Task<ServiceResponse<string>> CreateNewSubscriptionAsync(string usernameOfProfessional, string currentUsername);
        Task<ServiceResponse<string>> DeleteSubscriptionRequestAsync(int id);
    }

    public class FollowService : IFollowService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public FollowService(IRepositoryWrapper repositoryWrapper,
                             IMapper mapper,
                             IEmailService emailService,
                             INotificationService notificationService,
                             UserManager<AppUser> userManager,
                             IUserService userService,
                             IHttpContextAccessor httpContextAccessor,
                             IBackgroundJobClient backgroundJobClient)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _emailService = emailService;
            _notificationService = notificationService;
            _userManager = userManager;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _backgroundJobClient = backgroundJobClient;
        }


        public async Task<ServiceResponse<string>> CreateNewSubscriptionAsync(string usernameOfProfessional, string currentUsername)
        {
            var sr = new ServiceResponse<string>();

            var patient = await _userManager.FindByNameAsync(currentUsername);

            if (patient == null)
            {
                return sr.HelperMethod(401, "Unauthenticated access.", false);
            }


            //get existing relationship
            if (patient.RelationshipWithProfessionals.Any(f => f.Professional.UserName == usernameOfProfessional))
            {
                return sr.HelperMethod(409, "Already subscribed to this professional.", false);
            }

            //verify professional exist
            var professional = await _userManager.FindByNameAsync(usernameOfProfessional);
            if (professional == null)
            {
                return sr.HelperMethod(404, "the professional does not exist.", false);
            }

            //create subscription request
            var subscribeRequest = new Follow
            {
                PatientId = patient.Id,
                ProfessionalId = professional.Id,
                Status = FollowStatus.PENDING.ToString()
            };

            //create notification for Professional
            var message = $"{patient.FirstName} {patient.LastName} has requested to <b>Follow</b> you.";
            var notification = new Notification
            {
                CreatedById = patient.Id,
                CreatedForId = professional.Id,
                Message = message,
                Type = NotificationType.SUBSCRIPTION.ToString(),
                IsSeen = false
            };

            //insert to db and save.
            await _repositoryWrapper.Follow.InsertAsync(subscribeRequest);
            await _repositoryWrapper.Notification.InsertAsync(notification);
            await _repositoryWrapper.SaveChangesAsync();

            if (!professional.Email.Contains("mindplace.com"))
            {
                //send mail to Professional
                _backgroundJobClient.Enqueue(() => _emailService.SendSubscriptionMailAsync(professional, patient));
            }

            sr.Code = 200;
            sr.Message = "Subscription request sent.";
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<string>> ConfirmSubscriptionRequestAsync(int id, string patientUsername)
        {
            var sr = new ServiceResponse<string>();

            var followRelationship = await _repositoryWrapper.Follow.GetByIdAsync(id);

            if (followRelationship == null)
            {
                return sr.HelperMethod(404, $"No pending request with id '{id}' was found.", false);
            }

            if (followRelationship.Professional.UserName != _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser())
            {
                return sr.HelperMethod(403, $"Accessing subscription request of other users is not allowed.", false);
            }

            //check existing relationship
            if (followRelationship.Patient.UserName != patientUsername)
            {
                return sr.HelperMethod(404, $"You have no pending request from the patient with username '{patientUsername}'.", false);
            }

            if(followRelationship.Status == FollowStatus.CONFIRMED.ToString())
            {
                return sr.HelperMethod(400, $"You are already following this patient.", false);
            }

            //accept follow request
            followRelationship.Status = FollowStatus.CONFIRMED.ToString();

            //create notification for Professional
            var message = $"{followRelationship.Professional.FirstName} {followRelationship.Professional.LastName} has accepted your subscription request.";
            var notification = new Notification
            {
                CreatedById = followRelationship.Professional.Id,
                CreatedForId = followRelationship.Patient.Id,
                Message = message,
                Type = NotificationType.SUBSCRIPTION.ToString(),
                IsSeen = false
            };

            //insert to db and save.
            _repositoryWrapper.Follow.Update(followRelationship);
            await _repositoryWrapper.Notification.InsertAsync(notification);
            await _repositoryWrapper.SaveChangesAsync();

            //send mail to Patient
            var patientFullname = $"{followRelationship.Patient.FirstName} {followRelationship.Patient.LastName}";
            _backgroundJobClient.Enqueue(() => _emailService.SendNotificationMailAsync(patientUsername, followRelationship.Patient.Email, patientFullname, "Subscription Request", message));

            sr.Code = 200;
            sr.Message = "Subscription request accepted!";
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<string>> DeleteSubscriptionRequestAsync(int id)
        {
            var sr = new ServiceResponse<string>();

            var followRelationship = await _repositoryWrapper.Follow.GetByIdAsync(id);

            if (followRelationship == null)
            {
                return sr.HelperMethod(404, $"No pending request with id '{id}' was found.", false);
            }

            //get username of current user
            var currentUser = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();

            if (followRelationship.Professional.UserName != currentUser && followRelationship.Patient.UserName != currentUser)
            {
                return sr.HelperMethod(403, $"Deleting subscription request of other users is not allowed.", false);
            }

            //delete from db and save.
            _repositoryWrapper.Follow.Delete(followRelationship);
            await _repositoryWrapper.SaveChangesAsync();

            sr.Code = 200;
            sr.Message = "Subscription request has been successfully deleted!";
            sr.Success = true;
            return sr;
        }

    }
}