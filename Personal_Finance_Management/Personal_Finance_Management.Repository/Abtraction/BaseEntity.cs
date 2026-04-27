namespace Personal_Finance_Management.Repository.Abtraction;

public abstract class BaseEntity
{
    public Guid Id  { get; set; } // Id dùng Guid thống nhất cho toàn bộ entity
    public bool IsDeleted  { get; set; } // xóa mềm bth là false mà xóa là true
}
