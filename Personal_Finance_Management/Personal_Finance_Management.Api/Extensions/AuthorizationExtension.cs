using Personal_Finance_Management.Repository.Enum;
using AppPolicies = Personal_Finance_Management.Service.Common.Constants.Policies;

namespace Personal_Finance_Management.Api.Extensions
{
    public static class AuthorizationExtension
    {
        public static class Policies
        {
            public const string User = AppPolicies.User;
            public const string Admin = AppPolicies.Admin;
        }

        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AppPolicies.Admin, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(AccountRole.Admin.ToString());
                });

                options.AddPolicy(AppPolicies.User, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(AccountRole.User.ToString());
                });
            });
        }
    }
}
