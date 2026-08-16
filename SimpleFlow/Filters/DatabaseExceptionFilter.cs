using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace SimpleFlow.Filters;

/// <summary>
/// Глобальный фильтр необработанных ошибок доступа к базе данных.
/// </summary>
/// <param name="logger">Сервис журналирования.</param>
public sealed class DatabaseExceptionFilter(ILogger<DatabaseExceptionFilter> logger)
    : IAsyncExceptionFilter
{
    /// <summary>
    /// Обрабатывает необработанное исключение базы данных.
    /// </summary>
    /// <param name="context">Контекст выполнения фильтра исключений.</param>
    /// <returns>Завершённая асинхронная операция.</returns>
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not DbException and not DbUpdateException)
            return Task.CompletedTask;

        logger.LogError(context.Exception, "An unhandled database operation failed.");

        context.Result = new ViewResult
        {
            ViewName = "/Views/Shared/DatabaseError.cshtml",
            StatusCode = StatusCodes.Status503ServiceUnavailable
        };
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
