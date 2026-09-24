using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedExpenseTrackerApp.WebMvcApp.Helpers;

namespace SharedExpenseTrackerApp.WebMvcApp.Filters;

public class RequireLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        if (!session.IsAuthenticated())
        {
            // For Axios / XHR requests return 401 JSON so the client can handle it
            var isXhr = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isXhr)
            {
                context.Result = new JsonResult(new { message = "Session expired. Please log in again." })
                {
                    StatusCode = 401
                };
            }
            else
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
            return;
        }
        base.OnActionExecuting(context);
    }
}
