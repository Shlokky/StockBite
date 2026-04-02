using StockBite.Models;

namespace StockBite.Services
{
    public class SupportTicketReplyService
    {
        public bool AddAdminReply(SupportTicket ticket, string replyText)
        {
            if (string.IsNullOrWhiteSpace(replyText))
            {
                return false;
            }

            ticket.AdminReply = replyText.Trim();
            ticket.RepliedAt = DateTime.Now;
            ticket.IsResolved = true;

            return true;
        }

        public bool AddCustomerReply(SupportTicket ticket, string replyText)
        {
            if (string.IsNullOrWhiteSpace(replyText))
            {
                return false;
            }

            ticket.CustomerReply = replyText.Trim();
            ticket.CustomerRepliedAt = DateTime.Now;
            ticket.IsResolved = false;

            return true;
        }
    }
}
