namespace ProductsManagement.Logging;

public static class LogEvents
{
    public static readonly EventId ProductCreationStarted = new EventId(2001, nameof(ProductCreationStarted));
    public static readonly EventId ProductValidationFailed = new EventId(2002, nameof(ProductValidationFailed));
    public static readonly EventId ProductCreationCompleted = new EventId(2003, nameof(ProductCreationCompleted));
    public static readonly EventId DatabaseOperationStarted = new EventId(2004, nameof(DatabaseOperationStarted));
    public static readonly EventId DatabaseOperationCompleted = new EventId(2005, nameof(DatabaseOperationCompleted));
    public static readonly EventId CacheOperationPerformed = new EventId(2006, nameof(CacheOperationPerformed));
    public static readonly EventId SKUValidationPerformed = new EventId(2007, nameof(SKUValidationPerformed));
    public static readonly EventId StockValidationPerformed = new EventId(2008, nameof(StockValidationPerformed));
}