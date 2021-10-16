using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MindPlaceApi.Models {
    public class AppUser : IdentityUser<int> {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateTime DOB { get; set; }
        //public string Qualification { get; set; }
        //public string Employment { get; set; }
        public string ReferralCode { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
        public string TimeZone { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public virtual Wallet Wallet { get; set; }
        public virtual ICollection<Qualification> Qualifications { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<WorkExperience> WorkExperiences { get; set; }

        [InverseProperty("CreatedBy")]
        public virtual List<Notification> CreatedByNotifications { get; set; }
        [InverseProperty("CreatedFor")]
        public virtual List<Notification> CreatedForNotifications { get; set; }
        [InverseProperty("Professional")]
        public virtual List<Follow> RelationshipWithPatients { get; set; }
        [InverseProperty("Patient")] //return a list of follow entity where the patientId matches this user id.
        public virtual List<Follow> RelationshipWithProfessionals {get; set;}
        [InverseProperty("ReferredUser")]
        public virtual List<Referral> Referrals{ get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }

    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public virtual AppUser User { get; set; }
        public virtual AppRole Role { get; set; }
    }
}