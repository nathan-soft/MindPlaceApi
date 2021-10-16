using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos.Response
{
    public class SubscriptionResponseDto
    {
        public int Id { get; set; }
        public AbbrvUser User { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
