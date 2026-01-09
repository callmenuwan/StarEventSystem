namespace StarEventSystem.Models
{
    public class OrganizerEventReportViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
