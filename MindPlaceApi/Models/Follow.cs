using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindPlaceApi.Models {
    public class Follow : BaseEntity {
        public int Id { get; set; }
        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        [ForeignKey("Professional")]
        public int ProfessionalId { get; set; }
        [Required]
        public string Status { get; set; }

        public virtual AppUser Professional { get; set; }
        public virtual AppUser Patient { get; set; }
    }
}