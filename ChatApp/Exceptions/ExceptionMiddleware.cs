namespace ChatApp.Exceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        private static string GetDeepestMessage(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;

            return ex.Message;
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode = 500;
            string message = GetDeepestMessage(exception);

            switch (exception)
            {
                case NotFoundException:
                    statusCode = 404;
                    break;
                case ConflictException:
                    statusCode = 409;
                    break;
                case ForbiddenException:
                    statusCode = 403;
                    break;
                case BadRequestException:
                    statusCode = 400;
                    break;
                case NotAcceptableException:
                    statusCode = 406;
                    break;
                case UnauthorizedException:
                    statusCode = 401;
                    break;
                default:
                    statusCode = 500;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsJsonAsync(new
            {
                statusCode,
                message
            });

        }
    }
}
