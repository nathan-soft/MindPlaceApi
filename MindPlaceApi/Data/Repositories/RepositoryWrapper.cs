using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface IRepositoryWrapper
    {
        ICommentRepository Comment { get; }
        IFollowRepository Follow { get; }
        INotificationRepository Notification { get; }
        IQualificationRepository Qualification { get; }
        IQuestionRepository Question { get; }
        IQuestionLikeRepository QuestionLike { get; }
        IQuestionTagRepository QuestionTag { get; }
        ITagRepository Tag { get; }
        ITransactionRepository Transaction { get; }
        IUserRepository User { get; }
        IWalletRepository Wallet { get; }
        IWorkExperienceRepository WorkExperience { get; }

        /// <summary>
        /// Saves all the changes made to the database in a particular context.
        /// </summary>
        /// <returns></returns>
        Task SaveChangesAsync();
    }

    public class RepositoryWrapper : IRepositoryWrapper
    {
        private IdentityAppContext _context;
        private ICommentRepository _comment;
        private IFollowRepository _follow;
        private INotificationRepository _notification;
        private IQualificationRepository _qualification;
        private IQuestionRepository _question;
        private IQuestionLikeRepository _questionLike;
        private IQuestionTagRepository _questionTag;
        private ITagRepository _tag;
        private ITransactionRepository _transaction;
        private IUserRepository _user;
        private IWalletRepository _wallet;
        private IWorkExperienceRepository _workExperience;

        public ICommentRepository Comment
        {
            get
            {
                if (_comment == null)
                {
                    _comment = new CommentRepository(_context);
                }
                return _comment;
            }
        }

        public IFollowRepository Follow
        {
            get
            {
                if (_follow == null)
                {
                    _follow = new FollowRepository(_context);
                }
                return _follow;
            }
        }

        public INotificationRepository Notification
        {
            get
            {
                if (_notification == null)
                {
                    _notification = new NotificationRepository(_context);
                }
                return _notification;
            }
        }

        public IQualificationRepository Qualification
        {
            get
            {
                if (_qualification == null)
                {
                    _qualification = new QualificationRepository(_context);
                }
                return _qualification;
            }
        }

        public IQuestionRepository Question
        {
            get
            {
                if (_question == null)
                {
                    _question = new QuestionRepository(_context);
                }
                return _question;
            }
        }

        public IQuestionLikeRepository QuestionLike
        {
            get
            {
                if (_questionLike == null)
                {
                    _questionLike = new QuestionLikeRepository(_context);
                }
                return _questionLike;
            }
        }

        public IQuestionTagRepository QuestionTag
        {
            get
            {
                if (_questionTag == null)
                {
                    _questionTag = new QuestionTagRepository(_context);
                }
                return _questionTag;
            }
        }

        public IUserRepository User
        {
            get
            {
                if (_user == null)
                {
                    _user = new UserRepository(_context);
                }
                return _user;
            }
        }

        public ITagRepository Tag
        {
            get
            {
                if (_tag == null)
                {
                    _tag = new TagRepository(_context);
                }
                return _tag;
            }
        }

        public ITransactionRepository Transaction
        {
            get
            {
                if (_transaction == null)
                {
                    _transaction = new TransactionRepository(_context);
                }
                return _transaction;
            }
        }

        public IWalletRepository Wallet
        {
            get
            {
                if (_wallet == null)
                {
                    _wallet = new WalletRepository(_context);
                }
                return _wallet;
            }
        }

        public IWorkExperienceRepository WorkExperience
        {
            get
            {
                if (_workExperience == null)
                {
                    _workExperience = new WorkExperienceRepository(_context);
                }
                return _workExperience;
            }
        }

        public RepositoryWrapper(IdentityAppContext context)
        {
            _context = context;
        }

        
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
