using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Models
{
    public class Promotion : BaseEntity
    {
        public int Id { get; set; }
        public int MentorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public string UpdatedBy { get; set; }

        public virtual AppUser Professional { get; set; }
    }
}
