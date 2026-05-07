namespace Personal_Finance_Management.Service.Dashboard;

public class Request
{
    public class GetDashboardRequest
    {
        public string period { get; set; } = "current_month";
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
    }
}