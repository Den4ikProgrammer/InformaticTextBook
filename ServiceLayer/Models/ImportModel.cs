using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Models
{
    public class ImportLection
    {
        [Required]
        public string ThemeName { get; set; } = null!;

        [Required]
        public string LectionName { get; set; } = null!;

        [Required]
        public string LectionText { get; set; } = null!;

        public DateOnly LectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        // Путь к изображению или имя файла
        public string? ImageFileName { get; set; }
    }

    // Модель для импорта тестов
    public class ImportQuestion
    {
        [Required]
        public string LectionName { get; set; } = null!;

        [Required]
        public string QuestionText { get; set; } = null!;

        [Required]
        public List<ImportAnswer> Answers { get; set; } = new();
    }

    public class ImportAnswer
    {
        [Required]
        public string AnswerText { get; set; } = null!;

        public bool IsCorrect { get; set; }
    }

    // Результат импорта (может быть в этом же файле или отдельно)
    public class ImportResult
    {
        public bool Success { get; set; }
        public int ImportedThemes { get; set; }
        public int ImportedLections { get; set; }
        public int ImportedTests { get; set; }
        public int ImportedQuestions { get; set; }
        public int ImportedAnswers { get; set; }
        public int SkippedLections { get; set; }
        public int SkippedQuestions { get; set; }
        public List<string> Errors { get; set; } = new();

        public string GetSummary()
        {
            return $"Импорт {(Success ? "успешен" : "завершен с ошибками")}\n" +
                   $"Тем: {ImportedThemes}\n" +
                   $"Лекций: {ImportedLections}\n" +
                   $"Тестов: {ImportedTests}\n" +
                   $"Вопросов: {ImportedQuestions}\n" +
                   $"Ответов: {ImportedAnswers}\n" +
                   $"Пропущено (дубликаты): Лекций - {SkippedLections}, Вопросов - {SkippedQuestions}\n" +
                   $"Ошибок: {Errors.Count}";
        }
    }
}
