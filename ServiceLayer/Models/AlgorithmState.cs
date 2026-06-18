using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Models
{
    public class AlgorithmState
    {
        public int[] Array { get; set; } = System.Array.Empty<int>();
        public int[] HighlightedIndices { get; set; } = System.Array.Empty<int>();
        public int CurrentLine { get; set; }
        public string Comment { get; set; } = "";
        public bool IsCompleted { get; set; }
    }
}
