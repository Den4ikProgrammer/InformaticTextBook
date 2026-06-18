using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class Visit
{
    public int LectionId { get; set; }

    public int UserId { get; set; }

    public int? VisitTime { get; set; }

    public virtual Lection Lection { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
