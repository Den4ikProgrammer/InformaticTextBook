using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using ServiceLayer.Models;

namespace ServiceLayer.Services
{
    public class QuestionsResultsService
    {
        private readonly InformaticTextBookContext _context;

        public QuestionsResultsService(InformaticTextBookContext context)
        {
            _context = context;
        }

        public async Task AddTestResults(List<QuestionsResult> questionsResults)
        {
            foreach (var result in questionsResults)
            {
                // Проверяем, существует ли уже запись с таким QuestionId и UserId
                var existingResult = await _context.QuestionsResults
                    .FirstOrDefaultAsync(qr => qr.QuestionId == result.QuestionId && qr.UserId == result.UserId);

                if (existingResult != null)
                {
                    // Обновляем существующую запись
                    existingResult.IsRightAnswer = result.IsRightAnswer;
                    _context.QuestionsResults.Update(existingResult);
                }
                else
                {
                    // Добавляем новую запись
                    await _context.QuestionsResults.AddAsync(result);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<QuestionsResult>?> GetUserResults(int userId)
        {
            return await _context.QuestionsResults
                .Include(qr => qr.Question).ThenInclude(q => q.Test).ThenInclude(t => t.Lection)
                .Where(qr => qr.UserId == userId)
                .ToListAsync();
        }
    }
}
