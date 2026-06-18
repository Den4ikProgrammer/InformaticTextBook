using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class ExportService
    {
        private readonly InformaticTextBookContext _context;

        private int ConvertToGrade(double percentage)
        {
            if (percentage >= 90) return 5;
            if (percentage >= 75) return 4;
            if (percentage >= 60) return 3;
            return 2;
        }

        public ExportService(InformaticTextBookContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Экспорт успеваемости всех студентов в Excel-файл.
        /// </summary>
        /// <param name="filePath">Путь для сохранения (полученный из SaveFileDialog).</param>
        public async Task ExportStudentGradesAsync(string filePath)
        {
            // Получаем всех студентов (RoleId = 2)
            var students = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == 2)
                .ToListAsync();

            // Собираем данные: логин, список пройденных тестов с оценками
            var studentGrades = new List<StudentExportModel>();

            foreach (var student in students)
            {
                var model = new StudentExportModel
                {
                    Login = student.UserLogin,
                    TestResults = new List<TestExportModel>()
                };

                // Результаты тестов
                var results = await _context.QuestionsResults
                    .Include(qr => qr.Question)
                        .ThenInclude(q => q.Test)
                            .ThenInclude(t => t.Lection)
                    .Where(qr => qr.UserId == student.UserId)
                    .ToListAsync();

                var testGroups = results
                    .GroupBy(r => r.Question.Test)
                    .Select(g => new TestExportModel
                    {
                        LectionName = g.Key.Lection?.LectionName ?? $"Тест {g.Key.TestId}",
                        CorrectAnswers = g.Count(r => r.IsRightAnswer),
                        TotalQuestions = g.Count() / 2,   // каждая попытка хранится как запись с IsRightAnswer
                        // Если нужно пересчитать оценку по 5-балльной шкале:
                        // Grade = CalculateGrade(...)
                    })
                    .ToList();

                model.TestResults = testGroups;
                studentGrades.Add(model);
            }

            // Формируем Excel-документ
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Успеваемость");

            // Заголовки
            worksheet.Cell(1, 1).Value = "Студент";
            worksheet.Cell(1, 2).Value = "Лекция (тест)";
            worksheet.Cell(1, 3).Value = "Правильных ответов";
            worksheet.Cell(1, 4).Value = "Всего вопросов";
            worksheet.Cell(1, 5).Value = "Оценка (Балл)";

            // Форматирование заголовков
            var headerRange = worksheet.Range("A1:E1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = 2;
            foreach (var student in studentGrades)
            {
                if (!student.TestResults.Any())
                {
                    // Студент без тестов
                    worksheet.Cell(row, 1).Value = student.Login;
                    worksheet.Cell(row, 2).Value = "Нет тестов";
                    row++;
                    continue;
                }

                foreach (var test in student.TestResults)
                {
                    worksheet.Cell(row, 1).Value = student.Login;
                    worksheet.Cell(row, 2).Value = test.LectionName;
                    worksheet.Cell(row, 3).Value = test.CorrectAnswers;
                    worksheet.Cell(row, 4).Value = test.TotalQuestions;
                    double percentage = test.TotalQuestions > 0 ? (double)test.CorrectAnswers / test.TotalQuestions * 100 : 0;
                    worksheet.Cell(row, 5).Value = ConvertToGrade(percentage);
                    row++;
                }
            }

            // Автоподбор ширины столбцов
            worksheet.Columns().AdjustToContents();

            // Сохраняем файл
            workbook.SaveAs(filePath);
        }
    }


    // Вспомогательные модели для экспорта
    public class StudentExportModel
    {
        public string Login { get; set; }
        public List<TestExportModel> TestResults { get; set; }
    }

    public class TestExportModel
    {
        public string LectionName { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
    }
}
