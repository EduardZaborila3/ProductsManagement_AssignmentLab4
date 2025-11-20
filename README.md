# Advanced Product Management API

This project is a robust .NET 9.0 Web API designed to demonstrate advanced techniques in RESTful service development. It focuses on complex validation logic, conditional object mapping, structured logging, and performance telemetry, following the requirements of the **Advanced .NET Assignment (Lab 4)**.

## 🚀 Key Features

### 1. Advanced AutoMapper Patterns
* **Custom Value Resolvers:**
    * `ProductAgeResolver`: Calculates "New Release", "Vintage", etc., based on release date.
    * `PriceFormatterResolver`: Handles currency formatting based on locale.
    * `BrandInitialsResolver`: Generates initials from multi-word brand names.
* **Conditional Mapping:**
    * Applies a **10% discount** automatically for products in the `Home` category.
    * Removes `ImageUrl` for `Home` products (content filtering).

### 2. Complex Validation (FluentValidation)
* **Async Database Checks:** Ensures uniqueness for `SKU` and `Name` + `Brand` combinations.
* **Cross-Field Validation:**
    * Expensive products (>$100) are restricted to low stock (≤20).
    * Electronics must be released within the last 5 years.
* **Business Rules:**
    * Electronics must cost at least $50.00.
    * Daily creation limit of 500 products.
    * Content filtering for "Home" category names (e.g., no "Toxic" items).
* **Custom Attributes:** Implemented `[ValidSKU]` for format validation.

### 3. Observability & Telemetry
* **Structured Logging:** Uses `ILogger` scopes with unique **Operation IDs**.
* **Correlation ID Middleware:** Tracks requests end-to-end via `X-Correlation-ID` header.
* **Performance Metrics:** Logs detailed execution times for Validation, DB Save, and Total Request (Target: <100ms).
* **Custom Log Events:** Uses specific Event IDs (e.g., `2001 Created`, `2002 ValidationFailed`).

### 4. Testing & Quality
* **Integration Tests:** xUnit tests using `InMemoryDatabase` covering:
    * Happy paths (Electronics, Home logic).
    * Validation failures (Duplicate SKU).
    * Conditional mapping verification.
* **Clean Architecture:** Organized by Features (Vertical Slice approach).

---

## 🛠️ Tech Stack

* **.NET 9.0** - ASP.NET Core Web API
* **Entity Framework Core** (In-Memory Database)
* **AutoMapper** - Object-to-object mapping
* **FluentValidation** - Validation rules
* **xUnit & Moq** - Integration Testing
* **Swagger/OpenAPI** - API Documentation

---

## 📂 Project Structure

```text
ProductsManagement/
├── Common/
│   ├── Logging/        # Custom LogEvents, Metrics Records, Extensions
│   ├── Mapping/        # AutoMapper Profiles & Resolvers
│   └── Middleware/     # CorrelationID Middleware
├── Features/
│   └── Products/       # Handlers, DTOs, Entities, Enums
├── Persistence/        # DbContext configuration
├── Validators/         # FluentValidation rules & Custom Attributes
├── Program.cs          # DI Container & Pipeline configuration
└── ProductsManagement.Tests/
    └── CreateProductHandlerIntegrationTests.cs # Integration Tests
