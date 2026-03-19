using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Controllers;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;

namespace StockBite.Controllers
{
    public class SupportController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public SupportController(ApplicationDbContext dbContext, IRoleContext roleContext)
            : base(roleContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult ContactUs()
        {
            var model = BuildSupportPageViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ContactUs(SupportPageViewModel model)
        {
            model.CustomerName = (model.CustomerName ?? "").Trim();
            model.CustomerEmail = (model.CustomerEmail ?? "").Trim();
            model.Subject = (model.Subject ?? "").Trim();
            model.Message = (model.Message ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.CustomerName) ||
                string.IsNullOrWhiteSpace(model.CustomerEmail) ||
                string.IsNullOrWhiteSpace(model.Subject) ||
                string.IsNullOrWhiteSpace(model.Message))
            {
                TempData["ErrorMessage"] = "Fill in all ticket details.";
                model.Tickets = GetVisibleTickets();
                return View(model);
            }

            var ticket = new SupportTicket
            {
                ConsumerId = CurrentConsumerId,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                Subject = model.Subject,
                Message = model.Message
            };

            _dbContext.SupportTickets.Add(ticket);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Your ticket has been created.";
            return RedirectToAction(nameof(ContactUs));
        }

        public IActionResult AdminTickets()
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            var tickets = _dbContext.SupportTickets
                .Include(x => x.Consumer)
                .OrderBy(x => x.IsResolved)
                .ThenByDescending(x => x.CreatedAt)
                .ToList();

            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reply(int id, string adminReply)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            var ticket = _dbContext.SupportTickets.FirstOrDefault(x => x.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            adminReply = (adminReply ?? "").Trim();
            if (string.IsNullOrWhiteSpace(adminReply))
            {
                TempData["ErrorMessage"] = "Reply cannot be empty.";
                return RedirectToAction(nameof(AdminTickets));
            }

            ticket.AdminReply = adminReply;
            ticket.IsResolved = true;
            ticket.RepliedAt = DateTime.Now;
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Reply sent for ticket #{ticket.Id}.";
            return RedirectToAction(nameof(AdminTickets));
        }

        private SupportPageViewModel BuildSupportPageViewModel()
        {
            return new SupportPageViewModel
            {
                CustomerName = GetDefaultCustomerName(),
                Tickets = GetVisibleTickets()
            };
        }

        private string GetDefaultCustomerName()
        {
            if (!CurrentConsumerId.HasValue)
            {
                return "";
            }

            return _dbContext.Consumers
                .Where(x => x.Id == CurrentConsumerId.Value)
                .Select(x => x.Name)
                .FirstOrDefault() ?? "";
        }

        private List<SupportTicket> GetVisibleTickets()
        {
            if (!CurrentConsumerId.HasValue)
            {
                return new List<SupportTicket>();
            }

            return _dbContext.SupportTickets
                .Where(x => x.ConsumerId == CurrentConsumerId.Value)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }
    }
}
