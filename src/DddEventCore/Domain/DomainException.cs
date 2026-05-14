namespace Neominal.DddEventCore.Domain;

/// <summary>
/// Exception used for business rule violations in the domain layer.
/// Represents business logic errors rather than technical errors.
/// </summary>
public class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
