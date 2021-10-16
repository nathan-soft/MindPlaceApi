using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos
{
    public class QualificationDto
    {
        [Required]
        public string SchoolName { get; set; }
        [Required]
        public string QualificationType { get; set; }
        [Required]
        public string Major { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
    }
}
