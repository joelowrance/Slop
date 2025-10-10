# Testing Workflow

## Overview
Comprehensive testing strategy for the VerdaVidaLawnCare application, ensuring code quality, reliability, and maintainability through systematic testing approaches.

## Testing Pyramid

### 1. Unit Tests (Foundation)
- **Purpose**: Test individual methods and classes in isolation
- **Coverage Target**: 80%+ code coverage
- **Tools**: xUnit, Moq, FluentAssertions
- **Scope**: Business logic, services, utilities

### 2. Integration Tests (Middle)
- **Purpose**: Test component interactions and database operations
- **Coverage Target**: All API endpoints and critical workflows
- **Tools**: xUnit, TestServer, InMemory database
- **Scope**: API controllers, database operations, external service integrations

### 3. End-to-End Tests (Top)
- **Purpose**: Test complete user workflows
- **Coverage Target**: Critical business processes
- **Tools**: Playwright, Selenium, or similar
- **Scope**: User journeys, cross-browser compatibility

## Workflow Steps

### 1. Test Planning
- **Identify Test Cases**: List all scenarios to be tested
- **Prioritize Tests**: Focus on critical business logic first
- **Define Test Data**: Create test data sets and fixtures
- **Set Up Test Environment**: Configure test database and services

### 2. Unit Test Development
- **Arrange-Act-Assert Pattern**: Structure tests clearly
- **Mock Dependencies**: Use Moq for external dependencies
- **Test Edge Cases**: Include boundary conditions and error scenarios
- **Use Descriptive Names**: Test names should explain what is being tested

### 3. Integration Test Development
- **Test API Endpoints**: Verify HTTP responses and status codes
- **Test Database Operations**: Ensure data persistence and retrieval
- **Test Authentication**: Verify security mechanisms
- **Test Error Handling**: Ensure proper error responses

### 4. Test Execution
- **Run Tests Locally**: Execute tests during development
- **CI/CD Pipeline**: Automatically run tests on code changes
- **Test Reports**: Generate coverage and test result reports
- **Monitor Performance**: Ensure tests run efficiently

### 5. Test Maintenance
- **Update Tests**: Modify tests when code changes
- **Refactor Tests**: Keep tests clean and maintainable
- **Remove Obsolete Tests**: Delete tests for removed functionality
- **Review Test Coverage**: Ensure adequate coverage across codebase

## Unit Testing Patterns

### Service Testing
```csharp
[Fact]
public async Task CreateCustomer_WithValidData_ReturnsSuccess()
{
    // Arrange
    var customer = new Customer { Name = "John Doe", Email = "john@example.com" };
    var mockRepository = new Mock<ICustomerRepository>();
    mockRepository.Setup(r => r.AddAsync(It.IsAny<Customer>()))
                 .ReturnsAsync(customer);
    var service = new CustomerService(mockRepository.Object);

    // Act
    var result = await service.CreateCustomerAsync(customer);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEquivalentTo(customer);
    mockRepository.Verify(r => r.AddAsync(customer), Times.Once);
}
```

### Controller Testing
```csharp
[Fact]
public async Task GetCustomer_WithValidId_ReturnsCustomer()
{
    // Arrange
    var customer = new Customer { Id = 1, Name = "John Doe" };
    var mockService = new Mock<ICustomerService>();
    mockService.Setup(s => s.GetByIdAsync(1))
               .ReturnsAsync(Result<Customer>.Success(customer));
    var controller = new CustomersController(mockService.Object, Mock.Of<ILogger<CustomersController>>());

    // Act
    var result = await controller.Get(1);

    // Assert
    var okResult = result.Result as OkObjectResult;
    okResult.Value.Should().BeEquivalentTo(customer);
}
```

## Integration Testing Patterns

### API Testing
```csharp
[Fact]
public async Task POST_Customers_WithValidData_ReturnsCreated()
{
    // Arrange
    var client = _factory.CreateClient();
    var customer = new { Name = "John Doe", Email = "john@example.com" };
    var content = new StringContent(JsonSerializer.Serialize(customer), 
                                   Encoding.UTF8, "application/json");

    // Act
    var response = await client.PostAsync("/api/customers", content);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var responseContent = await response.Content.ReadAsStringAsync();
    var createdCustomer = JsonSerializer.Deserialize<Customer>(responseContent);
    createdCustomer.Name.Should().Be("John Doe");
}
```

### Database Testing
```csharp
[Fact]
public async Task CustomerRepository_AddAsync_SavesToDatabase()
{
    // Arrange
    using var context = CreateTestContext();
    var repository = new CustomerRepository(context);
    var customer = new Customer { Name = "John Doe", Email = "john@example.com" };

    // Act
    var result = await repository.AddAsync(customer);
    await context.SaveChangesAsync();

    // Assert
    var savedCustomer = await context.Customers.FindAsync(result.Id);
    savedCustomer.Should().NotBeNull();
    savedCustomer.Name.Should().Be("John Doe");
}
```

## Test Data Management

### Test Fixtures
```csharp
public class CustomerTestFixture
{
    public Customer ValidCustomer => new()
    {
        Name = "John Doe",
        Email = "john@example.com",
        Phone = "555-1234"
    };

    public List<Customer> MultipleCustomers => new()
    {
        new Customer { Name = "John Doe", Email = "john@example.com" },
        new Customer { Name = "Jane Smith", Email = "jane@example.com" }
    };
}
```

### Database Seeding
```csharp
public static class TestDataSeeder
{
    public static void SeedTestData(ApplicationDbContext context)
    {
        if (!context.Customers.Any())
        {
            context.Customers.AddRange(
                new Customer { Name = "Test Customer 1", Email = "test1@example.com" },
                new Customer { Name = "Test Customer 2", Email = "test2@example.com" }
            );
            context.SaveChanges();
        }
    }
}
```

## Quality Checklist
- [ ] Unit tests cover all business logic methods
- [ ] Integration tests cover all API endpoints
- [ ] Tests use descriptive names and clear structure
- [ ] Mock objects are used appropriately for dependencies
- [ ] Test data is isolated and doesn't affect other tests
- [ ] Error scenarios and edge cases are tested
- [ ] Test coverage meets the 80% target
- [ ] Tests run quickly and reliably
- [ ] CI/CD pipeline includes test execution
- [ ] Test reports are generated and reviewed
