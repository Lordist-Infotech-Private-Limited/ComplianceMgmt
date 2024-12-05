using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Filters
{
    public class ApiExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order => int.MaxValue;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path
                };

                context.Result = new BadRequestObjectResult(problemDetails);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
