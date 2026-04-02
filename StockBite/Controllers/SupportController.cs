using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;
using StockBite.ViewModels;

namespace StockBite.Controllers
{
    public class SupportController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly SupportTicketReplyService _supportTicketReplyService;

        public SupportController(
            ApplicationDbContext db,
            IRoleContext roleContext,
            SupportTicketReplyService supportTicketReplyService)
            : base(roleContext)
        {
            _db = db;
            _supportTicketReplyService = supportTicketReplyService;
        }

        public IActionResult ContactUs()
        {
            return View(new SupportPageViewModel
            {
                CustomerName = GetCustomerName(),
                Tickets = GetMyTickets()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ContactUs(SupportPageViewModel model)
        {
            model.CustomerName = Clean(model.CustomerName);
            model.CustomerEmail = Clean(model.CustomerEmail);
            model.Subject = Clean(model.Subject);
            model.Message = Clean(model.Message);

            if (model.CustomerName == "" || model.CustomerEmail == "" || model.Subject == "" ||
model.Message == "")
            {
                TempData["ErrorMessage"] = "Fill in all ticket details.";
                model.Tickets = GetMyTickets();
                return View(model);
            }

            _db.SupportTickets.Add(new SupportTicket
            {
                ConsumerId = CurrentConsumerId,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                Subject = model.Subject,
                Message = model.Message
            });

            _db.SaveChanges();
            TempData["SuccessMessage"] = "Your ticket has been created.";
            return RedirectToAction(nameof(ContactUs));
        }

        public IActionResult AdminTickets()
        {
            if (!IsAuthorized(UserRole.Admin)) return RedirectToUnauthorized();

            var tickets = _db.SupportTickets
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
            if (!IsAuthorized(UserRole.Admin)) return RedirectToUnauthorized();

            var ticket = _db.SupportTickets.FirstOrDefault(x => x.Id == id);
            if (ticket == null) return NotFound();

            adminReply = Clean(adminReply);
            if (!_supportTicketReplyService.AddAdminReply(ticket, adminReply))
            {
                TempData["ErrorMessage"] = "Reply cannot be empty.";
                return RedirectToAction(nameof(AdminTickets));
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = $"Reply sent for ticket #{ticket.Id}.";
            return RedirectToAction(nameof(AdminTickets));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerReply(SupportReplyViewModel model)
        {
            if (!IsAuthorized(UserRole.Consumer))
            {
                return RedirectToUnauthorized();
            }

            var ticket = _db.SupportTickets.FirstOrDefault(x => x.Id == model.Id && x.ConsumerId == CurrentConsumerId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(ticket.AdminReply))
            {
                TempData["ErrorMessage"] = "Wait for the admin reply before sending your follow-up.";
                return RedirectToAction(nameof(ContactUs));
            }

            if (!_supportTicketReplyService.AddCustomerReply(ticket, Clean(model.ReplyText)))
            {
                TempData["ErrorMessage"] = "Reply cannot be empty.";
                return RedirectToAction(nameof(ContactUs));
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = $"Reply sent for ticket #{ticket.Id}.";
            return RedirectToAction(nameof(ContactUs));
        }

        private string GetCustomerName()
        {
            if (!CurrentConsumerId.HasValue) return "";

            return _db.Consumers
                .Where(x => x.Id == CurrentConsumerId.Value)
                .Select(x => x.Name)
                .FirstOrDefault() ?? "";
        }

        private List<SupportTicket> GetMyTickets()
        {
            if (!CurrentConsumerId.HasValue) return new List<SupportTicket>();

            return _db.SupportTickets
                .Where(x => x.ConsumerId == CurrentConsumerId.Value)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        private static string Clean(string? text)
        {
            return (text ?? "").Trim();
        }
    }
}
