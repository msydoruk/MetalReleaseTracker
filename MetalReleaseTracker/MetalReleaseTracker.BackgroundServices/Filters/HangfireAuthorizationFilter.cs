using Hangfire.Dashboard;

namespace MetalReleaseTracker.BackgroundServices.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
        }
    }
}
