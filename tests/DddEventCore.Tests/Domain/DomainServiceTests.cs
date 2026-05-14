using Neominal.DddEventCore.Domain;
using FluentAssertions;
using Xunit;

namespace Neominal.DddEventCore.Tests.Domain;

public class DomainServiceTests
{
    private class TestDomainService : IDomainService
    {
        public string PerformBusinessLogic(string input)
        {
            return $"Processed: {input}";
        }
    }

    [Fact]
    public void DomainService_ShouldImplementMarkerInterface()
    {
        // Arrange & Act
        var service = new TestDomainService();

        // Assert
        service.Should().BeAssignableTo<IDomainService>();
    }

    [Fact]
    public void DomainService_ShouldExecuteBusinessLogic()
    {
        // Arrange
        var service = new TestDomainService();
        var input = "test data";

        // Act
        var result = service.PerformBusinessLogic(input);

        // Assert
        result.Should().Be("Processed: test data");
    }
}
