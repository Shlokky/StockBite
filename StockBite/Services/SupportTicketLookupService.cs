using StockBite.Data;
using StockBite.Models;

namespace StockBite.Services
{
    public class SupportTicketLookupService
    {
        public string NormalizeEmail(string? email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        public List<SupportTicket> GetTickets(ApplicationDbContext db, int? consumerId, string? email)
        {
            if (consumerId.HasValue)
            {
                return db.SupportTickets
                    .Where(x => x.ConsumerId == consumerId.Value)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList();
            }

            var normalizedEmail = NormalizeEmail(email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return new List<SupportTicket>();
            }

            return db.SupportTickets
                .Where(x => x.CustomerEmail.ToLower() == normalizedEmail)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }
    }
}
