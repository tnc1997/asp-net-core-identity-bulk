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

    #region IBulkUserEmailStore

    [Test]
    public async Task GetNormalizedEmailsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var expected = new List<string> { "ALICE@EXAMPLE.COM", "BOB@EXAMPLE.COM" };
        var users = expected.Select(normalizedEmail => new IdentityUser { NormalizedEmail = normalizedEmail });

        // Act
        var actual = await store.GetNormalizedEmailsAsync(users, CancellationToken.None);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetEmailsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var expected = new List<string> { "alice@example.com", "bob@example.com" };
        var users = expected.Select(userName => new IdentityUser { Email = userName });

        // Act
        var actual = await store.GetEmailsAsync(users, CancellationToken.None);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task SetNormalizedEmailsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>> { new(), new() };
        var expected = new List<string?> { "ALICE@EXAMPLE.COM", "BOB@EXAMPLE.COM" };
        var tuples = users.Zip(expected).ToList();

        // Act
        await store.SetNormalizedEmailsAsync(tuples, CancellationToken.None);

        // Assert
        var actual = users.Select(user => user.NormalizedEmail);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task SetEmailsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>> { new(), new() };
        var expected = new List<string?> { "alice@example.com", "bob@example.com" };
        var tuples = users.Zip(expected).ToList();

        // Act
        await store.SetEmailsAsync(tuples, CancellationToken.None);

        // Assert
        var actual = users.Select(user => user.Email);

        Assert.That(actual, Is.EqualTo(expected));
    }

    #endregion

    #region IUserStore

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
    public async Task GetNormalizedUserNamesAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var expected = new List<string> { "ALICE", "BOB" };
        var users = expected.Select(normalizedUserName => new IdentityUser { NormalizedUserName = normalizedUserName });

        // Act
        var actual = await store.GetNormalizedUserNamesAsync(users, CancellationToken.None);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetUserNamesAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var expected = new List<string> { "alice", "bob" };
        var users = expected.Select(userName => new IdentityUser { UserName = userName });

        // Act
        var actual = await store.GetUserNamesAsync(users, CancellationToken.None);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task SetNormalizedUserNamesAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>> { new(), new() };
        var expected = new List<string?> { "ALICE", "BOB" };
        var tuples = users.Zip(expected).ToList();

        // Act
        await store.SetNormalizedUserNamesAsync(tuples, CancellationToken.None);

        // Assert
        var actual = users.Select(user => user.NormalizedUserName);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task SetUserNamesAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>> { new(), new() };
        var expected = new List<string?> { "alice", "bob" };
        var tuples = users.Zip(expected).ToList();

        // Act
        await store.SetUserNamesAsync(tuples, CancellationToken.None);

        // Assert
        var actual = users.Select(user => user.UserName);

        Assert.That(actual, Is.EqualTo(expected));
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

    #endregion

    [TearDown]
    public async Task TearDown()
    {
        await _connection.DisposeAsync();

        await _context.DisposeAsync();
    }
}
