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
                "The record was changed or deleted by another user. Refresh the page and try again.");
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
                2601 or 2627 => "A record with the same unique data already exists.",
                547 => "The record cannot be deleted or changed because it is referenced by other data.",
                -2 => "The database server did not respond in time. Try again later.",
                _ => "The database changes could not be saved. Try again later."
            };
        }

        return "The database changes could not be saved. Try again later.";
    }
}
