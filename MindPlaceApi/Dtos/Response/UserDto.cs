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
        public string Qualification { get; set; }
        public string Employment { get; set; }
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

        //if the user  is a professional, this refers to the number of subscribed patients they have.
        //if the user is a patient,  this refers to the number of professionals they've subscribed to.
        //public int FriendsCount { get; set; }
        //refers to questions answered for professionals.
        //for patients, refers to total number of questions asked.
        public List<QuestionResponseDto> Questions { get; set; }

    }

    public class AbbrvUser
    {
        public string FullName { get; set; }
        public string Username { get; set; }
    }
}