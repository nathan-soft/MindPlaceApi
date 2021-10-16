using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Models
{
    public class Tag : BaseEntity
    {
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; }
        [Required, MaxLength(250)]
        public string CreatedBy { get; set; }
        [MaxLength(250)]
        public string LastUpdatedBy { get; set; }
    }
}
