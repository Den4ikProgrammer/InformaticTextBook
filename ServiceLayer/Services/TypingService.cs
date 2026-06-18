using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using ServiceLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ServiceLayer.Services
{
    public class TypingService
    {
        private readonly InformaticTextBookContext _context;
        private readonly Random _random = new();

        // Наборы текстов по уровням сложности
        private readonly Dictionary<string, string[]> _textsByDifficulty = new()
        {
            ["Easy"] = new[]
            {
                "Информатика — это наука о способах обработки информации с помощью компьютеров.",
                "Алгоритм — это последовательность шагов для решения задачи.",
                "Переменная хранит данные, которые может использовать программа.",
                "Цикл позволяет повторять действия несколько раз подряд.",
                "Функция — это блок кода, который выполняет определённую задачу.",
                "Компьютер состоит из процессора, памяти и устройств ввода.",
                "База данных хранит информацию в структурированном виде.",
                "Язык программирования нужен для написания компьютерных программ.",
                "Операционная система управляет ресурсами компьютера.",
                "Интернет — это глобальная сеть для обмена данными."
            },
            ["Normal"] = new[]
            {
                "Компьютер состоит из процессора, оперативной памяти, жёсткого диска и устройств ввода-вывода. Процессор выполняет вычисления, память хранит данные, а устройства позволяют взаимодействовать с пользователем.",
                "Программирование — это процесс создания компьютерных программ. Программист пишет код на языке программирования, который затем компилируется в машинный код и выполняется процессором.",
                "Базы данных используются для хранения и управления большими объёмами информации. Реляционные базы данных организуют данные в таблицы, которые можно связывать между собой с помощью ключей.",
                "Компьютерные сети позволяют устройствам обмениваться данными. Интернет — это глобальная сеть, объединяющая миллионы компьютеров по всему миру.",
                "Алгоритм — это конечная последовательность шагов, выполнение которых приводит к решению задачи. Свойства алгоритма: дискретность, понятность, определённость, конечность и массовость.",
                "Язык C# является объектно-ориентированным языком программирования. Он используется для создания приложений Windows, веб-сайтов и мобильных приложений на платформе .NET."
            },
            ["Hard"] = new[]
            {
                "Объектно-ориентированное программирование — это парадигма, основанная на концепции объектов, которые содержат данные в виде полей и код в виде методов. Основные принципы ООП: инкапсуляция, наследование, полиморфизм и абстракция. Инкапсуляция скрывает внутреннюю реализацию класса, предоставляя только интерфейс для взаимодействия.",
                "В современной информатике алгоритмы сортировки играют фундаментальную роль. Быстрая сортировка (QuickSort) использует стратегию разделяй и властвуй: выбирается опорный элемент, массив делится на элементы меньше и больше опорного, затем рекурсивно сортируются обе части. Средняя сложность O(n log n).",
                 "Компьютерные сети строятся на основе эталонных моделей OSI и TCP/IP. Модель OSI включает семь уровней: от физического до прикладного. Модель TCP/IP состоит из четырёх уровней и лежит в основе Интернета. Протоколы HTTP, FTP, SMTP и DNS обеспечивают работу веб-сайтов, передачу файлов, электронную почту и преобразование доменных имён."
            }
        };

        public TypingService(InformaticTextBookContext context) => _context = context;

        public async Task<string> GetRandomTrainingTextAsync(string difficulty = "Normal")
        {
            // Пробуем взять из лекций
            //var lection = await _context.Lections
            //    .OrderBy(l => Guid.NewGuid())
            //    .FirstOrDefaultAsync();

            //if (lection?.LectionText != null)
            //{
            //    var clean = StripMarkdown(lection.LectionText);
            //    int maxLength = difficulty switch
            //    {
            //        "Easy" => 120,
            //        "Hard" => 600,
            //        _ => 300
            //    };

            //    if (clean.Length > maxLength)
            //    {
            //        int cutIndex = clean.LastIndexOf(' ', maxLength);
            //        if (cutIndex > 0)
            //            clean = clean[..cutIndex];
            //        else
            //            clean = clean[..maxLength];
            //    }

            //    return clean.TrimEnd() + ".";
            //}
            // Запасной набор
            var texts = _textsByDifficulty.GetValueOrDefault(difficulty, _textsByDifficulty["Normal"]);
            return texts[_random.Next(texts.Length)];
        }

        private string StripMarkdown(string md) => string.Join(" ", md.Split('\n')
            .Select(l => System.Text.RegularExpressions.Regex.Replace(l, @"[#*>`\[\]()_~]", "").Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l)));

        public async Task SaveResultAsync(int userId, decimal speed, decimal accuracy, int durationSec, string difficulty = "Normal")
        {
            _context.TypingResults.Add(new TypingResult
            {
                UserId = userId,
                Speed = speed,
                Accuracy = accuracy,
                DurationSeconds = durationSec,
                Difficulty = difficulty ?? "Normal",
                DateRecorded = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<TypingResult>> GetUserHistoryAsync(int userId, int count = 10)
        {
            return await _context.TypingResults
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.DateRecorded)
                .Take(count)
                .ToListAsync();
        }

        public async Task<TypingResult?> GetBestResultAsync(int userId)
        {
            return await _context.TypingResults
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Speed)
                .FirstOrDefaultAsync();
        }
    }
}
