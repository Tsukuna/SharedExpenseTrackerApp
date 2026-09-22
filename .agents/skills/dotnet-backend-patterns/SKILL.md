---
name: dotnet-backend-patterns
description: Master C#/.NET backend development patterns for building robust APIs, MCP servers, and enterprise applications. Covers async/await, dependency injection, Entity Framework Core, Dapper, configuration, caching, and testing with xUnit. Use when developing .NET backends, reviewing C# code, or designing API architectures.
---

# .NET Backend Development Patterns

Master C#/.NET patterns for building production-grade APIs, MCP servers, and enterprise backends with modern best practices (2024/2025).

## When to Use This Skill

- Developing new .NET Web APIs or MCP servers
- Reviewing C# code for quality and performance
- Designing service architectures with dependency injection
- Writing unit and integration tests
- Optimizing database access with EF Core or Dapper
- Handling errors and implementing resilience patterns

## Core Concepts

### 1. Project Structure (Clean Architecture)

```
src/
├── Domain/                     # Core business logic (no dependencies)
│   ├── Features/               # Combine Business logic + Data Acess + DTOs used as services
|   ├── Interfaces/             # Repository and service interfaces
│   └── Models/                 # Domain entities and value objects
├── Database/                   # Use Database class for Domain layer
│   └── AppDbContextModels/
├── Shared/                     # Shared utilities, DTOs, and abstractions
│   ├── Shared/                   
│       ├── BaseController/     # To inheritance base controller for all Controllers       
│       ├── DapperService/      # Dapper helper methods
│       └── Result/             # Result pattern for error handling
└── Api/                        
    ├── Controllers/            # API controllers (if using MVC)
    └── Program.cs
```

### 2. Dependency Injection Patterns

```csharp
// Service registration by lifetime
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Scoped: One instance per HTTP request
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

        // Singleton: One instance for app lifetime
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration["Redis:Connection"]!));

        // Transient: New instance every time
        services.AddTransient<IValidator<CreateOrderRequest>, CreateOrderValidator>();

        // Options pattern for configuration
        services.Configure<CatalogOptions>(configuration.GetSection("Catalog"));
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));

        // Factory pattern for conditional creation
        services.AddScoped<IPriceCalculator>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PricingOptions>>().Value;
            return options.UseNewEngine
                ? sp.GetRequiredService<NewPriceCalculator>()
                : sp.GetRequiredService<LegacyPriceCalculator>();
        });

        // Keyed services (.NET 8+)
        services.AddKeyedScoped<IPaymentProcessor, StripeProcessor>("stripe");
        services.AddKeyedScoped<IPaymentProcessor, PayPalProcessor>("paypal");

        return services;
    }
}

// Usage for DI
public class CheckoutService
{
    public CheckoutService(
         IPaymentProcessor stripeProcessor)
    {
        _processor = stripeProcessor;
    }
}
```

### 3. Async/Await Patterns

```csharp
// ✅ CORRECT: Async all the way down
public async Task<Product> GetProductAsync(string id, CancellationToken ct = default)
{
    return await _repository.GetByIdAsync(id, ct);
}

// ✅ CORRECT: Parallel execution with WhenAll
public async Task<(Stock, Price)> GetStockAndPriceAsync(
    string productId,
    CancellationToken ct = default)
{
    var stockTask = _stockService.GetAsync(productId, ct);
    var priceTask = _priceService.GetAsync(productId, ct);

    await Task.WhenAll(stockTask, priceTask);

    return (await stockTask, await priceTask);
}

// ✅ CORRECT: ConfigureAwait in libraries
public async Task<T> LibraryMethodAsync<T>(CancellationToken ct = default)
{
    var result = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
    return await result.Content.ReadFromJsonAsync<T>(ct).ConfigureAwait(false);
}

// ✅ CORRECT: ValueTask for hot paths with caching
public ValueTask<Product?> GetCachedProductAsync(string id)
{
    if (_cache.TryGetValue(id, out Product? product))
        return ValueTask.FromResult(product);

    return new ValueTask<Product?>(GetFromDatabaseAsync(id));
}

// ❌ WRONG: Blocking on async (deadlock risk)
var result = GetProductAsync(id).Result;  // NEVER do this
var result2 = GetProductAsync(id).GetAwaiter().GetResult(); // Also bad

// ❌ WRONG: async void (except event handlers)
public async void ProcessOrder() { }  // Exceptions are lost

// ❌ WRONG: Unnecessary Task.Run for already async code
await Task.Run(async () => await GetDataAsync());  // Wastes thread
```

### 4. Result Pattern (Avoiding Exceptions for Flow Control)

```csharp
// Generic Result type
public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsError { get { return !IsSuccess; } }
        private EnumResType Type { get; set; }
        public string Message { get; set; } = null!;
        public T? Data { get; set;  }

        public EnumResType GetEnumType() => Type;

        public static Result<T> Success(T? data, string message = "Success")
        {
            return new Result<T>()
            {
                IsSuccess = true,
                Type = EnumResType.Success,
                Data = data,
                Message = message
            };
        }

        public static Result<T> DeleteSuccess(string message = "Delete success")
        {
            return new Result<T>()
            {
                IsSuccess = true,
                Type = EnumResType.Success,
                Message = message
            };
        }

        public static Result<T> ValidationError(string message, T? data = default)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Type = EnumResType.ValidationError,
                Data = data,
                Message = message
            };
        }

        public static Result<T> SystemError(string message, T? data = default)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Type = EnumResType.SystemError,
                Data = data,
                Message = message
            };
        }


        public static Result<T> NotFound(string message, T? data = default)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Type = EnumResType.NotFound,
                Data = data,
                Message = message
            };
        }
    }

public enum EnumResType
{
    None,
    Success,
    ValidationError,
    SystemError,
    NotFound
}

// Usage in service
 public async Task<Result<AnnouncementRespModel>> CreateAnnouncement(CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = new BDMS.Database.AppDbContextModels.Announcement
            {
                Title = request.Title,
                Content = request.Content,
                Category = request.Category.ToString(),
                IsActive = request.IsActive,
                ExpiredAt = request.ExpiredAt,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _dbContext.Announcements.Add(announcement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var res = new AnnouncementRespModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Category = announcement.Category,
                Content = announcement.Content,
                IsActive = announcement.IsActive,
                ExpiredAt = announcement.ExpiredAt,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt
            };

            return Result<AnnouncementRespModel>.Success(res, "Announcement created successfully.");
        }
        catch (Exception ex)
        {
            return Result<AnnouncementRespModel>.SystemError($"An error occurred while creating the announcement: {ex.Message}");
        }
    }
```

## Data Access Patterns

### Entity Framework Core

```csharp
// DbContext configuration
public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filters
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
    }
}

// Entity configuration
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasMaxLength(40);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => new { p.CategoryId, p.Name });

        builder.HasMany(p => p.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId);
    }
}

// Repository with EF Core
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(
        ProductSearchCriteria criteria,
        CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{criteria.SearchTerm}%"));

        if (criteria.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == criteria.CategoryId);

        if (criteria.MinPrice.HasValue)
            query = query.Where(p => p.Price >= criteria.MinPrice);

        if (criteria.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= criteria.MaxPrice);

        return await query
            .OrderBy(p => p.Name)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(ct);
    }
}
```

### Dapper for Performance

```csharp
public class DapperProductRepository : IProductRepository
{
    private readonly IDbConnection _connection;

    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, Sku, Price, CategoryId, Stock, CreatedAt
            FROM Products
            WHERE Id = @Id AND IsDeleted = 0
            """;

        return await _connection.QueryFirstOrDefaultAsync<Product>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(
        ProductSearchCriteria criteria,
        CancellationToken ct = default)
    {
        var sql = new StringBuilder("""
            SELECT Id, Name, Sku, Price, CategoryId, Stock, CreatedAt
            FROM Products
            WHERE IsDeleted = 0
            """);

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            sql.Append(" AND Name LIKE @SearchTerm");
            parameters.Add("SearchTerm", $"%{criteria.SearchTerm}%");
        }

        if (criteria.CategoryId.HasValue)
        {
            sql.Append(" AND CategoryId = @CategoryId");
            parameters.Add("CategoryId", criteria.CategoryId);
        }

        if (criteria.MinPrice.HasValue)
        {
            sql.Append(" AND Price >= @MinPrice");
            parameters.Add("MinPrice", criteria.MinPrice);
        }

        if (criteria.MaxPrice.HasValue)
        {
            sql.Append(" AND Price <= @MaxPrice");
            parameters.Add("MaxPrice", criteria.MaxPrice);
        }

        sql.Append(" ORDER BY Name OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
        parameters.Add("Offset", (criteria.Page - 1) * criteria.PageSize);
        parameters.Add("PageSize", criteria.PageSize);

        var results = await _connection.QueryAsync<Product>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct));

        return results.ToList();
    }

    // Multi-mapping for related data
    public async Task<Order?> GetOrderWithItemsAsync(int orderId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT o.*, oi.*, p.*
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
            LEFT JOIN Products p ON oi.ProductId = p.Id
            WHERE o.Id = @OrderId
            """;

        var orderDictionary = new Dictionary<int, Order>();

        await _connection.QueryAsync<Order, OrderItem, Product, Order>(
            new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: ct),
            (order, item, product) =>
            {
                if (!orderDictionary.TryGetValue(order.Id, out var existingOrder))
                {
                    existingOrder = order;
                    existingOrder.Items = new List<OrderItem>();
                    orderDictionary.Add(order.Id, existingOrder);
                }

                if (item != null)
                {
                    item.Product = product;
                    existingOrder.Items.Add(item);
                }

                return existingOrder;
            },
            splitOn: "Id,Id");

        return orderDictionary.Values.FirstOrDefault();
    }
}
```


## Testing Patterns

### Unit Tests with xUnit and Moq

```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<IStockService> _mockStockService;
    private readonly Mock<IValidator<CreateOrderRequest>> _mockValidator;
    private readonly OrderService _sut; // System Under Test

    public OrderServiceTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockStockService = new Mock<IStockService>();
        _mockValidator = new Mock<IValidator<CreateOrderRequest>>();

        // Default: validation passes
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new OrderService(
            _mockRepository.Object,
            _mockStockService.Object,
            _mockValidator.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            ProductId = "PROD-001",
            Quantity = 5,
            CustomerOrderCode = "ORD-2024-001"
        };

        _mockStockService
            .Setup(s => s.CheckAsync("PROD-001", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockResult { IsAvailable = true, Available = 10 });

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order { Id = 1, CustomerOrderCode = "ORD-2024-001" });

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);

        _mockRepository.Verify(
            r => r.CreateAsync(It.Is<Order>(o => o.CustomerOrderCode == "ORD-2024-001"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ReturnsFailure()
    {
        // Arrange
        var request = new CreateOrderRequest { ProductId = "PROD-001", Quantity = 100 };

        _mockStockService
            .Setup(s => s.CheckAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockResult { IsAvailable = false, Available = 5 });

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("INSUFFICIENT_STOCK", result.ErrorCode);
        Assert.Contains("5 available", result.Error);

        _mockRepository.Verify(
            r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateOrderAsync_WithInvalidQuantity_ReturnsValidationError(int quantity)
    {
        // Arrange
        var request = new CreateOrderRequest { ProductId = "PROD-001", Quantity = quantity };

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Quantity", "Quantity must be greater than 0")
            }));

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("VALIDATION_ERROR", result.ErrorCode);
    }
}
```

### Integration Tests with WebApplicationFactory

```csharp
public class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real database with in-memory
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                // Replace Redis with memory cache
                services.RemoveAll<IDistributedCache>();
                services.AddDistributedMemoryCache();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Products.Add(new Product
        {
            Id = "TEST-001",
            Name = "Test Product",
            Price = 99.99m
        });
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/products/TEST-001");

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.Equal("Test Product", product!.Name);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/products/NONEXISTENT");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

## Best Practices

### DO

1. **Use async/await** all the way through the call stack
2. **Inject dependencies** through constructor injection
3. **Use IOptions<T>** for typed configuration
4. **Return Result types** instead of throwing exceptions for business logic
5. **Use CancellationToken** in all async methods
6. **Prefer Dapper** for read-heavy, performance-critical queries
7. **Use EF Core** for complex domain models with change tracking
9. **Write unit tests** for business logic, integration tests for APIs
10. **Use record types** for DTOs and immutable data

### DON'T

1. **Don't block on async** with `.Result` or `.Wait()`
2. **Don't use async void** except for event handlers
3. **Don't catch generic Exception** without re-throwing or logging
4. **Don't hardcode** configuration values
5. **Don't expose EF entities** directly in APIs (use DTOs)
6. **Don't forget** `AsNoTracking()` for read-only queries
7. **Don't ignore** CancellationToken parameters
8. **Don't create** `new HttpClient()` manually (use IHttpClientFactory)
9. **Don't mix** sync and async code unnecessarily
10. **Don't skip** validation at API boundaries

## Common Pitfalls

- **N+1 Queries**: Use `.Include()` or explicit joins
- **Memory Leaks**: Dispose IDisposable resources, use `using`
- **Deadlocks**: Don't mix sync and async, use ConfigureAwait(false) in libraries
- **Over-fetching**: Select only needed columns, use projections
- **Timeout Issues**: Configure appropriate timeouts for HTTP clients
