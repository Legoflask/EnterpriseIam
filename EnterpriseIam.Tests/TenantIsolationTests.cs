using System;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseIam.Core.Entities;
using EnterpriseIam.Core.Interfaces;
using EnterpriseIam.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EnterpriseIam.Tests.Data;

public class TenantIsolationTests
{
    [Fact]
    public async Task DbContext_ShouldOnlyReturnUsersMatchingTheActiveTenantId()
    {
        // 1. ARRANGE: Define mock tenant tracking IDs
        var activeTenantId = Guid.NewGuid();
        var rivalTenantId = Guid.NewGuid();

        // Use Moq to fake the ITenantProvider so it pretends the active user belongs to activeTenantId
        var mockTenantProvider = new Mock<ITenantProvider>();
        mockTenantProvider.Setup(p => p.TenantId).Returns(activeTenantId);

        // Configure a lightning-fast isolated database copy directly in RAM
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"InMemoryTenantDb_{Guid.NewGuid()}")
            .Options;

        // Populate our in-memory database with test data across multiple distinct tenants
        using (var seedContext = new ApplicationDbContext(options, mockTenantProvider.Object))
        {
            // User belonging to the logged-in client tenant company
            seedContext.Users.Add(new User { Id = Guid.NewGuid(), TenantId = activeTenantId, Email = "active@tenant.com", PasswordHash = "hash" });

            // User belonging to a completely separate competitor enterprise company
            seedContext.Users.Add(new User { Id = Guid.NewGuid(), TenantId = rivalTenantId, Email = "hacker@rivalcompany.com", PasswordHash = "hash" });

            await seedContext.SaveChangesAsync();
        }

        // 2. ACT: Read from the database using a fresh context container simulating a new incoming HTTP web request
        using (var testContext = new ApplicationDbContext(options, mockTenantProvider.Object))
        {
            var discoveredUsers = await testContext.Users.ToListAsync();

            // 3. ASSERT: Enforce that the rival tenant user was automatically filtered out by EF Core 10!
            Assert.Single(discoveredUsers); // Exactly one user should be found
            Assert.Equal("active@tenant.com", discoveredUsers.First().Email);
            Assert.DoesNotContain(discoveredUsers, u => u.TenantId == rivalTenantId);
        }
    }
}