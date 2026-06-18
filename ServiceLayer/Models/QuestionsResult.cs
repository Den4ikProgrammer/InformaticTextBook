using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class QuestionsResult
{
    public int QuestionId { get; set; }

    public int UserId { get; set; }

    public bool IsRightAnswer { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
