using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WeatherAlertAPI.Models;

namespace WeatherAlertAPI.Services
{
    public class HypermediaProblemDetailsFactory : ProblemDetailsFactory
    {
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            var errorResponse = new ErrorResponse(
                code: type?.Split('/').Last() ?? "ERROR",
                message: detail ?? title ?? "An error occurred"
            );

            errorResponse.AddLink("documentation", $"/docs/errors/{errorResponse.Error.Code}");
            errorResponse.AddLink("support", "https://weatheralert.com/support");

            if (statusCode == StatusCodes.Status404NotFound)
            {
                errorResponse.AddLink("home", "/");
            }

            return new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance,
                Extensions = { ["hypermedia"] = errorResponse }
            };
        }

        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            var errors = modelStateDictionary
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var errorResponse = new ErrorResponse(
                code: "VALIDATION_ERROR",
                message: "One or more validation errors occurred"
            );

            errorResponse.AddLink("documentation", "/docs/errors/VALIDATION_ERROR");
            errorResponse.AddLink("support", "https://weatheralert.com/support");

            return new ValidationProblemDetails(modelStateDictionary)
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance,
                Extensions = { ["hypermedia"] = errorResponse }
            };
        }
    }
}
