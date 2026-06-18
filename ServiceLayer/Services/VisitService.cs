using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using ServiceLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class VisitService
    {
        private readonly InformaticTextBookContext _context;

        public VisitService(InformaticTextBookContext context)
        {
            _context = context;
        }

        public async Task<List<Visit>?> GetUserVisits(int userId)
        {
            return await _context.Visits
                .Include(v => v.Lection).ThenInclude(l => l.Theme)
                .Where(v => v.UserId == userId)
                .ToListAsync();
        }

        public async Task<Visit> CreateOrGetVisitAsync(int userId, int lectionId)
        {
            var visit = await _context.Visits
                .FirstOrDefaultAsync(v => v.UserId == userId && v.LectionId == lectionId);

            if (visit == null)
            {
                visit = new Visit
                {
                    UserId = userId,
                    LectionId = lectionId,
                    VisitTime = 0
                };
                _context.Visits.Add(visit);
                await _context.SaveChangesAsync();
            }

            return visit;
        }

        // обновить время
        public async Task UpdateVisitTimeAsync(int userId, int lectionId, int seconds)
        {
            var visit = await _context.Visits
                .FirstOrDefaultAsync(v => v.UserId == userId && v.LectionId == lectionId);

            if (visit != null)
            {
                visit.VisitTime = (visit.VisitTime ?? 0) + seconds;
                await _context.SaveChangesAsync();
            }
        }
    }
}
