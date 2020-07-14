using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PQS.SeedApp.Api
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment _envirioment;
        private readonly ILogger<HttpGlobalExceptionFilter> _log;

        public HttpGlobalExceptionFilter(IHostEnvironment env, ILogger<HttpGlobalExceptionFilter> log)
        {
            this._envirioment = env;
            this._log = log;
        }

        public void OnException(ExceptionContext context)
        {
            if (context == null)
            {
                return;
            }

            _log.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);


            if (context.Exception is FluentValidation.ValidationException)
            {
                // maneja los errores de validacion
                var pobjValidationException = context.Exception as FluentValidation.ValidationException;

                var problemDetails = new ValidationProblemDetails()
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation exception",
                    Detail = pobjValidationException.Message
                };


                // agrupa los errores por proiedad y los agrega al detalle
                foreach (var errorGropup in pobjValidationException.Errors.GroupBy(e => e.PropertyName))
                {
                    problemDetails.Errors.Add(errorGropup.Key, errorGropup.Select(e => e.ErrorMessage).ToArray());

                }
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Result = new BadRequestObjectResult(problemDetails);

            }
            /*
             else if (context.Exception is DbUpdateException)
             {
                 var pobjEntityException = context.Exception as DbUpdateException;

                 // maneja excepciones PQS 
                 var problemDetails = new ProblemDetails()
                 {
                     Instance = context.HttpContext.Request.Path,
                     Status = StatusCodes.Status500InternalServerError,
                     Title = "Error updating the database",
                     Detail = Tools.LastExceptionMessage(pobjEntityException)
                 };

                 if (_envirioment.IsDevelopment())
                 {
                     problemDetails.Extensions.Add("errors", pobjEntityException.InnerException);
                 }
                 context.Result = new InternalServerErrorObjectResult(problemDetails);
                 context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                 _log.LogError($"DB Exception in {context.HttpContext.Request.Path}", context.Exception);
             }*/

            else
            {
                var json = new JsonErrorResponse
                {
                    Messages = new[] { "An error ocurred." }
                };

                if (_envirioment.IsDevelopment())
                {
                    json.DeveloperMessage = context.Exception;
                }

                context.Result = new InternalServerErrorObjectResult(json);
                context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                _log.LogError($"Unhandleld exception in {context.HttpContext.Request.Path}", context.Exception);
            }


            context.ExceptionHandled = true;
        }

        private class JsonErrorResponse
        {
            public string[] Messages { get; set; }

            public object DeveloperMessage { get; set; }
        }
        public class InternalServerErrorObjectResult : ObjectResult
        {
            public InternalServerErrorObjectResult(object error)
                : base(error)
            {
                StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}
