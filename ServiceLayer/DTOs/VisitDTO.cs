using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class VisitDTO
    {
        public string LectionName { get; set; }
        public int VisitTimeSeconds { get; set; }

        public string DisplayTime
        {
            get
            {
                var ts = TimeSpan.FromSeconds(VisitTimeSeconds);
                if (ts.TotalMinutes >= 1)
                    return $"{ts.Minutes} мин {ts.Seconds} сек";
                else
                    return $"{ts.Seconds} сек";
            }
        }
    }
}
