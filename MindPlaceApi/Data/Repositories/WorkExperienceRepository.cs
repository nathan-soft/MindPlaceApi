using Microsoft.EntityFrameworkCore;
using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface IWorkExperienceRepository : IGenericRepository<WorkExperience>
    {
        Task<List<WorkExperience>> GetAllForUserAsync(int userId);
    }

    public class WorkExperienceRepository : GenericRepository<WorkExperience>, IWorkExperienceRepository
    {
        public WorkExperienceRepository(IdentityAppContext context) : base(context)
        {

        }

        public async Task<List<WorkExperience>> GetAllForUserAsync(int userId)
        {
            return await context.Set<WorkExperience>()
                                .Where(we => we.UserId == userId)
                                .OrderByDescending(qi => qi.CurrentlyWorking)
                                .ThenByDescending(qi => qi.EndYear)
                                .ToListAsync();
        }
    }
}
