# API Endpoint Creation Workflow

## Overview
Step-by-step process for creating new API endpoints in the VerdaVidaLawnCare application, ensuring consistency with existing patterns and best practices.

## Workflow Steps

### 1. Define the Endpoint
- **Identify Resource**: Determine the main entity/resource being accessed
- **Choose HTTP Method**: GET (read), POST (create), PUT (update), DELETE (remove)
- **Define Route**: Follow RESTful conventions (e.g., `api/customers/{id}/appointments`)
- **Specify Parameters**: Path parameters, query parameters, request body

### 2. Create/Update DTOs
- **Request DTO**: Define input validation and structure
- **Response DTO**: Define output structure and serialization
- **Validation Attributes**: Add data annotations for validation
- **Documentation**: Include XML comments for Swagger

### 3. Update Service Layer
- **Service Interface**: Add method signature to interface
- **Service Implementation**: Implement business logic
- **Validation**: Add business rule validation
- **Error Handling**: Return Result<T> for consistent error handling

### 4. Update Repository Layer
- **Repository Interface**: Add data access method
- **Repository Implementation**: Implement database operations
- **Query Optimization**: Ensure efficient database queries
- **Error Handling**: Handle database-specific exceptions

### 5. Create Controller Method
- **Method Signature**: Define async method with proper return type
- **Parameter Binding**: Use [FromBody], [FromRoute], [FromQuery] attributes
- **Validation**: Check ModelState.IsValid
- **Service Call**: Call appropriate service method
- **Error Handling**: Use try-catch with specific exception types
- **Logging**: Add structured logging for debugging

### 6. Add Documentation
- **XML Comments**: Document method, parameters, and return values
- **Swagger Examples**: Provide example requests and responses
- **Error Responses**: Document possible error scenarios
- **Status Codes**: Document HTTP status codes returned

### 7. Add Tests
- **Unit Tests**: Test service layer logic
- **Integration Tests**: Test complete API endpoint
- **Edge Cases**: Test validation, error scenarios, and boundary conditions
- **Performance Tests**: Ensure endpoint performs well under load

### 8. Security Considerations
- **Authentication**: Determine if endpoint requires authentication
- **Authorization**: Check if user has permission to access resource
- **Input Validation**: Sanitize and validate all inputs
- **Rate Limiting**: Consider if endpoint needs rate limiting
- **Audit Logging**: Log access to sensitive endpoints

## Example Implementation

```csharp
/// <summary>
/// Creates a new appointment for a customer
/// </summary>
/// <param name="customerId">The customer ID</param>
/// <param name="request">The appointment creation request</param>
/// <returns>The created appointment</returns>
/// <response code="201">Appointment created successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="404">Customer not found</response>
/// <response code="500">Internal server error</response>
[HttpPost("customers/{customerId}/appointments")]
[ProducesResponseType(typeof(AppointmentDto), 201)]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public async Task<ActionResult<AppointmentDto>> CreateAppointment(
    int customerId,
    [FromBody] CreateAppointmentRequest request)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _appointmentService.CreateAppointmentAsync(customerId, request);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(GetAppointment), 
            new { customerId, id = result.Value.Id }, 
            result.Value);
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning("Validation failed for appointment creation: {Error}", ex.Message);
        return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating appointment for customer {CustomerId}", customerId);
        return StatusCode(500, "Internal server error");
    }
}
```

## Quality Checklist
- [ ] Endpoint follows RESTful conventions
- [ ] Proper HTTP status codes are returned
- [ ] Input validation is implemented
- [ ] Error handling uses Result<T> pattern
- [ ] Structured logging is included
- [ ] XML documentation is complete
- [ ] Unit tests cover business logic
- [ ] Integration tests cover the endpoint
- [ ] Security considerations are addressed
- [ ] Performance is acceptable
