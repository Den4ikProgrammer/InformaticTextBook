using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class Test
{
    public int TestId { get; set; }

    public int LectionId { get; set; }

    public virtual Lection Lection { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
