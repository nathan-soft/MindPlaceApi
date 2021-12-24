using System.ComponentModel.DataAnnotations;

namespace MindPlaceApi.Dtos
{
    public class ResetPasswordDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Please enter at least 6 alphanumeric characters", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string Token { get; set; }
        
    }

    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

    }
}
