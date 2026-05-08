using Personal_Finance_Management.Repository.Enum;

namespace Personal_Finance_Management.Repository.Constants;

public static class AppRoles
{
    public static class Ids
    {
        public static readonly Guid User = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid Admin = Guid.Parse("00000000-0000-0000-0000-000000000002");
    }

    public static class Codes
    {
        public static readonly string User = AccountRole.User.ToString();
        public static readonly string Admin = AccountRole.Admin.ToString();
    }
}
