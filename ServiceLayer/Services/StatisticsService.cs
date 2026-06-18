using DocumentFormat.OpenXml.Office2010.CustomUI;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class StatisticsService
    {
        private readonly InformaticTextBookContext _context;

        public StatisticsService(InformaticTextBookContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает оценки по темам для графика
        /// </summary>
        public async Task<List<ThemeGrade>> GetThemeGradesAsync(int userId)
        {
            var results = await _context.QuestionsResults
                .Include(qr => qr.Question)
                    .ThenInclude(q => q.Test)
                        .ThenInclude(t => t.Lection)
                            .ThenInclude(l => l.Theme)
                .Where(qr => qr.UserId == userId)
                .ToListAsync();

            // Группируем по темам
            var themeGrades = results
                .GroupBy(r => r.Question.Test.Lection.Theme)
                .Select(g =>
                {
                    // Считаем уникальные вопросы (каждый вопрос может быть отвечен несколько раз)
                    var questionAttempts = g.GroupBy(r => r.QuestionId).ToList();
                    int totalQuestions = questionAttempts.Count;
                    int correctAnswers = questionAttempts.Count(qa => qa.Any(r => r.IsRightAnswer));

                    double grade = totalQuestions > 0
                        ? Math.Round((double)correctAnswers / totalQuestions * 5, 1)
                        : 0;

                    return new ThemeGrade
                    {
                        ThemeName = g.Key.ThemeName,
                        Grade = grade,
                        TotalQuestions = totalQuestions,
                        CorrectAnswers = correctAnswers
                    };
                })
                .OrderBy(t => t.ThemeName)
                .ToList();

            return themeGrades;
        }

        /// <summary>
        /// Получает прогресс по тестам (каждый тест отдельно)
        /// </summary>
        public async Task<List<TestGrade>> GetTestGradesAsync(int userId)
        {


            //var results = await _context.QuestionsResults
            //    .Include(qr => qr.Question)
            //        .ThenInclude(q => q.Test)
            //            .ThenInclude(t => t.Lection)
            //    .Where(qr => qr.UserId == userId)
            //    .ToListAsync();

            //var testGrades = results
            //    .GroupBy(r => r.Question.Test)
            //    .Select(g =>
            //    {
            //        var questionAttempts = g.GroupBy(r => r.QuestionId).ToList();
            //        int totalQuestions = questionAttempts.Count;
            //        int correctAnswers = questionAttempts.Count(q => q.First().IsRightAnswer);

            //        double percentage = totalQuestions > 0
            //            ? Math.Round((double)correctAnswers / totalQuestions * 100, 1)
            //            : 0;

            //        return new TestGrade
            //        {
            //            TestName = g.Key.Lection.LectionName,
            //            Percentage = percentage,
            //            TotalQuestions = totalQuestions,
            //            CorrectAnswers = correctAnswers,
            //            ThemeName = g.Key.Lection.Theme.ThemeName
            //        };
            //    })
            //    .OrderBy(t => t.ThemeName)
            //    .ThenBy(t => t.TestName)
            //    .ToList();



            //return testGrades;

            var results = await _context.QuestionsResults
         .Include(qr => qr.Question)
             .ThenInclude(q => q.Test)
                 .ThenInclude(t => t.Lection)
         .Where(qr => qr.UserId == userId)
         .ToListAsync();

            var testGrades = new List<TestGrade>();
            var groupedByTest = results.GroupBy(r => r.Question.Test);

            foreach (var testGroup in groupedByTest)
            {
                int totalQuestions = testGroup.Count();
                int correctAnswers = testGroup.Count(r => r.IsRightAnswer);

                double percentage = totalQuestions > 0
                    ? Math.Round((double)correctAnswers / totalQuestions * 100, 1)
                    : 0;

                testGrades.Add(new TestGrade
                {
                    TestName = testGroup.Key.Lection.LectionName,
                    Percentage = percentage,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswers,
                    ThemeName = testGroup.Key.Lection.Theme.ThemeName
                });
            }

            return testGrades
                .OrderBy(t => t.ThemeName)
                .ThenBy(t => t.TestName)
                .ToList();
        }

        /// <summary>
        /// Общая статистика
        /// </summary>
        public async Task<UserStatistics> GetUserStatisticsAsync(int userId)
        {
            var stats = new UserStatistics();

            // Пройдено тестов
            var results = await _context.QuestionsResults
                .Include(qr => qr.Question)
                .Where(qr => qr.UserId == userId)
                .ToListAsync();

            var testIds = results
                .Select(r => r.Question.TestId)
                .Distinct()
                .ToList();
            stats.TestsPassed = testIds.Count;

            // Прочитано лекций
            var visits = await _context.Visits
                .Where(v => v.UserId == userId)
                .ToListAsync();
            stats.LectionsRead = visits.Count;

            // Общее время
            stats.TotalTimeSeconds = visits.Sum(v => v.VisitTime ?? 0);

            // Средняя оценка
            if (testIds.Any())
            {
                var testGroups = results.GroupBy(r => r.Question.TestId);
                double sumGrades = 0;
                foreach (var test in testGroups)
                {
                    var questions = test.GroupBy(r => r.QuestionId);
                    int correct = questions.Count(q => q.Any(r => r.IsRightAnswer));
                    double grade = (double)correct / questions.Count() * 5;
                    sumGrades += grade;
                }
                stats.AverageGrade = Math.Round(sumGrades / testIds.Count, 1);
            }

            // Тренажёр печати
            var typingResults = await _context.TypingResults
                .Where(t => t.UserId == userId)
                .ToListAsync();
            stats.TypingBestSpeed = typingResults.Any()
                ? typingResults.Max(t => t.Speed)
                : 0;
            stats.TypingAttempts = typingResults.Count;

            return stats;
        }
    }

    // Модели данных
    public class ThemeGrade
    {
        public string ThemeName { get; set; } = "";
        public double Grade { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }

    public class TestGrade
    {
        public string TestName { get; set; } = "";
        public double Percentage { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public string ThemeName { get; set; } = "";
    }

    public class UserStatistics
    {
        public int TestsPassed { get; set; }
        public int LectionsRead { get; set; }
        public int TotalTimeSeconds { get; set; }
        public double AverageGrade { get; set; }
        public decimal TypingBestSpeed { get; set; }
        public int TypingAttempts { get; set; }
    }
}
