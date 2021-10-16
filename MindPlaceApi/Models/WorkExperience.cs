using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Models
{
    public class WorkExperience : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required, MaxLength(255)]
        public string JobTitle { get; set; }
        [Required, MaxLength(70)]
        public string EmploymentType { get; set; }
        [Required, MaxLength(255)]
        public string CompanyName { get; set; }
        [Required, MaxLength(150)]
        public string Location { get; set; }
        [Required]
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public bool CurrentlyWorking { get; set; }

        public virtual AppUser User { get; set; }
    }
}
