using MindPlaceApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<AppUser>> GetAdministrativeUsersAsync();
        Task<IEnumerable<AppUser>> GetNonAdministrativeUsersAsync(string filterString);
        void DeleteUsers(IEnumerable<AppUser> users);
        Task<List<string>> GetNonAdministrativeUserEmailAsync();
        IQueryable<AppUser> Find(Expression<Func<AppUser, bool>> predicate);
    }

    public class UserRepository : IUserRepository
    {
        protected readonly IdentityAppContext _context;
        public UserRepository(IdentityAppContext context) {
            _context = context;
        }

        public IQueryable<AppUser> Find(Expression<Func<AppUser, bool>> predicate)
        {
            return _context.Set<AppUser>().Where(predicate);
        }

        public async Task<IEnumerable<AppUser>> GetAdministrativeUsersAsync()
        {
            return await _context.Set<AppUser>()
                                .Where(u => u.UserRoles.Any(r => r.Role.Name == "Moderator") && u.IsActive)
                                .ToListAsync();
        }

        public async Task<IEnumerable<AppUser>> GetNonAdministrativeUsersAsync(string filterString)
        {
            var query = _context.Set<AppUser>()
                                .Where(u => u.UserRoles.Any(r => r.Role.Name == "Mentor" || r.Role.Name == "Mentee") && u.IsActive);

            if (!string.IsNullOrWhiteSpace(filterString))
            {
                query = query.Where(u => u.Email.Contains(filterString) 
                                        || u.FirstName.Contains(filterString) 
                                        || u.LastName.Contains(filterString));
            }

            return await query.ToListAsync();
        }

        public async Task<List<string>> GetNonAdministrativeUserEmailAsync()
        {
            return await _context.Set<AppUser>()
                                .Where(u => u.UserRoles.Any(r => r.Role.Name == "Mentor" || r.Role.Name == "Mentee") && u.IsActive)
                                .Select(u => u.Email)
                                .AsNoTracking()
                                .ToListAsync();
        }

        public void DeleteUsers(IEnumerable<AppUser> users)
        {
            _context.Set<AppUser>().RemoveRange(users);
        }
    }
}
