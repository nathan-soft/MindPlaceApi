using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Column(TypeName = "decimal(18, 2")]
        public decimal Balance { get; set; }
        public virtual AppUser User { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
