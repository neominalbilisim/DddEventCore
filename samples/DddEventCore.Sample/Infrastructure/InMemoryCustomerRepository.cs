namespace Neominal.DddEventCore.Sample.Domain.Customers.Services;

/// <summary>
/// In-memory customer repository (simple implementation for example)
/// </summary>
public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<string, Customer> _customersByEmail = new(StringComparer.OrdinalIgnoreCase);

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _customersByEmail.TryGetValue(email, out var customer);
        return Task.FromResult(customer);
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _customersByEmail[customer.Email] = customer;
        return Task.CompletedTask;
    }
}
