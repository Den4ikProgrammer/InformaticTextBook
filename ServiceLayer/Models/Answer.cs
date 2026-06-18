using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class Answer
{
    public int AnswerId { get; set; }

    public string AnswerText { get; set; } = null!;

    public int QuestionId { get; set; }

    public bool IsCorrect { get; set; }

    public virtual Question Question { get; set; } = null!;
}
