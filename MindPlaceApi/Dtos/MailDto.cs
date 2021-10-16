using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos
{
    public class SendBroadcastMailDto
    {
        [Required]
        public IEnumerable<string> Recipients { get; set; }
        [Required, StringLength(50, MinimumLength = 2, ErrorMessage = "The {0} must be between 2 and 50 characters.")]
        public string Subject { get; set; }
        [Required, StringLength(5000, MinimumLength = 2, ErrorMessage = "The {0} must be between 2 and 5000 characters.")]
        public string Message { get; set; }
    }
}
