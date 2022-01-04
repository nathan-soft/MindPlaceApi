using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindPlaceApi.Codes;
using MindPlaceApi.Data.Repositories;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Services
{
    public interface IQuestionService
    {
        Task<ServiceResponse<QuestionResponseDto>> AddQuestionAsync(QuestionDto questionDetails);
        Task<ServiceResponse<ForumQuestionResponseDto>> AddForumQuestionAsync(ForumQuestionDto questionDetails);
        Task<ServiceResponse<string>> DeleteQuestionAsync(int questionId);
        Task<ServiceResponse<QuestionResponseDto>> EditQuestionAsync(int questionId, QuestionDto questionDetails);
        Task<ServiceResponse<ForumQuestionResponseDto>> EditForumQuestionAsync(int questionId, ForumQuestionDto questionDetails);
        Task<ServiceResponse<PaginatedResponse<ForumQuestionResponseDto>>> GetForumQuestionsAsync(QuestionFilterParams filterParams);
        Task<ServiceResponse<ForumPostResponseDto>> GetForumQuestionAsync(int questionId);
        Task<ServiceResponse<ForumQuestionResponseDto>> GetQuestionAsync(int questionId);
        Task<ServiceResponse<List<QuestionResponseDto>>> GetQuestionsAsync();
        /// <summary>
        /// Likes a question
        /// </summary>
        /// <param name="questionId">The question to like.</param>
        /// <returns>>A task that resolve to a ServiceResponse class which will contain information about the asynchronous operation.</returns>
        Task<ServiceResponse<string>> LikeQuestionAsync(int questionId);
        /// <summary>
        /// Unlikes a question.
        /// </summary>
        /// <param name="questionId">The question to unlike.</param>
        /// <returns>A task that resolve to a ServiceResponse class which will contain information about the asynchronous operation.</returns>
        Task<ServiceResponse<string>> UnlikeQuestionAsync(int questionId);
    }

    public class QuestionService : IQuestionService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public QuestionService(IRepositoryWrapper repositoryWrapper,
                             IMapper mapper,
                             INotificationService notificationService,
                             UserManager<AppUser> userManager,
                             IUserService userService,
                             IHttpContextAccessor httpContextAccessor)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _notificationService = notificationService;
            _userManager = userManager;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<List<QuestionResponseDto>>> GetQuestionsAsync()
        {
            var sr = new ServiceResponse<List<QuestionResponseDto>>();

            //get question from db;
            var questions = await _repositoryWrapper.Question.GetAll()
                                                                .Include(q => q.User)
                                                                .OrderByDescending(k => k.UpdatedOn)
                                                                //.AsNoTracking()
                                                                .ToListAsync();
            //var questionList = questions.Select(q => new QuestionResponseDto
            //                    {
            //                        Id = q.Id,
            //                        Title = q.Title,
            //                        Content = q.Content,
            //                        CreatedOn = q.CreatedOn,
            //                        User = (q.User == null)? null : new AbbrvUser { 
            //                            FullName = $"{q.User.FirstName} {q.User.LastName}",
            //                            Username = q.User.UserName
            //                        },
            //                        Comments = q.Comments.Select(c => new CommentResponseDto 
            //                                                        { 
            //                                                            Content = c.Content,
            //                                                            CreatedOn = c.CreatedOn,
            //                                                            User = (c.User == null) ? null : new AbbrvUser
            //                                                            {
            //                                                                FullName = $"{q.User.FirstName} {q.User.LastName}",
            //                                                                Username = q.User.UserName
            //                                                            }
            //                                                        }
            //                        ).ToList()
            //                    }).ToList();

            sr.Data = _mapper.Map<List<QuestionResponseDto>>(questions);
            sr.Code = 200;
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<PaginatedResponse<ForumQuestionResponseDto>>> GetForumQuestionsAsync(QuestionFilterParams filterParams)
        {
            var sr = new ServiceResponse<PaginatedResponse<ForumQuestionResponseDto>>();

            //get question from db;
            var query = _repositoryWrapper.Question.GetForumQuestions()
                                          .Include(q => q.User)
                                          .Include(q => q.QuestionTags)
                                            .ThenInclude(qt => qt.Tag).AsQueryable();

            if (filterParams.CanSearchText)
            {
                query = query.Where(q => q.Title.Contains(filterParams.SearchText));
            }

            if (filterParams.FilterText == null)
            {
                filterParams.FilterText = "";
            }

            var filterBy = filterParams.FilterText.Trim().ToLower();
            if(filterBy == "popular of month")
            {
                query = query.Where(q => q.CreatedOn.Year == DateTime.UtcNow.Year && 
                                    q.CreatedOn.Month == DateTime.UtcNow.Month)
                                .OrderByDescending(q => q.Comments.Count)
                                .ThenByDescending(q => q.Likes.Count);
            }
            else if(filterBy == "popular this week"){
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                query = query.Where(q => q.CreatedOn >= startOfWeek && q.CreatedOn <= endOfWeek)
                                .OrderByDescending(q => q.Comments.Count)
                                .ThenByDescending(q => q.Likes.Count);
            }
            else if (filterBy == "trending")
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                query = query.Where(q => q.CreatedOn == DateTime.Today)
                                .OrderByDescending(q => q.Comments.Count);
            }
            else if(filterBy == "unanswered")
            {
                query = query.Where(q => q.Comments.Count == 0).OrderByDescending(q => q.CreatedOn);
            }
            else
            {
                //gets latest question(s)
                query = query.OrderByDescending(q => q.UpdatedOn);
            }

            //convert to paginated response.
            var result = await PaginatedResponse<ForumQuestionResponseDto>
                                                        .ToPagedListAsync(query, filterParams.PageNumber, filterParams.PageSize, _mapper);

            sr.Data = result;
            return sr;
        }

        public async Task<ServiceResponse<ForumQuestionResponseDto>> GetQuestionAsync(int questionId)
        {
            var sr = new ServiceResponse<ForumQuestionResponseDto>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, $"Unable to load the question.", false);
            }

            sr.Data = _mapper.Map<ForumQuestionResponseDto>(question);
            sr.Code = 200;
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<ForumPostResponseDto>> GetForumQuestionAsync(int questionId)
        {
            var sr = new ServiceResponse<ForumPostResponseDto>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetForumQuestions()
                                                      .Where(q => q.Id == questionId)
                                                      .Include(q => q.User)
                                                      .Include(q => q.Comments)
                                                      .Include(q => q.QuestionTags)
                                                        .ThenInclude(qt => qt.Tag).FirstOrDefaultAsync();

            if (question == null)
            {
                return sr.HelperMethod(404, "Unable to load the question.", false);
            }

            sr.Data = _mapper.Map<ForumPostResponseDto>(question);
            sr.Code = 200;
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<QuestionResponseDto>> AddQuestionAsync(QuestionDto questionDetails)
        {
            var sr = new ServiceResponse<QuestionResponseDto>();

            //convert to question entity
            var question = _mapper.Map<Question>(questionDetails);
            question.Status = QuestionStatus.APPROVED.ToString();
            question.Type = QuestionType.STANDARD.ToString();
            question.ApprovedBy = "SYSTEM";

            var username = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();

            if (string.IsNullOrWhiteSpace(username))
            {
                //Unexpected/Unlikely error
                return sr.HelperMethod(400, $"Unable to load user with username '{username}'", false);
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                //assign the id of the user.
                question.UserId = user.Id;
            }

            //save to database.
            await _repositoryWrapper.Question.InsertAsync(question);
            await _repositoryWrapper.SaveChangesAsync();

            //return newly created resource.
            sr.Data = _mapper.Map<QuestionResponseDto>(question);
            sr.Code = 201;
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<ForumQuestionResponseDto>> AddForumQuestionAsync(ForumQuestionDto questionDetails)
        {
            var sr = new ServiceResponse<ForumQuestionResponseDto>();

            //convert to question entity
            var question = _mapper.Map<Question>(questionDetails);
            question.Status = QuestionStatus.APPROVED.ToString();
            question.Type = QuestionType.FORUM.ToString();
            question.ApprovedBy = "SYSTEM";

            
            var username = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();

            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                //assign the id of the user.
                question.UserId = user.Id;
            }
            else
            {
                //error
                //A username that doesn't exist was used..
                return sr.HelperMethod(400, $"Unable to load user with username '{username}'", false);
            }

            //validate tags
            //string error = "";
            //var tags = new List<Tag>();
            //foreach(var tagName in questionDetails.Tags){
            //    var tag = _repositoryWrapper.Tag.GetByName(tagName);
            //    if (tag == null) {
            //        //tag don't exist.
            //        error = $"The tag '{tagName}' does not exist.";
            //        break;
            //    }

            //    //add to list...
            //    tags.Add(tag);
            //}

            ////return error if exist.
            //if (!string.IsNullOrWhiteSpace(error))
            //{
            //    return sr.HelperMethod(400, error, false);
            //}

            //add tags to questions.
            //question.QuestionTags = tags.Select(t => new QuestionTag { TagId = t.Id }).ToList();

            //save to database.
            await _repositoryWrapper.Question.InsertAsync(question);
            await _repositoryWrapper.SaveChangesAsync();

            //return newly created resource.
            sr.Data = _mapper.Map<ForumQuestionResponseDto>(question);
            sr.Code = 201;
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<QuestionResponseDto>> EditQuestionAsync(int questionId, QuestionDto questionDetails)
        {
            var sr = new ServiceResponse<QuestionResponseDto>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, "The question you're trying to edit wasn't found.", false);
            }

            //check if username matches.
            if (question.User?.UserName != _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser())
            {
                //trying to edit somebody else's question.
                return sr.HelperMethod(403, "Trying to edit someone else's question is not allowed.", false);
            }

            //update content and/title
            question.Title = questionDetails.Title;
            question.Content = questionDetails.Content;

            //save to database.
            _repositoryWrapper.Question.Update(question);
            await _repositoryWrapper.SaveChangesAsync();

            //return edited resource.
            sr.Data = _mapper.Map<QuestionResponseDto>(question);
            sr.Code = 200;
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<ForumQuestionResponseDto>> EditForumQuestionAsync(int questionId, ForumQuestionDto questionDetails)
        {
            var sr = new ServiceResponse<ForumQuestionResponseDto>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, "The question you're trying to edit wasn't found.", false);
            }

            //check if username matches.
            if (question.User?.UserName != _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser())
            {
                //trying to edit somebody else's question.
                return sr.HelperMethod(403, "Trying to edit someone else's question is not allowed.", false);
            }

            //validate tags
            //string error = "";
            //var updatedTags = new List<Tag>();
            ////get question old tags...
            //var oldTags = _repositoryWrapper.QuestionTag.GetQuestionTags(question.Id);
            //foreach (var tagName in questionDetails.Tags)
            //{
            //    //compare old tags with new ones...
            //    //check if "tagName" is part of old tags
            //    if (!oldTags.Any(t => t.Name == tagName)) {
            //        //not in old tags
            //        //fetch tag from db
            //        var tag = _repositoryWrapper.Tag.GetByName(tagName);
            //        if (tag == null)
            //        {
            //            //tag don't exist.
            //            error = $"The tag '{tagName}' does not exist.";
            //            break;
            //        }

            //        //add new tag to list...
            //        updatedTags.Add(tag);
            //    }
            //    else
            //    {
            //        //updated tag can be found in old tags..
            //        //add to updated list
            //        updatedTags.Add(oldTags.Where(t => t.Name == tagName).FirstOrDefault());
            //    }
            //}

            //return error if exist.
            //if (!string.IsNullOrWhiteSpace(error))
            //{
            //    return sr.HelperMethod(400, error, false);
            //}
            //delete old tags.
            //_repositoryWrapper.QuestionTag.DeleteRange(oldTags.Select(t => new QuestionTag { QuestionId = questionId, TagId = t.Id }));

            //update question
            question.Title = questionDetails.Title;
            question.Content = questionDetails.Content;
            //question.QuestionTags.AddRange(updatedTags.Select(t => new QuestionTag { TagId = t.Id }));

            //save to database.
            _repositoryWrapper.Question.Update(question);
            await _repositoryWrapper.SaveChangesAsync();

            //return edited resource.
            sr.Data = _mapper.Map<ForumQuestionResponseDto>(question);
            sr.Code = 200;
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<string>> LikeQuestionAsync(int questionId)
        {
            var sr = new ServiceResponse<string>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, $"Unable to load the question.", false);
            }

            var username = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {
                return sr.HelperMethod(404, $"Unable to load the current user.", false);
            }

            //save to database.
            await _repositoryWrapper.QuestionLike.InsertAsync(new QuestionLike {UserId = foundUser.Id, QuestionId = questionId });
            await _repositoryWrapper.SaveChangesAsync();

            sr.Data = "The like operation was successful.";
            return sr;
        }

        public async Task<ServiceResponse<string>> UnlikeQuestionAsync(int questionId)
        {
            var sr = new ServiceResponse<string>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, $"Unable to load the question.", false);
            }

            var username = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {
                return sr.HelperMethod(404, $"Unable to load the current user.", false);
            }

            //get the question like record.
            var likedQuestion = question.Likes.Where(ql => ql.UserId == foundUser.Id).SingleOrDefault();

            if (likedQuestion != null)
            {
                //unlike
                _repositoryWrapper.QuestionLike.Delete(likedQuestion);
                await _repositoryWrapper.SaveChangesAsync();
            }

            //success
            sr.Data = "You've unliked the question.";
            return sr;
        }

        public async Task<ServiceResponse<string>> DeleteQuestionAsync(int questionId)
        {
            var sr = new ServiceResponse<string>();

            //get question from db;
            var question = await _repositoryWrapper.Question.GetByIdAsync(questionId);

            if (question == null)
            {
                return sr.HelperMethod(404, $"Unable to load the question", false);
            }

            //check if username matches.
            if (question.User.UserName != _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser())
            {
                //trying to delete somebody else's question.
                return sr.HelperMethod(403, "The action is forbidden.", false);
            }

            //save to database.
            _repositoryWrapper.Question.Delete(question);
            await _repositoryWrapper.SaveChangesAsync();

            return sr.HelperMethod(message: "Question deleted successfully.");
        }
    }
}
