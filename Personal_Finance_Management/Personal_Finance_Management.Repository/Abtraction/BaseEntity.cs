namespace Personal_Finance_Management.Repository.Abtraction;

public abstract class BaseEntity<TKey>
{
    public TKey Id  { get; set; } // Id thôi tùy loại dữ liệu mình chọn
    public bool IsDeleted  { get; set; } // xóa mềm bth là false mà xóa là true
}