namespace ProductsManagement.Logging;


public record ProductCreationMetrics
(
    string OperationId,
    string ProductName,
    string SKU,
    Features.Products.ProductCategory Category,
    TimeSpan ValidationDuration,
    TimeSpan DatabaseSaveDuration,
    TimeSpan TotalDuration,
    bool Success,
    string? ErrorReason
);