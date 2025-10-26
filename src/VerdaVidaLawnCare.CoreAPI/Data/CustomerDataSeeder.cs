using Bogus;
using Microsoft.EntityFrameworkCore;
using VerdaVidaLawnCare.CoreAPI.Models;

namespace VerdaVidaLawnCare.CoreAPI.Data;

/// <summary>
/// Service for seeding customer data with realistic fake information
/// </summary>
public class CustomerDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerDataSeeder> _logger;

    public CustomerDataSeeder(ApplicationDbContext context, ILogger<CustomerDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with 1000 fake customer records
    /// </summary>
    public async Task SeedCustomersAsync(int count = 1000)
    {
        _logger.LogInformation("Starting customer data seeding for {Count} records", count);

        // Check if customers already exist
        if (await _context.Customers.CountAsync() > 25)
        {
            _logger.LogWarning("Customers already exist in the database. Seeding skipped.");
            throw new InvalidOperationException("Customers already exist in the database. Clear existing customers before seeding.");
        }

        // Define real MD/VA/DE locations within ~60 miles of Annapolis, MD
        var locations = new[]
        {
            // Maryland
            new { City = "Annapolis", State = "MD", ZipCode = "21401" },
            new { City = "Annapolis", State = "MD", ZipCode = "21403" },
            new { City = "Baltimore", State = "MD", ZipCode = "21201" },
            new { City = "Baltimore", State = "MD", ZipCode = "21202" },
            new { City = "Columbia", State = "MD", ZipCode = "21044" },
            new { City = "Columbia", State = "MD", ZipCode = "21045" },
            new { City = "Ellicott City", State = "MD", ZipCode = "21043" },
            new { City = "Bowie", State = "MD", ZipCode = "20715" },
            new { City = "Glen Burnie", State = "MD", ZipCode = "21061" },
            new { City = "Severna Park", State = "MD", ZipCode = "21146" },
            new { City = "Pasadena", State = "MD", ZipCode = "21122" },
            new { City = "Arnold", State = "MD", ZipCode = "21012" },
            new { City = "Crofton", State = "MD", ZipCode = "21114" },
            new { City = "Odenton", State = "MD", ZipCode = "21113" },
            new { City = "Millersville", State = "MD", ZipCode = "21108" },
            new { City = "Gambrills", State = "MD", ZipCode = "21054" },
            new { City = "Edgewater", State = "MD", ZipCode = "21037" },
            new { City = "Davidsonville", State = "MD", ZipCode = "21035" },
            new { City = "Lothian", State = "MD", ZipCode = "20711" },
            new { City = "Deale", State = "MD", ZipCode = "20751" },

            // Virginia
            new { City = "Alexandria", State = "VA", ZipCode = "22301" },
            new { City = "Alexandria", State = "VA", ZipCode = "22302" },
            new { City = "Arlington", State = "VA", ZipCode = "22201" },
            new { City = "Arlington", State = "VA", ZipCode = "22202" },
            new { City = "Fairfax", State = "VA", ZipCode = "22030" },
            new { City = "Fairfax", State = "VA", ZipCode = "22031" },
            new { City = "Reston", State = "VA", ZipCode = "20190" },
            new { City = "Reston", State = "VA", ZipCode = "20191" },
            new { City = "Herndon", State = "VA", ZipCode = "20170" },
            new { City = "Vienna", State = "VA", ZipCode = "22180" },
            new { City = "Vienna", State = "VA", ZipCode = "22181" },
            new { City = "McLean", State = "VA", ZipCode = "22101" },
            new { City = "McLean", State = "VA", ZipCode = "22102" },
            new { City = "Springfield", State = "VA", ZipCode = "22150" },
            new { City = "Springfield", State = "VA", ZipCode = "22151" },

            // Delaware
            new { City = "Wilmington", State = "DE", ZipCode = "19801" },
            new { City = "Wilmington", State = "DE", ZipCode = "19802" },
            new { City = "Newark", State = "DE", ZipCode = "19702" },
            new { City = "Newark", State = "DE", ZipCode = "19711" },
            new { City = "Dover", State = "DE", ZipCode = "19901" },
            new { City = "Middletown", State = "DE", ZipCode = "19709" },
            new { City = "Bear", State = "DE", ZipCode = "19701" },
            new { City = "Glasgow", State = "DE", ZipCode = "19702" },
            new { City = "New Castle", State = "DE", ZipCode = "19720" }
        };

        // Create Faker for customer data
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Email, (_, c) => $"{c.FirstName.ToLower()}.{c.LastName.ToLower()}@example.com")
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("(###) ###-####"))
            .RuleFor(c => c.Address, f => $"{f.Address.StreetAddress()}")
            .RuleFor(c => c.City, f => f.PickRandom(locations).City)
            .RuleFor(c => c.State, f => f.PickRandom(locations).State)
            .RuleFor(c => c.PostalCode, f => f.PickRandom(locations).ZipCode)
            .RuleFor(c => c.CreatedAt, f => DateTimeOffset.UtcNow.AddDays(f.Random.Int(-730, 0)))
            .RuleFor(c => c.UpdatedAt, f => DateTimeOffset.UtcNow.AddDays(f.Random.Int(-730, 0)))
            .RuleFor(c => c.IsActive, _ => true);

        // Generate unique customers (avoid duplicates)
        var customers = new HashSet<Customer>(count);
        var customerGenerator = customerFaker.Generate(count);

        // Ensure email uniqueness
        var emailSet = new HashSet<string>();
        foreach (var customer in customerGenerator)
        {
            var originalEmail = customer.Email;
            var counter = 1;

            while (emailSet.Contains(customer.Email))
            {
                customer.Email = $"{originalEmail.Split('@')[0]}{counter}@example.com";
                counter++;
            }

            emailSet.Add(customer.Email);
            customers.Add(customer);
        }

        // Add to database in batches for performance
        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully seeded {Count} customer records", customers.Count);
    }
}
