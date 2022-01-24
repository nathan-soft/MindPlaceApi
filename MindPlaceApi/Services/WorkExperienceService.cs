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
    public interface IWorkExperienceService
    {
        Task<ServiceResponse<WorkExperienceResponseDto>> AddAsync(WorkExperienceDto workExperience);
        Task<ServiceResponse<WorkExperienceResponseDto>> DeleteWorkExperienceAsync(int workExperienceId);
        Task<ServiceResponse<WorkExperienceResponseDto>> GetByIdAsync(int id);
        /// <summary>
        /// Gets the current user work experiences.
        /// </summary>
        /// <param name="username">the username of the currently logged in user.</param>
        /// <returns> A list of the current user work experiences.</returns>
        Task<ServiceResponse<List<WorkExperienceResponseDto>>> GetUserWorkExperiencesAsync(string username);
        Task<ServiceResponse<WorkExperienceResponseDto>> UpdateAsync(int workExperienceId, WorkExperienceDto workExperience);
    }

    public class WorkExperienceService : IWorkExperienceService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public WorkExperienceService(IRepositoryWrapper repositoryWrapper,
                             IMapper mapper,
                             UserManager<AppUser> userManager,
                             IHttpContextAccessor httpContextAccessor)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<List<WorkExperienceResponseDto>>> GetUserWorkExperiencesAsync(string username)
        {
            var sr = new ServiceResponse<List<WorkExperienceResponseDto>>();
            
            //get user
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {
                //user does not exist.
                return sr.HelperMethod(404, $"Unable to find user with username '{username}'.", false);
            }

            var userWorkExperiences = await _repositoryWrapper.WorkExperience.GetAllForUserAsync(foundUser.Id);

            sr.Code = StatusCodes.Status200OK;
            sr.Data = _mapper.Map<List<WorkExperienceResponseDto>>(userWorkExperiences);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<WorkExperienceResponseDto>> GetByIdAsync(int id)
        {
            var workExperience = await _repositoryWrapper.WorkExperience.GetByIdAsync(id);
            var sr = new ServiceResponse<WorkExperienceResponseDto>();
            if (workExperience != null)
            {
                sr.Code = StatusCodes.Status200OK;
                sr.Data = _mapper.Map<WorkExperienceResponseDto>(workExperience);
                sr.Success = true;
            }
            else
            {
                sr.HelperMethod(StatusCodes.Status404NotFound, "Work experience does not exist.", false);
            }
            return sr;
        }

        public async Task<ServiceResponse<WorkExperienceResponseDto>> AddAsync(WorkExperienceDto workExperience)
        {
            var sr = new ServiceResponse<WorkExperienceResponseDto>();
            List<string> errorMessages = new List<string>();

            //perform validation
            if (!IsValidWorkExperience(workExperience, out errorMessages))
            {
                return sr.HelperMethod(400, string.Join(",", errorMessages), false);
            }

            //get user
            var foundUser = await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.GetUsernameOfCurrentUser());
            if (foundUser == null)
            {
                //user does not exist.
                return sr.HelperMethod(404, "user not found.", false);
            }

            if (workExperience.CurrentlyWorking)
            {
                //user is currently working
                //set end year to 0.
                workExperience.EndYear = 0;
            }

            //convert/map
            var newWorkExperience = _mapper.Map<WorkExperience>(workExperience);
            //replace username with id
            newWorkExperience.UserId = foundUser.Id;

            //insert new work experience
            await _repositoryWrapper.WorkExperience.InsertAsync(newWorkExperience);
            await _repositoryWrapper.SaveChangesAsync();

            sr.Code = 201;
            sr.Success = true;
            sr.Data = _mapper.Map<WorkExperienceResponseDto>(newWorkExperience);
            return sr;
        }

        public async Task<ServiceResponse<WorkExperienceResponseDto>> UpdateAsync(int workExperienceId, WorkExperienceDto workExperience)
        {
            var sr = new ServiceResponse<WorkExperienceResponseDto>();
            List<string> errorMessages = new List<string>();

            //perform validation
            if (!IsValidWorkExperience(workExperience, out errorMessages))
            {
                return sr.HelperMethod(400, string.Join(",", errorMessages), false);
            }

            //get user work experience.
            var userWorkExperience = await _repositoryWrapper.WorkExperience.GetByIdAsync(workExperienceId);
            if (userWorkExperience == null)
            {
                //work experience does not exist.
                return sr.HelperMethod(StatusCodes.Status404NotFound, "No work experience record found.", false);
            }

            //verify user
            if (userWorkExperience.User.UserName != _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser())
            {
                //user does not exist.
                return sr.HelperMethod(StatusCodes.Status403Forbidden, "You don't have the permission to perform this action.", false);
            }

            //if (workExperience.CurrentlyWorking)
            //{
            //    //user is currently working
            //    //set end year to 0.
            //    workExperience.EndYear = 0;
            //}

            //convert/map
            userWorkExperience.CompanyName = workExperience.CompanyName;
            userWorkExperience.JobTitle = workExperience.JobTitle;
            userWorkExperience.Location = workExperience.Location;
            userWorkExperience.EmploymentType = workExperience.EmploymentType;
            userWorkExperience.StartYear = workExperience.StartYear;
            userWorkExperience.EndYear = workExperience.EndYear;
            userWorkExperience.CurrentlyWorking = workExperience.CurrentlyWorking;

            _repositoryWrapper.WorkExperience.Update(userWorkExperience);
            await _repositoryWrapper.SaveChangesAsync();

            //sr.Success = true;
            //sr.Code = 200;
            sr.Data = _mapper.Map<WorkExperienceResponseDto>(userWorkExperience);
            return sr;
        }

        public async Task<ServiceResponse<WorkExperienceResponseDto>> DeleteWorkExperienceAsync(int workExperienceId)
        {
            var sr = new ServiceResponse<WorkExperienceResponseDto>();
            var workExperience = await _repositoryWrapper.WorkExperience.GetByIdAsync(workExperienceId);
            _repositoryWrapper.WorkExperience.Delete(workExperience);
            await _repositoryWrapper.SaveChangesAsync();

            sr.HelperMethod(StatusCodes.Status200OK, "The work experience was successfully deleted.", true);
            return sr;
        }

        private bool IsValidWorkExperience(WorkExperienceDto workExperience, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            if (workExperience.StartYear == 0)
            {
                errorMessages.Add("Please select a valid start year.");
            }
            if (workExperience.StartYear == System.DateTime.Now.Year)
            {
                errorMessages.Add("Please select a valid start year. A valid work experience must be at least 1 year.");
            }
            if (workExperience.EndYear > 0)
            {
                if (workExperience.CurrentlyWorking)
                {
                    errorMessages.Add("Please mark 'currentlyWorking' as false if you've ended your contract with the company.");
                }
            }
            if (!workExperience.CurrentlyWorking)
            {
                //Check start date and end date for any discrepancy.
                if (workExperience.StartYear > workExperience.EndYear)
                {
                    errorMessages.Add("Start year cannot be greater than end year.");
                }
                if (workExperience.StartYear == workExperience.EndYear)
                {
                    //work experience sould at least be for a year.
                    errorMessages.Add("End year cannot be the same as start year.");
                }
                if (workExperience.EndYear > System.DateTime.Now.Year)
                {
                    errorMessages.Add("A valid End Year cannot be greater than the current year.");
                }
            }

            if (errorMessages.Any())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
