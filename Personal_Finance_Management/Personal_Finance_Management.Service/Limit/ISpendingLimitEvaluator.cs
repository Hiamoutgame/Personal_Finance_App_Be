using Personal_Finance_Management.Repository.Entity;

namespace Personal_Finance_Management.Service.limit;

public interface ISpendingLimitEvaluator
{
    Task EvaluateAsync(Guid userId);
}
