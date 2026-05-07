using Personal_Finance_Management.Repository.Enum;

namespace Personal_Finance_Management.Api.Extensions
{
    public static class AuthorizationExtension
    {
        public static class Policies
        {
            public const string User = "User";
            public const string Admin = "Admin";
        }

        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Admin, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(AccountRole.Admin.ToString());
                });

                options.AddPolicy(Policies.User, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(AccountRole.User.ToString());
                });
            });
        }
    }
}
