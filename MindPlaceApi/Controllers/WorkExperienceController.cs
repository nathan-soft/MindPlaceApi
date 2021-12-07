using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Controllers
{
    [Produces("application/json")]
    [Authorize(Roles ="Patient, Professional")]
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Route("api/[controller]")]
    public class WorkExperienceController : ControllerBase
    {
        private readonly IWorkExperienceService _workExperienceService;
        public WorkExperienceController(IWorkExperienceService workExperienceService)
        {
            _workExperienceService = workExperienceService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<WorkExperienceResponseDto>> AddWorkExperience(WorkExperienceDto userWorkExperience)
        {
            try
            {
                var response = await _workExperienceService.AddAsync(userWorkExperience);
                if (response.Success)
                {
                    var uri = $"{Request.Host}/api/workexperience/{response.Data.Id}";
                    //return data.
                    return Created(uri, response.Data);
                    
                }
                else
                {
                    return Problem(response.Message, statusCode: response.Code);
                }
            }
            catch (UniqueConstraintException)
            {
                //duplicate record found.
                return Problem("This work experience already exists.", statusCode: 409);
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpPut("{workExperienceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<WorkExperienceResponseDto>> UpdateWorkExperienceAsync(int workExperienceId, WorkExperienceDto workExperience)
        {
            try
            {
                var response = await _workExperienceService.UpdateAsync(workExperienceId, workExperience);

                if (response.Success)
                {
                    //return data.
                    return Ok(response.Data);
                }
                else
                {
                    //error
                    return Problem(response.Message, statusCode: response.Code);
                }

            }
            catch (UniqueConstraintException)
            {
                //duplicate record found.
                return Problem("This work experience already exists.", statusCode: 409);
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpDelete("{workExperienceId}")]
        public async Task<ActionResult<ResponseMessageDto>> DeleteWorkExperience(int workExperienceId)
        {
            try
            {
                var result = await _workExperienceService.DeleteWorkExperienceAsync(workExperienceId);

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
