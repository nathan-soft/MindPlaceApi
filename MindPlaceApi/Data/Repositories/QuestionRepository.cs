using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        IQueryable<Question> GetForumQuestions();
    }

    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(IdentityAppContext context) : base(context) { }

        public IQueryable<Question> GetForumQuestions()
        {
            return context.Questions.Where(q => q.Type == QuestionType.FORUM.ToString() && q.Status == QuestionStatus.APPROVED.ToString());
        }
    }
}
