using MindPlaceApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entityCollection);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetAll();
        Task<T> GetByIdAsync(int id);
        Task InsertAsync(T entity);
        void Update(T entity);
    }

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly IdentityAppContext context;
        private DbSet<T> entities;
        string errorMessage = string.Empty;
        public GenericRepository(IdentityAppContext context)
        {
            this.context = context;
            entities = context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return entities;
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return entities.Where(predicate);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            //return entities.SingleOrDefault(s => s.Id == id);
            return await entities.FindAsync(id);
        }

        public async Task InsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            await entities.AddAsync(entity);
        }
        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
        }
        public void Delete(T entity)
        {
            //T entity = entities.SingleOrDefault(s => s.Id == id);
            //T entity = await entities.FindAsync(id);
            entities.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entityCollection)
        {
            //T entity = entities.SingleOrDefault(s => s.Id == id);
            //T entity = await entities.FindAsync(id);
            entities.RemoveRange(entityCollection);
        }
    }
}