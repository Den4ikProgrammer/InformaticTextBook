using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserLogin { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<QuestionsResult> QuestionsResults { get; set; } = new List<QuestionsResult>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
