using Microsoft.EntityFrameworkCore;
using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetCommentsForQuestionAsync(int questionId);
    }

    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        private IdentityAppContext _context;
        public CommentRepository(IdentityAppContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetCommentsForQuestionAsync(int questionId)
        {
            return await _context.Comments.Where(c => c.QuestionId == questionId).ToListAsync();
        }
    }
}
