using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Models
{
    public class TypingResult
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Speed { get; set; }
        public decimal Accuracy { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime DateRecorded { get; set; }
        public virtual User User { get; set; } = null!;
        public string? Difficulty { get; set; } = "Normal";
    }
}
