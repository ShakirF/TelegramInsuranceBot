using Domain.Interfaces;
using Newtonsoft.Json;
using System.Net;

namespace TelegramBot.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IUnitOfWork unitOfWork)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                // Error DB log
                var chatId = context.Request.Headers.TryGetValue("X-Telegram-UserId", out var userIdStr) &&
                             long.TryParse(userIdStr, out var userId) ? userId : 0;

                await unitOfWork.Errors.AddAsync(new Domain.Entities.Error
                {
                    TelegramUserId = chatId,
                    Message = ex.Message,
                    StackTrace = ex.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                await unitOfWork.SaveChangesAsync();

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var result = JsonConvert.SerializeObject(new
                {
                    error = "❗ Internal server error occurred. Please try again later."
                });

                await context.Response.WriteAsync(result);
            }
        }
    }

}
