using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using ServiceLayer.Models;

namespace ServiceLayer.Services
{
    public class TestService
    {
        private readonly InformaticTextBookContext _context;

        public TestService(InformaticTextBookContext context)
        {
            _context = context;
        }

        public async Task<Test?> GetTestById(int testId)    
        {
            return await _context.Tests
                .Include(t => t.Lection)
                .FirstOrDefaultAsync(t => t.TestId == testId);
        }

        public async Task<Test> GetTestByLectionIdAsync(int lectionId, int numberOfQuestions = 10)
        {
            var test = await _context.Tests
                .Include(t => t.Lection)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(t => t.LectionId == lectionId);

            if (test != null)
            {
                // Берем случайные N вопросов
                test.Questions = test.Questions
                    .OrderBy(q => Guid.NewGuid()) // Случайная сортировка
                    .Take(numberOfQuestions)
                    .ToList();

                // Перемешиваем ответы для каждого вопроса
                foreach (var question in test.Questions)
                {
                    question.Answers = question.Answers
                        .OrderBy(a => Guid.NewGuid())
                        .ToList();
                }
            }

            return test;
        }

        public async Task<string> GetTestNameAsync(Test test)
        {
            return "Тест по лекции: " + (await _context.Lections.FirstOrDefaultAsync(l => l.LectionId == test.LectionId)).LectionName;
        }

        public async Task<List<Question>> GetTestQuestionsAsync(int testId)
        {
            return await _context.Questions.Where(q=>q.TestId == testId).ToListAsync();
        }

        public async Task<List<Question>> GetQuestionsWithResults(int testId, int userId)
        {
            return await _context.Questions
                .Where(q => q.TestId == testId)
                .Include(q => q.QuestionsResults
                .Where(qr => qr.UserId == userId))
                .ToListAsync();
        }
    }
}
