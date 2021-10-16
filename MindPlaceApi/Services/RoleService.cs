using AutoMapper;
using LifeLongApi.Dtos;
using MindPlaceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MindPlaceApi.Codes;

namespace MindPlaceApi.Services
{
    public interface IRoleService
    {
        Task<ServiceResponse<string>> DeleteRoleAsync(string roleName);
        Task<ServiceResponse<List<RoleDto>>> GetAdminRolesAsync();
        Task<ServiceResponse<RoleDto>> GetRoleByIdAsync(int roleId);
        Task<ServiceResponse<RoleDto>> GetRoleByNameAsync(string roleName);
        Task<ServiceResponse<RoleDto>> NewRoleAsync(string roleName);
        Task<ServiceResponse<RoleDto>> UpdateRoleNameAsync(string oldRoleName, string newRoleName);
    }

    public class RoleService : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        public RoleService(RoleManager<AppRole> roleManager, IMapper mapper)
        {
            _mapper = mapper;
            _roleManager = roleManager;
        }

        ///<summary>
        ///<para>Creates a role.</para>
        ///<param name ="roleName">The name of the role to be created</param>
        ///</summary>
        public async Task<ServiceResponse<RoleDto>> NewRoleAsync(string roleName)
        {
            var sr = new ServiceResponse<RoleDto>();
            //check if role exist.
            var result = await _roleManager.RoleExistsAsync(roleName);
            if (!result)
            {
                //Create role;
                var roleResult = await _roleManager.CreateAsync(new AppRole { Name = roleName });
                var role = await _roleManager.FindByNameAsync(roleName);
                if (!roleResult.Succeeded)
                {
                    //error: role not created 
                    sr.Code = 500;
                    sr.Success = false;
                    sr.Message = roleResult.Errors.ToString();
                }
                else
                {
                    //role created
                    sr.Code = 201;
                    sr.Data = _mapper.Map<RoleDto>(role);
                    sr.Success = true;
                }
            }
            else
            {
                //error: role already in db.
                sr.Code = 409;
                sr.Success = false;
                sr.Message = "A role with the name already exist.";
            }
            return sr;
        }

        /// <summary>
        /// Gets admin level roles from the db
        /// </summary>
        /// <returns>All admin level roles except the "Admin" role.</returns>
        public async Task<ServiceResponse<List<RoleDto>>> GetAdminRolesAsync()
        {
            var roles = await _roleManager.Roles.AsNoTracking()
                                                .Where(r => r.NormalizedName != AppHelper.Roles.ADMIN.ToString()
                                                            && r.NormalizedName != AppHelper.Roles.PROFESSIONAL.ToString()
                                                            && r.NormalizedName != AppHelper.Roles.PATIENT.ToString())
                                                .ToListAsync();
            var sr = new ServiceResponse<List<RoleDto>>();
            sr.Code = 200;
            sr.Data = _mapper.Map<List<RoleDto>>(roles);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<RoleDto>> GetRoleByNameAsync(string roleName)
        {
            var sr = new ServiceResponse<RoleDto>();
            //get the role
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                //error: role not in db.
                sr.Code = 404;
                sr.Success = false;
                sr.Message = $"the role '{roleName}', does not exist.";
            }
            else
            {
                //return the role
                sr.Code = 200;
                sr.Data = _mapper.Map<RoleDto>(role);
                sr.Success = true;
            }
            return sr;
        }

        public async Task<ServiceResponse<RoleDto>> GetRoleByIdAsync(int roleId)
        {
            var sr = new ServiceResponse<RoleDto>();
            //get the role
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                //error: role not in db.
                sr.Code = 404;
                sr.Success = false;
                sr.Message = $"the role does not exist.";
            }
            else
            {
                //get the role
                sr.Code = 200;
                sr.Data = _mapper.Map<RoleDto>(role);
                sr.Success = true;
            }
            return sr;
        }

        public async Task<ServiceResponse<RoleDto>> UpdateRoleNameAsync(string oldRoleName, string newRoleName)
        {
            var sr = new ServiceResponse<RoleDto>();
            var foundRole = await GetRoleByNameAsync(oldRoleName);
            if (foundRole.Data == null)
            {
                //error: role not in db.
                sr.Code = 404;
                sr.Success = false;
                sr.Message = foundRole.Message;
            }
            else
            {
                //update role name.
                foundRole.Data.Name = newRoleName;
                var updatedRole = await _roleManager.UpdateAsync(_mapper.Map<AppRole>(foundRole.Data));
                if (!updatedRole.Succeeded)
                {
                    //error: Couldn't perform the update.
                    sr.Code = 500;
                    sr.Success = false;
                    sr.Message = "An Error occurred while trying to update the role, please try again.";
                }
                else
                {
                    //update successful!
                    sr.Code = 200;
                    sr.Success = true;
                    sr.Data = _mapper.Map<RoleDto>(updatedRole);
                }
            }
            return sr;
        }

        public async Task<ServiceResponse<string>> DeleteRoleAsync(string roleName)
        {
            var sr = new ServiceResponse<string>();
            var foundRole = await GetRoleByNameAsync(roleName);
            if (foundRole.Data == null)
            {
                //error: role not in db.
                sr.Code = 404;
                sr.Success = false;
                sr.Message = foundRole.Message;
            }
            else
            {
                //delete role
                var result = await _roleManager.DeleteAsync(_mapper.Map<AppRole>(foundRole));
                if (!result.Succeeded)
                {
                    //error: Couldn't perform the delete.
                    sr.Code = 500;
                    sr.Success = false;
                    sr.Message = "An Error occurred while trying to delete the role, please try again.";
                }
                else
                {
                    //delete successful!
                    sr.Code = 204;
                    sr.Success = true;
                    sr.Message = "The role was deleted successfully";
                }
            }
            return sr;
        }
    }
}