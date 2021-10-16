using Microsoft.EntityFrameworkCore;
using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface IQualificationRepository : IGenericRepository<Qualification>
    {
        Task<List<Qualification>> GetUserQualificationsAsync(int userId);
    }

    public class QualificationRepository : GenericRepository<Qualification>, IQualificationRepository
    {
        public QualificationRepository(IdentityAppContext context) : base(context)
        {

        }

        public async Task<List<Qualification>> GetUserQualificationsAsync(int userId)
        {
            return await context.Set<Qualification>()
                                .Where(q => q.UserId == userId)
                                .OrderByDescending(qi => qi.EndYear)
                                .ToListAsync();
        }
    }
}
