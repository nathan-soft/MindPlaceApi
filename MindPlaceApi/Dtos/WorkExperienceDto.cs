using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static MindPlaceApi.Codes.AppHelper;

namespace MindPlaceApi.Dtos
{
    public class WorkExperienceDto
    {
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string JobTitle { get; set; }
        [Required, EnumDataType(typeof(EmploymentType))]
        public string EmploymentType { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public bool CurrentlyWorking { get; set; }
    }
}
