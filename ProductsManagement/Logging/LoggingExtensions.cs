namespace ProductsManagement.Logging;

public static class LoggingExtensions
{
    public static void LogProductCreationMetrics(this ILogger logger, ProductCreationMetrics m)
    {
        logger.LogInformation(LogEvents.ProductCreationCompleted,
            "Product creation metrics: OperationId={OperationId} Name={Name} SKU={SKU} Category={Category} ValidationMs={ValidationMs} DBMs={DbMs} TotalMs={TotalMs} Success={Success} Error={Error}",
            m.OperationId, m.ProductName, m.SKU, m.Category, m.ValidationDuration.TotalMilliseconds, m.DatabaseSaveDuration.TotalMilliseconds, m.TotalDuration.TotalMilliseconds, m.Success, m.ErrorReason);
    }
}