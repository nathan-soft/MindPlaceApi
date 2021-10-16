using Microsoft.AspNetCore.Identity;
using MindPlaceApi.Dtos;
using MindPlaceApi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.DataInitializer
{
    public class DbSeeder
    {
        private static IUserService _userService;
        private static IFollowService _followService;

        public DbSeeder(IUserService userService,
                        IFollowService followService)
        {
            _followService = followService;
            _userService = userService;
        }
        public async  Task SeedDataAsync()
        {
            await SeedUsersAsync();
            await SeedMentorshipRequestAsync();

        }

        private static async Task SeedUsersAsync()
        {
            var users = new List<NewUserDto>() {
                new NewUserDto {
                    Email = "prof1@mindplace.com",
                    FirstName = "Professional",
                    LastName = "One",
                    Username = "prof_1",
                    Password = "Mindplace1$",
                    PhoneNumber = "08182257523",
                    Gender = "Male",
                    State = "California",
                    Country = "USA",
                    DOB = new DateTime(1990, 10, 12),
                    Role = "Professional",
                    TimeZone = TimeZoneInfo.Local.Id
                },
                new NewUserDto {
                    FirstName = "Professional",
                    LastName = "Two",
                    Gender = "Male",
                    Email = "prof2@mindplace.com",
                    Username = "prof_2",
                    Password = "Mindplace2$",
                    PhoneNumber = "08184477523",
                    State = "Lagos",
                    Country = "Nigeria",
                    DOB = new DateTime(1996, 6, 24),
                    Role = "Professional",
                    TimeZone = TimeZoneInfo.Local.Id
                },
                new NewUserDto
                {
                    FirstName = "Professional",
                    LastName = "Three",
                    Gender = "Female",
                    Email = "prof3@mindplace.com",
                    Username = "prof_3",
                    Password = "Mindplace3$",
                    PhoneNumber = "08188250023",
                    State = "New York",
                    Country = "USA",
                    DOB = new DateTime(1985, 1, 31),
                    Role = "Professional",
                    TimeZone = TimeZoneInfo.Local.Id
                },
                new NewUserDto
                {
                    FirstName = "Professional",
                    LastName = "Four",
                    Gender = "Female",
                    Email = "prof4@mindplace.com",
                    Username = "prof_4",
                    Password = "Mindplace4$",
                    PhoneNumber = "08144257623",
                    State = "Abuja",
                    Country = "Nigeria",
                    DOB = new DateTime(1987, 7, 7),
                    Role = "Professional",
                    TimeZone = TimeZoneInfo.Local.Id
                }
            };

            try
            {
                foreach (var user in users)
                {
                    //seed professionals
                    var result = await _userService.NewUserAsync(user);
                }

                //see patient
                await _userService.NewUserAsync(new NewUserDto
                {
                    Email = "patient@mindplace.com",
                    FirstName = "Patient",
                    LastName = "One",
                    Username = "patient_1",
                    Password = "Mindplace3$",
                    PhoneNumber = "08140715723",
                    Gender = "Female",
                    State = "New York",
                    Country = "USA",
                    DOB = new DateTime(1997, 5, 12),
                    Role = "Patient",
                    TimeZone = TimeZoneInfo.Local.Id
                });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static async Task SeedMentorshipRequestAsync(){
            var result = await _followService.CreateNewSubscriptionAsync("prof_2", "patient_1");
        }


        private static List<int> randomList = new List<int>();
        private static int NewNumber()
        {
            var MyNumber = new Random().Next(0, 6);
            
            if (!randomList.Contains(MyNumber))
            {
                randomList.Add(MyNumber);
            }
            return MyNumber;
        }
        
    }
}