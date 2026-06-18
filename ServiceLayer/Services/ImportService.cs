using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using ServiceLayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace ServiceLayer.Services
{
    public class ImportService
    {
        private readonly InformaticTextBookContext _context;

        public ImportService(InformaticTextBookContext context)
        {
            _context = context;
        }

        // Импорт лекций из JSON
        public async Task<ImportResult> ImportFromDocxAsync(string filePath)
        {
            var result = new ImportResult();
            try
            {
                using var stream = File.OpenRead(filePath);
                using var doc = WordprocessingDocument.Open(stream, false);
                var body = doc.MainDocumentPart?.Document.Body;
                if (body == null)
                {
                    result.Errors.Add("Документ пуст или повреждён");
                    return result;
                }

                var state = new DocxImportState();
                var elements = body.Elements().ToList();

                for (int i = 0; i < elements.Count; i++)
                {
                    var element = elements[i];
                    if (element is not Paragraph para) continue;

                    string text = para.InnerText.Trim();
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    var style = para.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

                    if (style?.Contains("Heading1") == true)
                    {
                        await SaveCurrentLection(result, state);
                        state.ThemeName = text.Replace("Тема:", "").Replace("#", "").Trim();
                    }
                    else if (style?.Contains("Heading2") == true)
                    {
                        await SaveCurrentLection(result, state);
                        state.LectionName = text.Replace("Лекция:", "").Replace("##", "").Trim();
                    }
                    else if (text.StartsWith("Дата:"))
                    {
                        state.LectionDate = text.Replace("Дата:", "").Trim();
                    }
                    else if (text.StartsWith("### Вопросы") || text.StartsWith("Вопросы:"))
                    {
                        var parsedQuestions = ParseQuestions(elements, ref i);
                        state.Questions.AddRange(parsedQuestions);
                    }
                    else
                    {
                        string mdLine = ConvertToMarkdown(para);
                        state.MarkdownContent.AppendLine(mdLine);
                    }
                }

                // Сохраняем последнюю лекцию
                await SaveCurrentLection(result, state);
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Ошибка импорта DOCX: {ex.Message}");
            }
            return result;
        }

        private string ConvertToMarkdown(Paragraph para)
        {
            var sb = new StringBuilder();
            foreach (var run in para.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
            {
                string text = run.InnerText;
                var props = run.RunProperties;
                if (props == null) { sb.Append(text); continue; }

                if (props.Bold != null) text = $"**{text}**";
                if (props.Italic != null) text = $"*{text}*";
                if (props.RunFonts?.Ascii?.Value?.Contains("Consolas") == true ||
                    props.RunFonts?.Ascii?.Value?.Contains("Courier") == true)
                    text = $"`{text}`";

                sb.Append(text);
            }
            return sb.ToString();
        }

        private List<ImportQuestion> ParseQuestions(List<OpenXmlElement> elements, ref int index)
        {
            var questions = new List<ImportQuestion>();

            for (int i = index + 1; i < elements.Count; i++)
            {
                if (elements[i] is not Paragraph para) continue;

                string text = para.InnerText.Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                // Прекращаем, если встретили следующий заголовок
                if (IsHeading(para)) break;

                // Проверяем, что это вопрос (начинается с цифры, точки и пробела)
                bool isQuestion = text.Length > 2 &&
                                  char.IsDigit(text[0]) &&
                                  (text.Contains('.') || text.Contains(')'));

                if (isQuestion)
                {
                    var question = new ImportQuestion
                    {
                        QuestionText = text,
                        Answers = new List<ImportAnswer>()
                    };

                    // Ищем ответы
                    int answersFound = 0;
                    for (int j = i + 1; j < elements.Count && answersFound < 4; j++)
                    {
                        if (elements[j] is not Paragraph answerPara) continue;

                        string ansText = answerPara.InnerText.Trim();
                        if (string.IsNullOrWhiteSpace(ansText)) continue;

                        // Проверяем варианты ответов (могут быть с пробелами)
                        bool isAnswer = (ansText.StartsWith("a)") || ansText.StartsWith("a )") ||
                                        ansText.StartsWith("b)") || ansText.StartsWith("b )") ||
                                        ansText.StartsWith("c)") || ansText.StartsWith("c )") ||
                                        ansText.StartsWith("d)") || ansText.StartsWith("d )"));

                        if (isAnswer)
                        {
                            bool isCorrect = ansText.Contains("(*)") || ansText.Contains("(правильный)");
                            // Убираем метку правильного ответа и скобки варианта
                            ansText = ansText.Replace("(*)", "").Replace("(правильный)", "").Trim();

                            // Убираем префикс a) или a )
                            if (ansText.StartsWith("a)") || ansText.StartsWith("a )"))
                                ansText = ansText[2..].Trim();
                            else if (ansText.StartsWith("b)") || ansText.StartsWith("b )"))
                                ansText = ansText[2..].Trim();
                            else if (ansText.StartsWith("c)") || ansText.StartsWith("c )"))
                                ansText = ansText[2..].Trim();
                            else if (ansText.StartsWith("d)") || ansText.StartsWith("d )"))
                                ansText = ansText[2..].Trim();

                            question.Answers.Add(new ImportAnswer
                            {
                                AnswerText = ansText,
                                IsCorrect = isCorrect
                            });
                            answersFound++;
                            i = j; // обновляем индекс
                        }
                        else if (answersFound > 0)
                        {
                            // Если уже нашли ответы и встретили не-ответ — заканчиваем
                            break;
                        }
                    }

                    // Добавляем вопрос, только если есть хотя бы 2 ответа
                    if (question.Answers.Count >= 2)
                        questions.Add(question);
                }
            }

            return questions;
        }

        private bool IsHeading(Paragraph para)
        {
            var style = para.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
            return style?.Contains("Heading") == true;
        }

        private async Task SaveCurrentLection(ImportResult result, DocxImportState state)
        {
            if (string.IsNullOrEmpty(state.LectionName)) return;

            // Найти или создать тему
            var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeName == state.ThemeName);
            if (theme == null)
            {
                theme = new Theme { ThemeName = state.ThemeName };
                _context.Themes.Add(theme);
                await _context.SaveChangesAsync();
                result.ImportedThemes++;
            }

            // Создать лекцию
            var lection = new Lection
            {
                ThemeId = theme.ThemeId,
                LectionName = state.LectionName,
                LectionText = state.MarkdownContent.ToString().Trim(),
                LectionDate = DateOnly.TryParse(state.LectionDate, out var d) ? d : DateOnly.FromDateTime(DateTime.Today)
            };
            _context.Lections.Add(lection);
            await _context.SaveChangesAsync();
            result.ImportedLections++;

            // Создать тест и вопросы
            if (state.Questions.Any())
            {
                var test = new Test { LectionId = lection.LectionId };
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
                result.ImportedTests++;

                foreach (var q in state.Questions)
                {
                    var question = new Question { TestId = test.TestId, QuestionText = q.QuestionText };
                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync();
                    result.ImportedQuestions++;

                    foreach (var a in q.Answers)
                    {
                        _context.Answers.Add(new Answer { QuestionId = question.QuestionId, AnswerText = a.AnswerText, IsCorrect = a.IsCorrect });
                    }
                    await _context.SaveChangesAsync();
                    result.ImportedAnswers += q.Answers.Count;
                }
            }

            // Очищаем состояние
            state.LectionName = null;
            state.LectionDate = null;
            state.MarkdownContent.Clear();
            state.Questions.Clear();
        }

        public void CreateTemplateDocx(string filePath)
        {
            using var doc = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Заголовок 1
            body.AppendChild(CreateStyledParagraph("Тема: Программирование", "Heading1"));
            body.AppendChild(new Paragraph()); // пустая строка

            // Заголовок 2
            body.AppendChild(CreateStyledParagraph("Лекция: Основы C#", "Heading2"));
            body.AppendChild(CreateStyledParagraph("Дата: 2024-01-15", null));
            body.AppendChild(new Paragraph());

            // Текст лекции с форматированием
            var para = new Paragraph();
            para.AppendChild(new Run(new Text("Язык C# — это ")));
            para.AppendChild(new Run(new Text("объектно-ориентированный")) { RunProperties = new RunProperties(new Bold()) });
            para.AppendChild(new Run(new Text(" язык программирования. Он позволяет создавать надёжные приложения.")));
            body.AppendChild(para);
            body.AppendChild(new Paragraph());

            // Вопросы
            body.AppendChild(CreateStyledParagraph("Вопросы:", "Heading3"));
            body.AppendChild(new Paragraph(new Run(new Text("1. Что такое класс?"))));
            body.AppendChild(new Paragraph(new Run(new Text("   a) Функция"))));
            body.AppendChild(new Paragraph(new Run(new Text("   b) Шаблон для создания объектов (*)"))));
            body.AppendChild(new Paragraph(new Run(new Text("   c) Переменная"))));
            body.AppendChild(new Paragraph(new Run(new Text("   d) Цикл"))));
            body.AppendChild(new Paragraph());

            body.AppendChild(new Paragraph(new Run(new Text("2. Что такое метод в C#?"))));
            body.AppendChild(new Paragraph(new Run(new Text("   a) Блок кода, выполняющий действие (*)"))));
            body.AppendChild(new Paragraph(new Run(new Text("   b) Класс"))));
            body.AppendChild(new Paragraph(new Run(new Text("   c) Свойство"))));
            body.AppendChild(new Paragraph(new Run(new Text("   d) Интерфейс"))));

            mainPart.Document.Save();
        }

        private Paragraph CreateStyledParagraph(string text, string? styleId)
        {
            var para = new Paragraph();
            if (!string.IsNullOrEmpty(styleId))
            {
                para.ParagraphProperties = new ParagraphProperties(
                    new ParagraphStyleId { Val = styleId }
                );
            }
            para.AppendChild(new Run(new Text(text)));
            return para;
        }

        private class DocxImportState
        {
            public string? ThemeName { get; set; }
            public string? LectionName { get; set; }
            public string? LectionDate { get; set; }
            public StringBuilder MarkdownContent { get; set; } = new();
            public List<ImportQuestion> Questions { get; set; } = new();
        }

    }
}