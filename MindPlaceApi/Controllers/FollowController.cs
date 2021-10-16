using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindPlaceApi.Codes;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MindPlaceApi.Controllers
{
    [Produces("application/json")]
    [Authorize(Roles = "Patient,Professional")]
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService )
        {
            _followService = followService;
        }

        // POST api/<FollowController>
        [Authorize(Roles ="Patient")]
        [HttpPost]
        public async Task<ActionResult<ResponseMessageDto>> CreateSubscriptionRequest(SubscriptionRequestDto details)
        {
            try
            {
                var result = await _followService
                                        .CreateNewSubscriptionAsync(details.UsernameOfProfessional, HttpContext.GetUsernameOfCurrentUser());

                if (result.Success)
                {
                    //return data.
                    return Ok(new { Message = result.Message });
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

        // PUT api/<FollowController>/5
        [Authorize(Roles ="Professional")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseMessageDto>> AcceptSubscriptionRequest(int id, UpdateSubscriptionRequestDto details)
        {
            try
            {
                var result = await _followService.ConfirmSubscriptionRequestAsync(id, details.PatientUsername);

                if (result.Success)
                {
                    //return data.
                    return Ok(new { Message = result.Message });
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

        // DELETE api/<FollowController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseMessageDto>> DeleteSubscriptionRequest(int id)
        {
            try
            {
                var result = await _followService.DeleteSubscriptionRequestAsync(id);

                if (result.Success)
                {
                    //return data.
                    return Ok(new { Message = result.Message });
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
