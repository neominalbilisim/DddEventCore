using Neominal.DddEventCore.Domain;
using Neominal.DddEventCore.Events;

namespace Neominal.DddEventCore.Sample.Domain.Customers.Services;

/// <summary>
/// Domain services related to customers.
/// Business logic that needs to communicate with repository goes here.
/// </summary>
public class CustomerUniquenessService : IDomainService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerUniquenessService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    /// <summary>
    /// Checks if the email address is unique in the system
    /// </summary>
    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var existingCustomer = await _customerRepository.GetByEmailAsync(email, cancellationToken);
        return existingCustomer == null;
    }

    /// <summary>
    /// Ensures email uniqueness during new customer registration
    /// </summary>
    public async Task EnsureEmailIsUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        var isUnique = await IsEmailUniqueAsync(email, cancellationToken);
        
        if (!isUnique)
            throw new DomainException($"Email '{email}' is already registered");
    }
}

/// <summary>
/// Customer repository interface (implemented in Infrastructure layer)
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Customer aggregate root
/// </summary>
public class Customer : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private Customer() : base()
    {
        Name = null!;
        Email = null!;
    }

    private Customer(Guid id, string name, string email) : base(id)
    {
        Name = name;
        Email = email;
        RegisteredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registers a new customer - checks email uniqueness with Domain Service
    /// </summary>
    public static async Task<Customer> RegisterAsync(
        string name, 
        string email,
        CustomerUniquenessService uniquenessService,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name is required");

        // Check email uniqueness using Domain Service
        await uniquenessService.EnsureEmailIsUniqueAsync(email, cancellationToken);

        var customer = new Customer(Guid.NewGuid(), name, email);

        // Raise event
        customer.RaiseEvent(new CustomerRegisteredEvent(customer.Id, name, email));
        
        return customer;
    }
}

/// <summary>
/// Event raised when a customer is registered
/// </summary>
public sealed record CustomerRegisteredEvent : IDomainEvent
{
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }
    
    public Guid CustomerId { get; }
    public string Name { get; }
    public string Email { get; }

    public CustomerRegisteredEvent(Guid customerId, string name, string email)
    {
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
        CustomerId = customerId;
        Name = name;
        Email = email;
    }
}
