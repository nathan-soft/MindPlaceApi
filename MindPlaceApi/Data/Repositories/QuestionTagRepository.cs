using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface IQuestionTagRepository : IGenericRepository<QuestionTag>
    {
        List<Tag> GetQuestionTags(int questionId);
    }

    public class QuestionTagRepository : GenericRepository<QuestionTag>, IQuestionTagRepository
    {
        public QuestionTagRepository(IdentityAppContext context) : base(context) { }

        public List<Tag> GetQuestionTags(int questionId)
        {
            return context.QuestionTags.Where(qt => qt.QuestionId == questionId)
                                        .Select(qt => qt.Tag)
                                        .ToList();
        }
    }
}
