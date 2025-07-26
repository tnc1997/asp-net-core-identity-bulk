using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore.Tests;

public class BulkUserStoreTests
{
    private SqliteConnection _connection;

    private IdentityDbContext _context;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Filename=:memory:");

        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;

        _context = new IdentityDbContext(options);

        await _context.Database.EnsureCreatedAsync();
    }

    [Test]
    public async Task CreateAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var expected = new List<IdentityUser> { new() { UserName = "alice" }, new() { UserName = "bob" } };

        // Act
        await store.CreateAsync(expected, CancellationToken.None);

        // Assert
        var actual = await _context.Users.OrderBy(user => user.UserName).ToListAsync();

        Assert.That(actual, Is.EqualTo(expected).Using(new IdentityUserComparer()));
    }

    [Test]
    public async Task DeleteAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser> { new() { UserName = "alice" }, new() { UserName = "bob" } };

        _context.Users.AddRange(users);

        await _context.SaveChangesAsync();

        // Act
        await store.DeleteAsync(users, CancellationToken.None);

        // Assert
        var actual = await _context.Users.ToListAsync();

        Assert.That(actual, Is.Empty);
    }

    [Test]
    public async Task UpdateAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser> { new() { UserName = "alice" }, new() { UserName = "bob" } };

        _context.Users.AddRange(users);

        await _context.SaveChangesAsync();

        // Act
        users[0].UserName = "yankee";
        users[1].UserName = "zulu";

        await store.UpdateAsync(users, CancellationToken.None);

        // Assert
        var actual = await _context.Users.OrderBy(user => user.UserName).ToListAsync();

        Assert.That(actual, Is.EqualTo(users).Using(new IdentityUserComparer()));
    }

    [TearDown]
    public async Task TearDown()
    {
        await _connection.DisposeAsync();

        await _context.DisposeAsync();
    }
}
