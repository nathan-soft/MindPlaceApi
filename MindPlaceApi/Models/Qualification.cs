using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Models
{
    public class Qualification : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required, MaxLength(255)]
        public string SchoolName { get; set; }
        [Required, MaxLength(100)]
        public string QualificationType { get; set; }
        [Required, MaxLength(255)]
        public string Major { get; set; }
        [Required]
        public int StartYear { get; set; }
        [Required]
        public int EndYear { get; set; }

        public virtual AppUser User { get; set; }
    }
}
