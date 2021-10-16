using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Models
{
    public class Referral : BaseEntity
    {
        /// <summary>
        /// The id of the user with that referred another user.
        /// </summary>
        public int ReferrerId { get; set; }
        /// <summary>
        /// the id of the user that was referred.
        /// </summary>
        public int ReferredUserId { get; set; }
        public DateTime? CompletedOn { get; set; }

        [ForeignKey("ReferrerId")]
        public virtual AppUser Referrer { get; set; }
        [ForeignKey("ReferredUserId")]
        public virtual AppUser ReferredUser { get; set; }
    }
}
