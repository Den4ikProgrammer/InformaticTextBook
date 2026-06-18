using System;
using System.Collections.Generic;

namespace ServiceLayer.Models;

public partial class Theme
{
    public int ThemeId { get; set; }

    public string ThemeName { get; set; } = null!;

    public virtual ICollection<Lection> Lections { get; set; } = new List<Lection>();
}
