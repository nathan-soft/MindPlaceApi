using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MindPlaceApi.Dtos {
    public class NewUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
        [Required]
        public string Username { get; set; }

        [Required]
        public string Gender { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime DOB { get; set; }
        public string ReferralCode { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Please enter at least 6 alphanumeric characters", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string TimeZone { get; set; }
        [Required]
        public string Role { get; set; }
    }
}