using Microsoft.EntityFrameworkCore;
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
        IQueryable<Question> GetPatientQuestions(int patientId);
        IQueryable<Question> GetQuestionsForProfessional(int professionalId);
    }

    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(IdentityAppContext context) : base(context) { }

        public IQueryable<Question> GetForumQuestions()
        {
            return context.Questions.Where(q => q.Type == QuestionType.FORUM.ToString() && q.Status == QuestionStatus.APPROVED.ToString());
        }

        public IQueryable<Question> GetPatientQuestions(int patientId)
        {
            return context.Questions.Where(q => q.UserId == patientId);
        }

        public IQueryable<Question> GetQuestionsForProfessional(int professionalId)
        {
            return context.Questions.Where(q => q.Comments.Any(c => c.UserId == professionalId));
        }
    }
}
