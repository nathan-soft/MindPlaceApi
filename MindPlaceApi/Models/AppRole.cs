using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace MindPlaceApi.Models {
    public class AppRole : IdentityRole<int> {
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}