using Microsoft.Extensions.DependencyInjection;

namespace ReqPrioritizer
{
    public static class PriorityExtensions
    {
        public static IServiceCollection AddRequestPrioritizer(this IServiceCollection services, Action<PriorityOptions> configure)
        {
            services.Configure(configure);

            services.AddScoped<PriorityFilter>();

            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                options.Filters.AddService<PriorityFilter>();
            });

            return services;
        }
    }
}
