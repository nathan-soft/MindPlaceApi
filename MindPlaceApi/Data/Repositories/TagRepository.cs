using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        Tag GetByName(string tagName);
    }

    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(IdentityAppContext context) : base(context) { }

        public Tag GetByName(string tagName)
        {
            return context.Tags.Where(t => t.Name.ToLower() == tagName.ToLower()).FirstOrDefault();
        }
    }
}
