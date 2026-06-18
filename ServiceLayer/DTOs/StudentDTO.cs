using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ServiceLayer.DTOs
{
    public class StudentDTO : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string UserLogin { get; set; } = null!;
        public string RoleName { get; set; } = "Студент";

        // Статистика
        public int LectionsCount { get; set; }
        public int TestsCount { get; set; }
        public double AverageGrade { get; set; }
        public int TotalTimeSeconds { get; set; }

        // Вычисляемые свойства
        public string BackgroundColor
        {
            get => (UserId % 2 == 0) ? "#FFFDD0" : "#F5F0E1";
        }

        public string TotalTime
        {
            get
            {
                var ts = TimeSpan.FromSeconds(TotalTimeSeconds);
                if (ts.TotalHours >= 1)
                    return $"{(int)ts.TotalHours} ч {ts.Minutes} мин";
                else if (ts.TotalMinutes >= 1)
                    return $"{ts.Minutes} мин {ts.Seconds} сек";
                else
                    return $"{ts.Seconds} сек";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}