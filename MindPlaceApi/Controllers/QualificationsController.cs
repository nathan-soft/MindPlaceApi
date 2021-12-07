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
    [Authorize(Roles = "Patient, Professional")]
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Route("api/[controller]")]
    public class QualificationsController : ControllerBase
    {
        private readonly IQualificationService _qualificationService;
        public QualificationsController(IQualificationService qualificationService)
        {
            _qualificationService = qualificationService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<QualificationResponseDto>> AddQualificationAsync(QualificationDto qualificationCred)
        {
            try
            {
                var response = await _qualificationService.AddQualificationAsync(qualificationCred);
                if (response.Success)
                {
                    var uri = $"{Request.Host}/api/qualifications/{response.Data.Id}";
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
                return Problem("This qualification already exist.", statusCode: 409);
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpPut("{qualificationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<QualificationResponseDto>> UpdateQualificationAsync(int qualificationId, QualificationDto qualificationCreds)
        {
            try
            {
                var response = await _qualificationService.UpdateQualificationAsync(qualificationId, qualificationCreds);

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
                return Problem("This qualification already exist.", statusCode: 409);
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpDelete("{qualificationId}")]
        public async Task<ActionResult<ResponseMessageDto>> DeleteQualificationAsync(int qualificationId)
        {
            try
            {
                var result = await _qualificationService.DeleteQualificationAsync(qualificationId);

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
