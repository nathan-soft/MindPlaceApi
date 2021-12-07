using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Models
{
    public class Question : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [Required, MaxLength(5000)]
        public string Content { get; set; }
        [Required, MaxLength(100)]
        public string Type { get; set; }
        [Required, MaxLength(100)]
        public string Status { get; set; }
        public int ViewCount { get; set; }
        [Required, MaxLength(100)]
        public string ApprovedBy { get; set; }

        public virtual AppUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual List<QuestionTag> QuestionTags { get; set; }
    }

    public class Comment : BaseEntity
    {
        public int Id { get; set; }
        public int? ParentCommentId { get; set; }
        public int UserId { get; set; }
        public int QuestionId { get; set; }
        [Required, MaxLength(200)]
        public string Content { get; set; }

        //public virtual Comment ParentComment { get; set; }
        public virtual Question Question { get; set; }
        public virtual AppUser User { get; set; }

    }

    public class QuestionTag{
        public int QuestionId { get; set; }
        public int TagId { get; set; }
        public virtual Question Question { get; set; }
        public virtual Tag Tag { get; set; }
    }

    public enum QuestionStatus
    {
        APPROVED,
        PENDING
    }

    public enum QuestionType
    {
        STANDARD,
        FORUM
    }

    public class CommentVM
    {
        public string Comment { get; set; }
        public string CommentAuthor { get; set; }
        public DateTime CommentDate { get; set; }
    }

    public class PostVM
    {
        public string PostTitle { get; set; }
        public string PostContent { get; set; }
        public string PostAuthor { get; set; }
        public DateTime PostDate { get; set; }
        List<CommentVM> Comments { get; set; }
    }
}
