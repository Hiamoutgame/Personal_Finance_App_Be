namespace Personal_Finance_Management.Service.limit;

public class Response
{

    public class GetLimitsResponse
    {
        public List<GetLimitItem> Data { get; set; } = new();
    }

    public class GetLimitItem
    {
        public Guid Id { get; set; }
       
        public required string TargetType { get; set; }
        public Guid TargetId { get; set; }
        public required string TargetName { get; set; }  
        public decimal LimitAmount { get; set; }
        public required string Period { get; set; }
        public decimal AlertAtPercentage { get; set; }
        public decimal CurrentSpent { get; set; }        
        public double CurrentPercentage { get; set; }    
        public required string Status { get; set; }    
    }

    public class CreateLimitResponse
    {
        public Guid Id { get; set; }
        
        public required string TargetType { get; set; }
        public Guid TargetId { get; set; }
        public decimal LimitAmount { get; set; }
        public required string Period { get; set; }
        public decimal AlertAtPercentage { get; set; }
    }

    public class UpdateLimitResponse
    {
        public Guid Id { get; set; }
        public decimal LimitAmount { get; set; }
        public decimal AlertAtPercentage { get; set; }
    }
}