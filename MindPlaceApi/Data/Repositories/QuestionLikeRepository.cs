using MindPlaceApi.Models;

namespace MindPlaceApi.Data.Repositories
{
    public interface IQuestionLikeRepository : IGenericRepository<QuestionLike>
    {
    }

    public class QuestionLikeRepository : GenericRepository<QuestionLike>, IQuestionLikeRepository
    {
        public QuestionLikeRepository(IdentityAppContext context) : base(context) { }
    }
}
