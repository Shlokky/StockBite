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
        private const string SupportEmailSessionKey = "SupportTicketEmail";
        private readonly ApplicationDbContext _db;
        private readonly SupportTicketReplyService _supportTicketReplyService;
        private readonly SupportTicketLookupService _supportTicketLookupService;

        public SupportController(
            ApplicationDbContext db,
            IRoleContext roleContext,
            SupportTicketReplyService supportTicketReplyService,
            SupportTicketLookupService supportTicketLookupService)
            : base(roleContext)
        {
            _db = db;
            _supportTicketReplyService = supportTicketReplyService;
            _supportTicketLookupService = supportTicketLookupService;
        }

        public IActionResult ContactUs(string? customerEmail = null)
        {
            var emailToUse = GetCurrentSupportEmail(customerEmail);
            return View(new SupportPageViewModel
            {
                CustomerName = GetCustomerName(),
                CustomerEmail = emailToUse,
                Tickets = GetMyTickets(emailToUse)
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
                model.Tickets = GetMyTickets(model.CustomerEmail);
                return View(model);
            }

            RememberSupportEmail(model.CustomerEmail);

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
            return RedirectToAction(nameof(ContactUs), new { customerEmail = model.CustomerEmail });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FindMyTickets(string customerEmail)
        {
            customerEmail = _supportTicketLookupService.NormalizeEmail(customerEmail);
            RememberSupportEmail(customerEmail);
            return RedirectToAction(nameof(ContactUs), new { customerEmail });
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
                return RedirectToAction(nameof(ContactUs), new { customerEmail = GetCurrentSupportEmail(null) });
            }

            if (!_supportTicketReplyService.AddCustomerReply(ticket, Clean(model.ReplyText)))
            {
                TempData["ErrorMessage"] = "Reply cannot be empty.";
                return RedirectToAction(nameof(ContactUs), new { customerEmail = GetCurrentSupportEmail(null) });
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = $"Reply sent for ticket #{ticket.Id}.";
            return RedirectToAction(nameof(ContactUs), new { customerEmail = GetCurrentSupportEmail(null) });
        }

        private string GetCustomerName()
        {
            if (!CurrentConsumerId.HasValue) return "";

            return _db.Consumers
                .Where(x => x.Id == CurrentConsumerId.Value)
                .Select(x => x.Name)
                .FirstOrDefault() ?? "";
        }

        private string GetCurrentSupportEmail(string? emailFromRequest)
        {
            if (CurrentConsumerId.HasValue)
            {
                var consumerEmail = _db.Consumers
                    .Where(x => x.Id == CurrentConsumerId.Value)
                    .Select(x => x.Email)
                    .FirstOrDefault();

                return _supportTicketLookupService.NormalizeEmail(consumerEmail);
            }

            if (!string.IsNullOrWhiteSpace(emailFromRequest))
            {
                return _supportTicketLookupService.NormalizeEmail(emailFromRequest);
            }

            return _supportTicketLookupService.NormalizeEmail(HttpContext.Session.GetString(SupportEmailSessionKey));
        }

        private void RememberSupportEmail(string email)
        {
            if (CurrentConsumerId.HasValue)
            {
                return;
            }

            HttpContext.Session.SetString(SupportEmailSessionKey, _supportTicketLookupService.NormalizeEmail(email));
        }

        private List<SupportTicket> GetMyTickets(string? customerEmail)
        {
            return _supportTicketLookupService.GetTickets(_db, CurrentConsumerId, customerEmail);
        }

        private static string Clean(string? text)
        {
            return (text ?? "").Trim();
        }
    }
}
