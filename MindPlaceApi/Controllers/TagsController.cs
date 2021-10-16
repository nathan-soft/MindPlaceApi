using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MindPlaceApi.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;
        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<List<TagResponseDto>>> ListTagsAsync()
        {
            try
            {
                var response = await _tagService.ListTagsAsync();
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

        [Authorize(Roles = "Admin, Moderator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<QualificationResponseDto>> AddTagAsync(TagDto tag)
        {
            try
            {
                var response = await _tagService.CreateTagAsync(tag);
                if (response.Success)
                {
                    var uri = $"{Request.Host}/api/tags/{response.Data.Id}";
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
                return Problem("A tag with same name already exist.", statusCode: 409);
            }
            catch (Exception ex)
            {
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }
    }
}
