using System;
using System.Collections.Generic;
using MindPlaceApi.Dtos.Response;

namespace MindPlaceApi.Dtos.Response
{
    public class UserResponseDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Gender { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public string UserName { get; set; }
        public DateTime DOB { get; set; }
        //only for patients
        public string ReferralCode { get; set; }
        //only for patients
        public int NoOfReferrals { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public List<AbbrvUser> Followers { get; set; }
        public List<AbbrvUser> Following { get; set; }

    }

    public class AbbrvUser
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }
    }
}