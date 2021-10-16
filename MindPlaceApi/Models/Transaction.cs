using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        [Required, MaxLength(100)]
        public string Description { get; set; }
        [Required]
        public string Type { get; set; }
        public virtual Wallet Wallet { get; set; }
    }

    public enum TransactionType
    {
        CREDIT,
        DEBIT
    }
}
