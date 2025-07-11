using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using ReqPrioritizer;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class PriorityFilterTests
    {
        [Fact]
        public async Task Allows_Request_If_Under_MaxConcurrentRequests()
        {
            var configs = new Dictionary<string, PriorityConfig>()
            {
                ["default"] = new PriorityConfig
                {
                    Name = "default",
                    MaxConcurrentRequests = 1
                }
            };

            var filter = new PriorityFilter(configs);

            var httpContext = new DefaultHttpContext();
            var actionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = typeof(TestController).GetMethod(nameof(TestController.DefaultAction))!,
                ControllerTypeInfo = typeof(TestController).GetTypeInfo()
            };

            var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), actionDescriptor);
            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new TestController());

            var actionExecuted = false;
            var actionExecutionDelegate = new ActionExecutionDelegate(() =>
            {
                actionExecuted = true;
                return Task.FromResult<ActionExecutedContext>(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new TestController()));
            });

            await filter.OnActionExecutionAsync(actionExecutingContext, actionExecutionDelegate);

            Assert.True(actionExecuted);
            Assert.NotEqual(429, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task Blocks_Request_If_Above_MaxConcurrentRequests()
        {
            var configs = new Dictionary<string, PriorityConfig>()
            {
                ["default"] = new PriorityConfig
                {
                    Name = "default",
                    MaxConcurrentRequests = 1
                }
            };

            var filter = new PriorityFilter(configs);

            var httpContext = new DefaultHttpContext();
            var actionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = typeof(TestController).GetMethod(nameof(TestController.DefaultAction))!,
                ControllerTypeInfo = typeof(TestController).GetTypeInfo()
            };

            var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), actionDescriptor);
            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new TestController());

            var firstCall = true;

            var actionExecutionDelegate = new ActionExecutionDelegate(() =>
            {
                if (firstCall)
                {
                    firstCall = false;
                    filter.GetType()
                        .GetField("_currentRequests", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.GetValue(filter)
                        .As<ConcurrentDictionary<string, int>>()
                        ?.AddOrUpdate("default", 1, (_, val) => val + 1);

                    return Task.FromResult<ActionExecutedContext>(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new TestController()));
                }
                else
                {
                    return Task.FromResult<ActionExecutedContext>(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new TestController()));
                }
            });

            await filter.OnActionExecutionAsync(actionExecutingContext, actionExecutionDelegate);

            var blockedHttpContext = new DefaultHttpContext();
            var blockedActionContext = new Microsoft.AspNetCore.Mvc.ActionContext(blockedHttpContext, new Microsoft.AspNetCore.Routing.RouteData(), actionDescriptor);
            var blockedActionExecutingContext = new ActionExecutingContext(
                blockedActionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new TestController());

            var blocked = false;
            var blockedDelegate = new ActionExecutionDelegate(() =>
            {
                blocked = true;
                return Task.FromResult<ActionExecutedContext>(new ActionExecutedContext(blockedActionContext, new List<IFilterMetadata>(), new TestController()));
            });

            await filter.OnActionExecutionAsync(blockedActionExecutingContext, blockedDelegate);

            Assert.False(blocked);
            Assert.Equal(429, blockedHttpContext.Response.StatusCode);
        }

        public class TestController
        {
            [Priority("default")]
            public void DefaultAction() { }
        }
    }

    public static class ObjectExtensions
    {
        public static T As<T>(this object obj) => (T)obj;
    }


}
