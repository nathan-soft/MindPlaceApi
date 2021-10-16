using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos.Response
{
    public class QuestionResponseDto
    {
        public int Id { get; set; }
        #nullable disable
        public AbbrvUser User { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CommentCount { get; set; }
    }

    public class ForumQuestionResponseDto : QuestionResponseDto
    {
        public int ViewCount { get; set; }
        public List<string> Tags { get; set; }
    }

    public class AbbrvCommentResponseDto
    {
        public int Id { get; set; }
        public AbbrvUser User { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CommentResponseDto : AbbrvCommentResponseDto
    {
        public int QuestionId { get; set; }
    }
}
