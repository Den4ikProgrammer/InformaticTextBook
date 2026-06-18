using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int TestId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<QuestionsResult> QuestionsResults { get; set; } = new List<QuestionsResult>();

    public virtual Test Test { get; set; } = null!;
}
