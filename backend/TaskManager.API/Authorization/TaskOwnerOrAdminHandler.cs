using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace TaskManager.API.Authorization
{
    public class TaskOwnerOrAdminHandler : AuthorizationHandler<TaskOwnerOrAdminRequirement, string>

    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,TaskOwnerOrAdminRequirement requirement,string resourceUserId)
       {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

           if (userId == resourceUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

    }
}