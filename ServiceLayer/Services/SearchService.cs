using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class SearchService
    {
        private readonly InformaticTextBookContext _context;

        public SearchService(InformaticTextBookContext context)
        {
            _context = context;
        }

        public async Task<List<SearchResult>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<SearchResult>();

            string searchTerm = query.Trim().ToLower();

            try
            {
                // Получаем ВСЕ лекции для отладки
                var allLections = await _context.Lections
                    .Include(l => l.Theme)
                    .ToListAsync();

                // Фильтруем в памяти для надёжности
                var results = allLections
                    .Where(l =>
                        (l.LectionName != null && l.LectionName.ToLower().Contains(searchTerm)) ||
                        (l.LectionText != null && l.LectionText.ToLower().Contains(searchTerm)))
                    .Select(l => new SearchResult
                    {
                        LectionId = l.LectionId,
                        LectionName = l.LectionName ?? "Без названия",
                        ThemeName = l.Theme?.ThemeName ?? "Без темы",
                        Snippet = GetSnippet(l.LectionText ?? "", searchTerm, 150)
                    })
                    .ToList();

                return results;
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                System.Diagnostics.Debug.WriteLine($"Ошибка поиска: {ex.Message}");
                return new List<SearchResult>();
            }
        }

        private string GetSnippet(string text, string searchTerm, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "[Текст отсутствует]";

            try
            {
                // Очищаем от Markdown
                string cleanText = Regex.Replace(text, @"[#*>`\[\]()_~]", " ").Trim();
                cleanText = Regex.Replace(cleanText, @"\s+", " ");

                if (string.IsNullOrWhiteSpace(cleanText))
                    return "[Текст не содержит читаемых символов]";

                int index = cleanText.ToLower().IndexOf(searchTerm);

                if (index == -1)
                {
                    // Если термин не найден (странно, но бывает из-за Markdown)
                    return cleanText.Length > maxLength
                        ? cleanText.Substring(0, maxLength) + "..."
                        : cleanText;
                }

                int start = Math.Max(0, index - 40);
                int length = Math.Min(maxLength, cleanText.Length - start);
                string snippet = cleanText.Substring(start, length);

                if (start > 0) snippet = "..." + snippet;
                if (start + length < cleanText.Length) snippet = snippet + "...";

                return snippet;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сниппета: {ex.Message}");
                return "[Ошибка обработки текста]";
            }
        }
    }

    public class SearchResult
    {
        public int LectionId { get; set; }
        public string LectionName { get; set; } = "";
        public string ThemeName { get; set; } = "";
        public string Snippet { get; set; } = "";
    }
}
