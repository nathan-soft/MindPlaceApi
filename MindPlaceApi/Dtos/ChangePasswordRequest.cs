using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos
{
    public class ChangePasswordRequest
    {
        [Required, DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [Required, DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must contain at least 6 characters consisting of at least 1 upper-case, 1 digit and 1 non-alphanumeric character.", MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
}
