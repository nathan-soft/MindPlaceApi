using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos
{
    public class QuestionDto
    {
        [Required, StringLength(100, MinimumLength = 5, ErrorMessage = "A question title is expected to contain at least 5 characters and not more than 100 characters.")]
        public string Title { get; set; }
        [Required, StringLength(5000, MinimumLength = 10, ErrorMessage = "A question is expected to contain at least 10 characters and not more than 5000 characters.")]
        public string Content { get; set; }
    }

    public class ForumQuestionDto : QuestionDto
    {
        //[MinLength(1, ErrorMessage = "At least one tag is required.")]
        //[MaxLength(3, ErrorMessage = "Maximum number of tags allowed is 3")]
        //public List<string> Tags { get; set; }
    }

    public class CommentDto
    {
        [Required, StringLength(200, MinimumLength = 1, ErrorMessage = "A comment is expected to contain at least a character and not more than 200 characters.")]
        public string Content { get; set; }
        
    }
}
