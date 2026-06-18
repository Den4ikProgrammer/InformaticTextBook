using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class Lection
{
    public int LectionId { get; set; }

    public int? LectionPhotoId { get; set; }

    public string LectionName { get; set; } = null!;

    public string LectionText { get; set; } = null!;

    public DateOnly LectionDate { get; set; }

    public int ThemeId { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();

    public virtual Theme Theme { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
