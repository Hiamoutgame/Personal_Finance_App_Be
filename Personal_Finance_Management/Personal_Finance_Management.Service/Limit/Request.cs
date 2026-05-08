namespace Personal_Finance_Management.Service.limit;

public class Request
{
    public class CreateLimitRequest
    {
        public required string TargetType { get; set; }

      
        public required Guid TargetId { get; set; }

        public decimal LimitAmount { get; set; }

       
        public required string Period { get; set; }
        
        public decimal AlertAtPercentage { get; set; }
    }

    public class UpdateLimitRequest
    {
      
        public decimal? LimitAmount { get; set; }
        public decimal? AlertAtPercentage { get; set; }
    }
}