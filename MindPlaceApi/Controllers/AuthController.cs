using AutoMapper;
using LifeLongApi.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindPlaceApi.Codes;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Services;
using System;
using System.Threading.Tasks;

namespace MindPlaceApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;

        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IMapper mapper, IUserService userService, IBlobService blobService, IEmailService emailService)
        {
            _authService = authService;
            _userService = userService;
            _mapper = mapper;
            _blobService = blobService;
            _emailService = emailService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<TokenDto>> Login(LoginDto loginCreds) {
            var result = await _authService.Login(loginCreds.Username, loginCreds.Password);
            if (result.Success) {
                return Ok(result.Data);
            } else {
                //error occurred
                return Problem(result.Message, statusCode: result.Code);
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseMessageDto>> CreateUser(NewUserDto userInfo) {
            try
            {
                //try creating the user.
                var result = await _userService.NewUserAsync(userInfo);
                if (result.Success)
                {
                    //user was created successfully
                    return Ok(new ResponseMessageDto() { Message = result.Message });
                }
                else
                {
                    //error
                    return Problem(result.Message, statusCode: result.Code);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Username") && ex.Message.Contains("is already taken."))
                {
                    //constraint validation
                    return Problem("The username is already taken.", statusCode: 409);
                }
                //log and return default custom error
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<ResponseMessageDto>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var result = await _userService.SendPasswordResetTokenAsync(forgotPasswordDto.Email);
                if (result.Success)
                {
                    //return data.
                    return Ok(new ResponseMessageDto { Message = result.Data });
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

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType(typeof(ProblemDetails))]
        public async Task<ActionResult<ResponseMessageDto>> ResetPasswordAsync(ResetPasswordDto userDetails)
        {
            try
            {
                var result = await _userService.ResetPassword(userDetails);
                if (result.Success)
                {
                    //return data.
                    return Ok(new ResponseMessageDto { Message = result.Data });
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

        
        
        
        
        
        
        
        
        
        
        
        
        
        //[HttpPost("send-email-token")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        ////[ProducesDefaultResponseType(typeof(ProblemDetails))]
        //public async Task<ActionResult<string>> SendEmailToken(EmailTokenDto tokenDto)
        //{
        //    try
        //    {
        //        //try creating the user.
        //        var result = await _userService.SendConfirmationTokenAsync(tokenDto.Username);
        //        if (result.Success)
        //        {
        //            //user was created successfully
        //            return Ok(result.Data);
        //        }
        //        else
        //        {
        //            //error
        //            return Problem(result.Message, statusCode: result.Code);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //log and return default custom error
        //        return Problem(detail: ex.Message, statusCode: 500);
        //    }
        //}

        //[HttpGet("Send_Mail")]
        //public ActionResult TestMail()
        //{
        //    try
        //    {
        //        _emailService.SendTestMailAsync();
        //        return Ok("Mail sent!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

        //[HttpPost("test_upload")]
        //public async Task<ActionResult> TestUpload([FromForm] ArticleDto article)
        //{
        //    //try
        //    //{
        //    //    var result = await _blobService.UploadFileBlobAsync("blog-images",
        //    //                                                        article.UploadedImage.OpenReadStream(),
        //    //                                                        article.UploadedImage.FileName);
        //    //    return Ok(result);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    return BadRequest(new { error = ex.Message });
        //    //}
        //}
    }
}