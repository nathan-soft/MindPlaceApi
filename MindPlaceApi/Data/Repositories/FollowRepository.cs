using MindPlaceApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MindPlaceApi.Codes.AppHelper;

namespace MindPlaceApi.Data.Repositories
{
    public interface IFollowRepository : IGenericRepository<Follow>
    {
        /// <summary>
        /// Gets subscription requests made by a patient.
        /// </summary>
        /// <param name="patientId"> the id of the patient</param>
        /// <returns></returns>
        Task<List<Follow>> GetPatientSubscriptionRequestsAsync(int patientId);

        /// <summary>
        /// Gets subscription requests for a professional.
        /// </summary>
        /// <param name="patientId"> the id of the professional</param>
        /// <returns></returns>
        Task<List<Follow>> GetSubscriptionRequestsForProfessionalAsync(int professionalId);
    }
    public class FollowRepository : GenericRepository<Follow>, IFollowRepository
    {
        public FollowRepository(IdentityAppContext context) : base(context) { }

        public async Task<List<Follow>> GetPatientSubscriptionRequestsAsync(int patientId)
        {
            return await context.Set<Follow>()
                                .Where(sr => sr.PatientId == patientId && sr.Status == FollowStatus.PENDING.ToString())
                                .OrderByDescending(sr => sr.CreatedOn)
                                .ToListAsync();
        }

        public async Task<List<Follow>> GetSubscriptionRequestsForProfessionalAsync(int professionalId)
        {
            return await context.Set<Follow>()
                                .Where(sr => sr.ProfessionalId == professionalId && sr.Status == FollowStatus.PENDING.ToString())
                                .OrderByDescending(sr => sr.CreatedOn)
                                .ToListAsync();
        }
    }
}