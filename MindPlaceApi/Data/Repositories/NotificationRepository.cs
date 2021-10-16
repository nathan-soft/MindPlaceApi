using System;
using System.Threading.Tasks;
using MindPlaceApi.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MindPlaceApi.Data.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        public Task<List<Notification>> GetNotificationsForUserAsync(int userId);
    }

    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(IdentityAppContext context) : base(context) { }

        /*
        *gets notifications for the user it was created for.
        */
        public async Task<List<Notification>> GetNotificationsForUserAsync(int createdForUserId)
        {
            var lastEightWks = DateTime.UtcNow.AddDays(-56);
            return await context.Set<Notification>()
                                .Where(n => n.CreatedForId == createdForUserId && n.CreatedOn >= lastEightWks)
                                .OrderByDescending(notif => notif.CreatedOn)
                                .ToListAsync();
        }
    }
}