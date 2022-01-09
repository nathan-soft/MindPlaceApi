using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindPlaceApi.Codes;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LifeLongApi.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly INotificationService _notificationService;
        private readonly IQualificationService _qualificationService;
        private readonly IWorkExperienceService _workExperienceService;
        private readonly IMapper _mapper;

        public UsersController(IMapper mapper, IUserService userService, IRoleService roleService, INotificationService notificationService, IQualificationService qualificationService, IWorkExperienceService workExperienceService)
        {
            _userService = userService;
            _roleService = roleService;
            _notificationService = notificationService;
            _qualificationService = qualificationService;
            _workExperienceService = workExperienceService;
            _mapper = mapper;
        }

        [HttpGet("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<UserResponseDto>> GetUserAsync(string username)
        {
            try
            {
                var result = await _userService.GetUserAsync(username);
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

        [HttpGet("professionals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<List<UserResponseDto>>> GetProfessionalsAsync()
        {
            try
            {
                var result = await _userService.GetProfessionalsAsync();
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
        [HttpGet("suggested-professionals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<List<AbbrvUser>>> GetSuggestedProfessionalsAsync()
        {
            try
            {
                var result = await _userService.GetSuggestedProfessionalsAsync();
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
        [HttpPatch("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<string>> ConfirmUserEmailAsync(EmailConfirmationDto confirmationDetails)
        {
            try
            {
                var result = await _userService.ConfirmEmail(confirmationDetails);
                if (result.Success)
                {
                    //return data.
                    return Ok(result.Message);
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                //set status code.
                return Problem(ex.Message, statusCode: 500);
            }

        }

        [HttpPatch("{username}/change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<string>> ChangePassword(string username, ChangePasswordRequest details)
        {
            try
            {
                var result = await _userService.ChangePassword(username, details);

                if (result.Success)
                {
                    //return data.
                    return Ok(result.Message);
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

        [HttpPatch("{username}/change-profile-photo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProfilePictureDto>> ChangeProfilePhoto(string username, IFormFile profilePhoto)
        {
            try
            {
                var result = await _userService.ChangeProfilePictureAsync(username, profilePhoto);

                if (result.Success)
                {
                    //return data.
                    return Ok(new ProfilePictureDto { ImageUrl = result.Data});
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





        [HttpGet("{username}/notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<List<NotificationResponseDto>>> GetUserNotifications(string username)
        {
            try
            {
                var result = await _notificationService.GetUserNotificationsAsync(username);

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

        [HttpGet("{username}/subscription-requests")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<List<SubscriptionResponseDto>>> GetUserSubscriptionRequests(string username)
        {
            try
            {
                var result = await _userService.GetUserSubscriptionRequestsAsync(username);

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

        [Authorize(Roles = "Patient,Professional")]
        [HttpGet("{username}/questions")]
        public async Task<ActionResult<List<QuestionResponseDto>>> GetUserQuestionsAsync(string username)
        {
            try
            {
                var result = await _userService.GetUserQuestionsAsync(username);

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

        [HttpGet("{username}/qualifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<List<QualificationResponseDto>>> FetchUserQualificationsAsync(string username)
        {
            try
            {
                var response = await _qualificationService.GetCurrentUserQualificationsAsync(username);
                if (response.Success)
                {
                    //return data.
                    return Ok(response.Data);
                }
                else
                {
                    return Problem(response.Message, statusCode: response.Code);
                }
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpGet("{username}/work-experiences")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<List<WorkExperienceResponseDto>>> FetchUserWorkExperiences(string username)
        {
            try
            {
                var response = await _workExperienceService.GetUserWorkExperiencesAsync(username);
                if (response.Success)
                {
                    //return data.
                    return Ok(response.Data);
                }
                else
                {
                    return Problem(response.Message, statusCode: response.Code);
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