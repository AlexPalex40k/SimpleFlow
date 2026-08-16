using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;

namespace SimpleFlow.Services;

/// <summary>
/// Обработчик ожидаемых ошибок сохранения Entity Framework Core.
/// </summary>
/// <param name="context">Контекст базы данных приложения.</param>
/// <param name="logger">Сервис журналирования.</param>
public sealed class DatabaseOperationService(
    SimpleFlowContext context,
    ILogger<DatabaseOperationService> logger) : IDatabaseOperationService
{
    /// <inheritdoc />
    public async Task<DatabaseOperationResult> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return new DatabaseOperationResult(true);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogWarning(exception, "A database concurrency conflict occurred.");
            return new DatabaseOperationResult(
                false,
                "Запись была изменена или удалена другим пользователем. Обновите страницу и повторите действие.");
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "A database update failed.");
            return new DatabaseOperationResult(false, GetFriendlyMessage(exception));
        }
    }

    private static string GetFriendlyMessage(DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            return sqlException.Number switch
            {
                2601 or 2627 => "Запись с такими уникальными данными уже существует.",
                547 => "Запись нельзя удалить или изменить, потому что она используется в других данных.",
                -2 => "Сервер базы данных не ответил вовремя. Повторите действие позже.",
                _ => "Не удалось сохранить изменения в базе данных. Повторите действие позже."
            };
        }

        return "Не удалось сохранить изменения в базе данных. Повторите действие позже.";
    }
}
