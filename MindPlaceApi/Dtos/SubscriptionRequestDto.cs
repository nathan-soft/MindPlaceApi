using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos
{
    public class SubscriptionRequestDto
    {
        public string UsernameOfProfessional { get; set; }
    }

    public class UpdateSubscriptionRequestDto
    {
        public string PatientUsername { get; set; }
    }
}
