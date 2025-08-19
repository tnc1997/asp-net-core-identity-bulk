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

    private IdentityDbContext<IdentityUser<string>, IdentityRole<string>, string> _context;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Filename=:memory:");

        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;

        _context = new IdentityDbContext<IdentityUser<string>, IdentityRole<string>, string>(options);

        await _context.Database.EnsureCreatedAsync();
    }

    #region IBulkUserRoleStore

    [Test]
    public async Task AddToRolesAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>>
        {
            new() { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
            new() { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" }
        };

        _context.AddRange(users);

        var roles = new List<IdentityRole<string>>
        {
            new() { Id = "1d982b8d-e021-49a0-85a1-7fc9382d502f", NormalizedName = "EDITOR" },
            new() { Id = "2e482612-5f8c-4388-ab5f-3eb997cf1662", NormalizedName = "VIEWER" }
        };

        _context.AddRange(roles);

        var tuples = new List<(IdentityUser<string>, string)>
        {
            (
                new IdentityUser<string> { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
                "EDITOR"
            ),
            (
                new IdentityUser<string> { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" },
                "VIEWER"
            )
        };

        await _context.SaveChangesAsync();

        // Act
        await store.AddToRolesAsync(tuples, CancellationToken.None);

        await _context.SaveChangesAsync();

        // Assert
        var actual = await _context.UserRoles
            .OrderBy(userLogin => userLogin.UserId)
            .ThenBy(userLogin => userLogin.RoleId)
            .ToListAsync();

        var expected = new List<IdentityUserRole<string>>
        {
            new()
            {
                UserId = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", RoleId = "1d982b8d-e021-49a0-85a1-7fc9382d502f"
            },
            new()
            {
                UserId = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", RoleId = "2e482612-5f8c-4388-ab5f-3eb997cf1662"
            }
        };

        Assert.That(actual, Is.EqualTo(expected).Using(new IdentityUserRoleComparer()));
    }

    [Test]
    public async Task AreInRolesAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>>
        {
            new() { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
            new() { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" }
        };

        _context.AddRange(users);

        var roles = new List<IdentityRole<string>>
        {
            new() { Id = "1d982b8d-e021-49a0-85a1-7fc9382d502f", NormalizedName = "EDITOR" },
            new() { Id = "2e482612-5f8c-4388-ab5f-3eb997cf1662", NormalizedName = "VIEWER" }
        };

        _context.AddRange(roles);

        var userRoles = new List<IdentityUserRole<string>>
        {
            new()
            {
                UserId = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", RoleId = "1d982b8d-e021-49a0-85a1-7fc9382d502f"
            }
        };

        _context.AddRange(userRoles);

        var tuples = new List<(IdentityUser<string>, string)>
        {
            (
                new IdentityUser<string> { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
                "EDITOR"
            ),
            (
                new IdentityUser<string> { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" },
                "VIEWER"
            )
        };

        await _context.SaveChangesAsync();

        // Act
        var actual = await store.AreInRolesAsync(tuples, CancellationToken.None);

        // Assert
        var expected = new List<bool> { true, false };

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetRolesAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>>
        {
            new() { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
            new() { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" }
        };

        _context.AddRange(users);

        var roles = new List<IdentityRole<string>>
        {
            new() { Id = "1d982b8d-e021-49a0-85a1-7fc9382d502f", Name = "editor" },
            new() { Id = "2e482612-5f8c-4388-ab5f-3eb997cf1662", Name = "viewer" }
        };

        _context.AddRange(roles);

        var userRoles = new List<IdentityUserRole<string>>
        {
            new()
            {
                UserId = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", RoleId = "1d982b8d-e021-49a0-85a1-7fc9382d502f"
            },
            new()
            {
                UserId = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", RoleId = "2e482612-5f8c-4388-ab5f-3eb997cf1662"
            }
        };

        _context.AddRange(userRoles);

        await _context.SaveChangesAsync();

        // Act
        var actual = await store.GetRolesAsync(users, CancellationToken.None);

        // Assert
        var expected = new List<List<string>> { new() { "editor" }, new() { "viewer" } };

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task RemoveFromRolesAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>>
        {
            new() { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
            new() { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" }
        };

        _context.AddRange(users);

        var roles = new List<IdentityRole<string>>
        {
            new() { Id = "1d982b8d-e021-49a0-85a1-7fc9382d502f", NormalizedName = "EDITOR" },
            new() { Id = "2e482612-5f8c-4388-ab5f-3eb997cf1662", NormalizedName = "VIEWER" }
        };

        _context.AddRange(roles);

        var userRoles = new List<IdentityUserRole<string>>
        {
            new()
            {
                UserId = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", RoleId = "1d982b8d-e021-49a0-85a1-7fc9382d502f"
            },
            new()
            {
                UserId = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", RoleId = "2e482612-5f8c-4388-ab5f-3eb997cf1662"
            }
        };

        _context.AddRange(userRoles);

        var tuples = new List<(IdentityUser<string>, string)>
        {
            (
                new IdentityUser<string> { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
                "EDITOR"
            ),
            (
                new IdentityUser<string> { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" },
                "VIEWER"
            )
        };

        await _context.SaveChangesAsync();

        // Act
        await store.RemoveFromRolesAsync(tuples, CancellationToken.None);

        await _context.SaveChangesAsync();

        // Assert
        var actual = await _context.UserRoles.ToListAsync();

        Assert.That(actual, Is.Empty);
    }

    #endregion

    [TearDown]
    public async Task TearDown()
    {
        await _connection.DisposeAsync();

        await _context.DisposeAsync();
    }
}
