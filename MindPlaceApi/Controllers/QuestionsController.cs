using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Services;

namespace MindPlaceApi.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : Controller
    {
        private readonly IQuestionService _questionService;

        private readonly ICommentService _commentService;

        public QuestionsController(IQuestionService questionService, ICommentService commentService)
        {
            _questionService = questionService;
            _commentService = commentService;
        }

        //[Authorize("Admin,Moderator")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<QuestionResponseDto>>> GetQuestionsAsync()
        {
            try
            {
                var result = await _questionService.GetQuestionsAsync();
                if (result.Success)
                {
                    //return data.
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(detail: ex.Message, statusCode: 500);
            }

        }

        [AllowAnonymous]
        [HttpGet("forum")]
        public async Task<ActionResult<PaginatedResponse<ForumQuestionResponseDto>>> GetForumQuestionsAsync([FromQuery]QuestionFilterParams filterParams)
        {
            try
            {
                var result = await _questionService.GetForumQuestionsAsync(filterParams);
                if (result.Success)
                {
                    //return data.
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(detail: ex.Message, statusCode: 500);
            }

        }

        [AllowAnonymous]
        [HttpGet("{questionId}")]
        public async Task<ActionResult<ForumQuestionResponseDto>> GetQuestionAsync(int questionId)
        {
            try
            {
                var result = await _questionService.GetQuestionAsync(questionId);
                if (result.Success)
                {
                    //return data.
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [AllowAnonymous]
        [HttpGet("forum/{questionId}")]
        public async Task<ActionResult<ForumPostResponseDto>> GetForumQuestionAsync(int questionId)
        {
            try
            {
                var result = await _questionService.GetForumQuestionAsync(questionId);
                if (result.Success)
                {
                    //return data.
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<QuestionResponseDto>> AddQuestionAsync(QuestionDto questionDetails)
        {
            try
            {
                var result = await _questionService.AddQuestionAsync(questionDetails);
                if (result.Success)
                {
                    var uri = new Uri($"{Request.Path}/{result.Data.Id}", UriKind.Relative);
                    //return data.
                    return Created(uri, result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [Authorize(Roles = "Patient")]
        [HttpPost("forum")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<ForumQuestionResponseDto>> AddForumQuestionAsync(ForumQuestionDto questionDetails)
        {
            try
            {
                var result = await _questionService.AddForumQuestionAsync(questionDetails);
                if (result.Success)
                {
                    var uri = new Uri($"{Request.Path}/{result.Data.Id}", UriKind.Relative);
                    //return data.
                    return Created(uri, result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [Authorize(Roles = "Patient")]
        [HttpPut("{questionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuestionResponseDto>> EditQuestionAsync(int questionId, QuestionDto questionDetails)
        {
            try
            {
                var result = await _questionService.EditQuestionAsync(questionId, questionDetails);
                if (result.Success)
                {
                    //return data.
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [Authorize(Roles = "Patient")]
        [HttpPut("forum/{questionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ForumQuestionResponseDto>> EditForumQuestionAsync(int questionId, ForumQuestionDto questionDetails)
        {
            try
            {
                var result = await _questionService.EditForumQuestionAsync(questionId, questionDetails);
                if (result.Success)
                {
                    //return data.
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [Authorize(Roles = "Patient,Admin,Moderator")]
        [HttpDelete("{questionId}")]
        public async Task<ActionResult<ResponseMessageDto>> DeleteQuestionAsync(int questionId)
        {
            try
            {
                var result = await _questionService.DeleteQuestionAsync(questionId);
                if (result.Success)
                {
                    //return data.
                    return Ok(new { message = result.Message });
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [AllowAnonymous]
        [HttpGet("{questionId}/comments")]
        public async Task<ActionResult<List<CommentResponseDto>>> GetCommentsAsync(int questionId)
        {
            try
            {
                var result = await _commentService.GetCommentsAsync(questionId);
                if (result.Success)
                {
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [AllowAnonymous]
        [HttpGet("{questionId}/comments/{commentId}")]
        public async Task<ActionResult<CommentResponseDto>> GetCommentAsync(int questionId, int commentId)
        {
            try
            {
                var result = await _commentService.GetCommentAsync(questionId, commentId);
                if (result.Success)
                {
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpPost("{questionId}/comments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<CommentResponseDto>> AddCommentAsync(int questionId, CommentDto commentDetails)
        {
            try
            {
                var result = await _commentService.AddCommentAsync(questionId, commentDetails);
                if (result.Success)
                {
                    var uri = new Uri($"{Request.Path}/{result.Data.Id}", UriKind.Relative);
                    //return data.
                    return Created(uri, result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [HttpPut("{questionId}/comments/{commentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentResponseDto>> EditCommentAsync(int questionId, int commentId, CommentDto commentDetails)
        {
            try
            {
                var result = await _commentService.EditCommentAsync(questionId, commentId, commentDetails);
                if (result.Success)
                {
                    //return data.
                    return Ok(result.Data);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpDelete("{questionId}/comments/{commentId}")]
        public async Task<ActionResult<ResponseMessageDto>> DeleteCommentAsync(int questionId, int commentId)
        {
            try
            {
                var result = await _commentService.DeleteCommentAsync(questionId, commentId);
                if (result.Success)
                {
                    //return data.
                    return Ok(new { message = result.Message });
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }
    }
}
