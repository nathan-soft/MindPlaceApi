using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MindPlaceApi.Codes;
using MindPlaceApi.Data.Repositories;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using Microsoft.AspNetCore.Identity;
using static MindPlaceApi.Codes.AppHelper;
using Microsoft.AspNetCore.Http;

namespace MindPlaceApi.Services
{
    public interface INotificationService
    {
        Task<ServiceResponse<string>> DeleteNotificationAsync(int notificationId);
        Task<ServiceResponse<string>> MarkNotificationAsSeenAsync(int notificationId);
        Task NewNotificationAsync(int? createdBy, int createdFor, string message, string resourceUrl, NotificationType type);
        Task<ServiceResponse<List<NotificationResponseDto>>> GetUserNotificationsAsync(string username);
    }

    public class NotificationService : INotificationService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(IRepositoryWrapper repositoryWrapper, IMapper mapper, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task NewNotificationAsync(
            int? createdBy,
            int createdFor,
            string message,
            string resourceUrl,
            NotificationType type)
        {
            var notification = new Notification
            {
                CreatedById = createdBy,
                CreatedForId = createdFor,
                Message = message,
                ResourceUrl = resourceUrl,
                Type = type.ToString(),
                IsSeen = false
            };

            await _repositoryWrapper.Notification.InsertAsync(notification);
            await _repositoryWrapper.SaveChangesAsync();
        }

        public async Task<ServiceResponse<List<NotificationResponseDto>>> GetUserNotificationsAsync(string username)
        {
            var sr = new ServiceResponse<List<NotificationResponseDto>>();

            if (username != _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser())
            {
                //make sure the username matches.
                return sr.HelperMethod(403, "You're not allowed to view other people's notifications.", false);
            }

            //verify user exists
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {
                return sr.HelperMethod(404, "User not found", false);
            }

            var userNotifications = await _repositoryWrapper.Notification.GetNotificationsForUserAsync(foundUser.Id);

            sr.Code = 200;
            sr.Data = _mapper.Map<List<NotificationResponseDto>>(userNotifications);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<string>> MarkNotificationAsSeenAsync(int notificationId)
        {
            var sr = new ServiceResponse<string>();

            var notification = await _repositoryWrapper.Notification.GetByIdAsync(notificationId);
            if (notification == null)
            {
                return sr.HelperMethod(404, "Notification not found", false);
            }

            notification.IsSeen = true;
            _repositoryWrapper.Notification.Update(notification);
            await _repositoryWrapper.SaveChangesAsync();
            //return newly created resource
            sr.Code = 201;
            sr.Data = "Success";
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<string>> DeleteNotificationAsync(int notificationId)
        {
            var sr = new ServiceResponse<string>();

            var notification = await _repositoryWrapper.Notification.GetByIdAsync(notificationId);

            if (notification == null)
            {
                return sr.HelperMethod(404, "Notification not found", false);
            }

            _repositoryWrapper.Notification.Delete(notification);
            await _repositoryWrapper.SaveChangesAsync();
            //Success
            sr.Code = 200;
            sr.Message = "Notification Deleted!";
            sr.Success = true;
            return sr;
        }

    }
}