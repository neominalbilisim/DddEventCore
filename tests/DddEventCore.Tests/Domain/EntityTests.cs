using Neominal.DddEventCore.Domain;
using FluentAssertions;
using Xunit;

namespace Neominal.DddEventCore.Tests.Domain;

public class EntityTests
{
    private class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
        private TestEntity() : base() { } // For testing
    }

    [Fact]
    public void Entity_ShouldHaveId()
    {
        // Arrange & Act
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        // Assert
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Entity_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act & Assert
        entity1.Should().Be(entity2);
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void Entity_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act & Assert
        entity1.Should().NotBe(entity2);
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Entity_Constructor_WithDefaultId_ShouldThrowException()
    {
        // Act
        var act = () => new TestEntity(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Entity ID cannot be null or default value*");
    }
}
