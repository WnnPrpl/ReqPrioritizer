using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;


namespace ReqPrioritizer
{
    public class PriorityFilter : IAsyncActionFilter
    {
        private readonly IDictionary<string, PriorityConfig> _priorityConfigs;
        private readonly ConcurrentDictionary<string, int> _currentRequests = new();

        public PriorityFilter(IDictionary<string, PriorityConfig> priorityConfigs)
        {
            _priorityConfigs = priorityConfigs;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string priority = GetPriorityFromContext(context) ?? "default";

            if (!_priorityConfigs.TryGetValue(priority, out var config))
            {
                if (!_priorityConfigs.TryGetValue("default", out config))
                {
                    await next();
                    return;
                }
            }

            if (config.MaxConcurrentRequests > 0)
            {
                int current = _currentRequests.GetOrAdd(priority, 0);
                if (current >= config.MaxConcurrentRequests)
                {
                    context.HttpContext.Response.StatusCode = 429;
                    await context.HttpContext.Response.WriteAsync("Too many concurrent requests.");
                    return;
                }
            }

            _currentRequests.AddOrUpdate(priority, 1, (_, val) => val + 1);

            try
            {
                await next();
            }
            finally
            {
                _currentRequests.AddOrUpdate(priority, 0, (_, val) => val > 0 ? val - 1 : 0);
            }
        }

        private string? GetPriorityFromContext(ActionExecutingContext context)
        {
            var cad = context.ActionDescriptor as ControllerActionDescriptor;
            if (cad == null) return null;

            var methodAttr = cad.MethodInfo.GetCustomAttributes(typeof(PriorityAttribute), true)
                               .Cast<PriorityAttribute>()
                               .FirstOrDefault();
            if (methodAttr != null)
                return methodAttr.Priority;

            var classAttr = cad.ControllerTypeInfo.GetCustomAttributes(typeof(PriorityAttribute), true)
                               .Cast<PriorityAttribute>()
                               .FirstOrDefault();
            return classAttr?.Priority;
        }
    }
}
