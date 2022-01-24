using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MindPlaceApi.Codes;
using MindPlaceApi.Data.Repositories;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Services
{
    public interface IQualificationService
    {
        Task<ServiceResponse<QualificationResponseDto>> AddQualificationAsync(QualificationDto qualificationCreds);
        Task<ServiceResponse<string>> DeleteQualificationAsync(int qualificationId);
        Task<ServiceResponse<List<QualificationResponseDto>>> GetUserQualificationsAsync(string username);
        Task<ServiceResponse<QualificationResponseDto>> UpdateQualificationAsync(int qualificationId, QualificationDto qualificationCreds);
    }

    public class QualificationService : IQualificationService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QualificationService(IRepositoryWrapper repositoryWrapper,
                             IMapper mapper,
                             UserManager<AppUser> userManager,
                             IHttpContextAccessor httpContextAccessor)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<ServiceResponse<List<QualificationResponseDto>>> GetUserQualificationsAsync(string username)
        {
            var sr = new ServiceResponse<List<QualificationResponseDto>>();

            //verify user exists
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {
                return sr.HelperMethod(404, $"Unable to find user with username '{username}'", false);
            }
            //gets all qualification for a user.
            var userQualifications = await _repositoryWrapper.Qualification.GetUserQualificationsAsync(foundUser.Id);

            sr.Data = _mapper.Map<List<QualificationResponseDto>>(userQualifications);
            return sr;
        }

        public async Task<ServiceResponse<QualificationResponseDto>> AddQualificationAsync(QualificationDto qualificationCreds)
        {
            var sr = new ServiceResponse<QualificationResponseDto>();

            //make sure years entered are reasonable/valid years.
            if (!ValidateQualificationYears(qualificationCreds.StartYear, qualificationCreds.EndYear, out string message))
            {
                sr.HelperMethod(400, message, false);
                return sr;
            }

            //get user exists
            var foundUser = await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.GetUsernameOfCurrentUser());

            //convert to Qualification entity.
            var userQualification = _mapper.Map<Qualification>(qualificationCreds);
            userQualification.UserId = foundUser.Id;
            //gets all qualification for a user.
            await _repositoryWrapper.Qualification.InsertAsync(userQualification);
            await _repositoryWrapper.SaveChangesAsync();

            //return newly created resource
            sr.Code = 201;
            sr.Data = _mapper.Map<QualificationResponseDto>(userQualification);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<QualificationResponseDto>> UpdateQualificationAsync(int qualificationId, QualificationDto qualificationCreds)
        {
            var sr = new ServiceResponse<QualificationResponseDto>();

            //make sure years entered are reasonable/valid years.
            if (!ValidateQualificationYears(qualificationCreds.StartYear, qualificationCreds.EndYear, out string message))
            {
                return sr.HelperMethod(400, message, false);
            }

            //get qualification.
            var userQualification = await _repositoryWrapper.Qualification.GetByIdAsync(qualificationId);
            //check exsitence of qualification.
            if (userQualification == null)
            {
                //error
                return sr.HelperMethod(404, "No qualification was found.", false);
            }

            //get username of current user.
            var currentUser = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            //verify username
            if (userQualification.User.UserName != currentUser)
            {
                return sr.HelperMethod(403, "You do not have the permission to update someone else's qualification.", false);
            }

            //map values.
            userQualification.SchoolName = qualificationCreds.SchoolName;
            userQualification.Major = qualificationCreds.Major;
            userQualification.QualificationType = qualificationCreds.QualificationType;
            userQualification.StartYear = qualificationCreds.StartYear;
            userQualification.EndYear = qualificationCreds.EndYear;
            //Update qualification for a user.
            _repositoryWrapper.Qualification.Update(userQualification);
            await _repositoryWrapper.SaveChangesAsync();
             
            sr.Code = 200;
            sr.Data = _mapper.Map<QualificationResponseDto>(userQualification);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<string>> DeleteQualificationAsync(int qualificationId)
        {
            var sr = new ServiceResponse<string>();

            //get qualification.
            var userHasQualification = await _repositoryWrapper.Qualification.GetByIdAsync(qualificationId);
            //check exsitence of qualification.
            if (userHasQualification == null)
            {
                //error
                return sr.HelperMethod(404, "No qualification matching id was found.", false);
            }

            //Delete qualification for a user.
            _repositoryWrapper.Qualification.Delete(userHasQualification);
            await _repositoryWrapper.SaveChangesAsync();

            sr.Code = 200;
            sr.Message = "Qualification delete was successful.";
            sr.Success = true;
            return sr;
        }

        private bool ValidateQualificationYears(int startYear, int endYear, out string message)
        {
            var currentYear = DateTime.Now.Year;
            var validMinimumYear = currentYear - 59;
            if (startYear < validMinimumYear || endYear > currentYear)
            {
                message = $"Please enter a valid start and end year between '{validMinimumYear}' and '{currentYear}'.";
                return false;
            }

            if (startYear > endYear)
            {
                message = "Start year cannot be greater than end year.";
                return false;
            }

            message = "";
            return true;
        }
    }
}
