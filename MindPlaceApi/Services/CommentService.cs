using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MindPlaceApi.Codes;
using MindPlaceApi.Data.Repositories;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MindPlaceApi.Codes.AppHelper;

namespace MindPlaceApi.Services
{
    public interface ICommentService
    {
        Task<ServiceResponse<CommentResponseDto>> AddCommentAsync(int questionId, CommentDto commentDetails);
        Task<ServiceResponse<string>> DeleteCommentAsync(int questionId, int commentId);
        Task<ServiceResponse<CommentResponseDto>> EditCommentAsync(int questionId, int commentId, CommentDto commentDetails);
        Task<ServiceResponse<CommentResponseDto>> GetCommentAsync(int questionId, int commentId);
        Task<ServiceResponse<List<CommentResponseDto>>> GetCommentsAsync(int questionId);
    }

    public class CommentService : ICommentService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public CommentService(IRepositoryWrapper repositoryWrapper,
                             IMapper mapper,
                             INotificationService notificationService,
                             UserManager<AppUser> userManager,
                             IUserService userService,
                             IHttpContextAccessor httpContextAccessor,
                             IBackgroundJobClient backgroundJobClient)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _notificationService = notificationService;
            _userManager = userManager;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<ServiceResponse<List<CommentResponseDto>>> GetCommentsAsync(int questionId)
        {
            var sr = new ServiceResponse<List<CommentResponseDto>>();

            //get  question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, $"The question with the id '{questionId}' was not found.", false);
            }

            sr.Code = 200;
            sr.Data = _mapper.Map<List<CommentResponseDto>>(question.Comments.OrderByDescending(c => c.CreatedOn));
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<CommentResponseDto>> GetCommentAsync(int questionId, int commentId)
        {
            var sr = new ServiceResponse<CommentResponseDto>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, $"The question resource was not found.", false);
            }

            var comment = question.Comments.Where(comment => comment.Id == commentId).FirstOrDefault();

            if (comment == null)
            {
                return sr.HelperMethod(404, $"The comment with the id '{commentId}' was not found.", false);
            }

            sr.Code = 200;
            sr.Data = _mapper.Map<CommentResponseDto>(comment);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<CommentResponseDto>> AddCommentAsync(int questionId, CommentDto commentDetails)
        {
            var sr = new ServiceResponse<CommentResponseDto>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, "Question not found.", false);
            }

            //convert to question entity
            var comment = _mapper.Map<Comment>(commentDetails);
            comment.QuestionId = questionId;

            var username = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            //get userId from db.
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                //assign the id of the user.
                comment.UserId = user.Id;
            }
            else
            {
                //error
                //User session is prolly expired or something.
                return sr.HelperMethod(400, $"Unable to load user with username '{username}'", false);
            }

            //add to db
            await _repositoryWrapper.Comment.InsertAsync(comment);
            //save to database.
            await _repositoryWrapper.SaveChangesAsync();


            //if the authenticated user making the comment isn't the user that asked the question in the first place
            if (question.UserId != comment.UserId)
            {
                //send notification to the user that asked the question.
                //name of authenticated commenter.
                var nameOfUser = $"<b>{comment.User.FirstName} {comment.User.LastName}</b>";
                var message = $"{nameOfUser} commented on your question.";

                //GET THE SITE DOMAIN
                var host = _httpContextAccessor.HttpContext.Request.Host;
                var url = $"{host}/api/questions/{questionId}";

                _backgroundJobClient.Enqueue(() => _notificationService.NewNotificationAsync(comment.UserId, question.UserId, message, url, NotificationType.COMMENT));
            }

            //return newly created resource.
            sr.Code = 200;
            sr.Data = _mapper.Map<CommentResponseDto>(comment);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<CommentResponseDto>> EditCommentAsync(int questionId, int commentId, CommentDto commentDetails)
        {
            var sr = new ServiceResponse<CommentResponseDto>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, $"The question with id: '{questionId}' was not found.", false);
            }

            var comment = question.Comments.Where(comment => comment.Id == commentId).FirstOrDefault();

            if (comment == null)
            {
                return sr.HelperMethod(404, $"The comment was not found.", false);
            }

            //current user
            var currUser = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            //The authenticated user that made the comment is the only user allowed to edit a comment
            if (comment.User.UserName != currUser)
            {
                //trying to edit somebody else's question.
                return sr.HelperMethod(403, "The action is forbidden", false);
            }

            //edit the comment
            comment.Content = commentDetails.Content;
            //save to db
            _repositoryWrapper.Comment.Update(comment);
            await _repositoryWrapper.SaveChangesAsync();

            sr.Code = 200;
            sr.Data = _mapper.Map<CommentResponseDto>(comment);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<string>> DeleteCommentAsync(int questionId, int commentId)
        {
            var sr = new ServiceResponse<string>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, $"The question was not found.", false);
            }

            var comment = question.Comments.Where(comment => comment.Id == commentId).FirstOrDefault();

            if (comment == null)
            {
                return sr.HelperMethod(404, $"The comment was not found.", false);
            }


            //current user
            var currUser = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            //people allowed to delete a comment are:
            //The authenticated user that made the comment.
            //The user/patient that asked the question..
            //And Admins
            if (comment.User.UserName != currUser
                            && comment.Question.User.UserName != currUser
                            && (_httpContextAccessor.HttpContext.User.IsInRole("Patient")
                            || _httpContextAccessor.HttpContext.User.IsInRole("Professional")))
            {
                //trying to delete somebody else's question.
                return sr.HelperMethod(403, "The action is forbidden", false);
            }

            //delete the comment
            _repositoryWrapper.Comment.Delete(comment);
            await _repositoryWrapper.SaveChangesAsync();

            return sr.HelperMethod(message: $"The comment was successfully deleted.");
        }

    }
}
