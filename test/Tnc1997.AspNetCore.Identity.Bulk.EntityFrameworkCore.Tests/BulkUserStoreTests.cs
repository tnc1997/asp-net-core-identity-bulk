using System;
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
        var users = expected.Select(email => new IdentityUser { Email = email });

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

    #region IBulkUserLockoutStore

    [Test]
    public async Task GetLockoutEnabledAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var expected = new List<bool> { true, true };
        var users = expected.Select(lockoutEnabled => new IdentityUser { LockoutEnabled = lockoutEnabled });

        // Act
        var actual = await store.GetLockoutEnabledAsync(users, CancellationToken.None);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task SetLockoutEnabledAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>> { new(), new() };
        var expected = new List<bool> { true, true };
        var tuples = users.Zip(expected).ToList();

        // Act
        await store.SetLockoutEnabledAsync(tuples, CancellationToken.None);

        // Assert
        var actual = users.Select(user => user.LockoutEnabled);

        Assert.That(actual, Is.EqualTo(expected));
    }

    #endregion

    #region IBulkUserLoginStore

    [Test]
    public async Task AddLoginsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var tuples = new List<(IdentityUser<string>, UserLoginInfo)>
        {
            (
                new IdentityUser<string> { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
                new UserLoginInfo("Facebook", "a8737ad6-71ab-46ca-b89d-a7d932e0f4c2", null)
            ),
            (
                new IdentityUser<string> { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" },
                new UserLoginInfo("Google", "b184c76d-cfa0-4eec-82b4-3a25b4f64574", null)
            )
        };

        foreach (var (user, _) in tuples)
        {
            _context.Add(user);
        }

        await _context.SaveChangesAsync();

        // Act
        await store.AddLoginsAsync(tuples, CancellationToken.None);

        await _context.SaveChangesAsync();

        // Assert
        var actual = await _context.UserLogins
            .OrderBy(userLogin => userLogin.UserId)
            .ThenBy(userLogin => userLogin.LoginProvider)
            .ThenBy(userLogin => userLogin.ProviderKey)
            .ToListAsync();

        var expected = tuples
            .Select(tuple => new IdentityUserLogin<string>
            {
                UserId = tuple.Item1.Id,
                LoginProvider = tuple.Item2.LoginProvider,
                ProviderKey = tuple.Item2.ProviderKey,
                ProviderDisplayName = tuple.Item2.ProviderDisplayName
            })
            .ToList();

        Assert.That(actual, Is.EqualTo(expected).Using(new IdentityUserLoginComparer()));
    }

    [Test]
    public async Task FindByLoginsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>>
        {
            new() { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
            new() { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" }
        };

        var tuples = new List<(string, string)>
        {
            ("Facebook", "a8737ad6-71ab-46ca-b89d-a7d932e0f4c2"), ("Google", "b184c76d-cfa0-4eec-82b4-3a25b4f64574")
        };

        foreach (var user in users)
        {
            _context.Add(user);
        }

        _context.Add(new IdentityUserLogin<string>
        {
            UserId = users[0].Id, LoginProvider = tuples[0].Item1, ProviderKey = tuples[0].Item2
        });

        await _context.SaveChangesAsync();

        // Act
        var actual = await store.FindByLoginsAsync(tuples, CancellationToken.None);

        // Assert
        var expected = new List<IdentityUser<string>?> { users[0], null };

        Assert.That(actual, Is.EqualTo(expected).Using(new IdentityUserComparer()));
    }

    [Test]
    public async Task RemoveLoginsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var tuples = new List<(IdentityUser<string>, string, string)>
        {
            (
                new IdentityUser<string> { Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678" },
                "Facebook",
                "a8737ad6-71ab-46ca-b89d-a7d932e0f4c2"
            ),
            (
                new IdentityUser<string> { Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a" },
                "Google",
                "b184c76d-cfa0-4eec-82b4-3a25b4f64574"
            )
        };

        foreach (var (user, loginProvider, providerKey) in tuples)
        {
            _context.Add(user);

            _context.Add(new IdentityUserLogin<string>
            {
                UserId = user.Id, LoginProvider = loginProvider, ProviderKey = providerKey
            });
        }

        await _context.SaveChangesAsync();

        // Act
        await store.RemoveLoginsAsync(tuples, CancellationToken.None);

        await _context.SaveChangesAsync();

        // Assert
        var actual = await _context.UserLogins.ToListAsync();

        Assert.That(actual, Is.Empty);
    }

    #endregion

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

    #region IBulkUserSecurityStampStore

    [Test]
    public async Task GetSecurityStampsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var expected = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        var users = expected.Select(securityStamp => new IdentityUser { SecurityStamp = securityStamp });

        // Act
        var actual = await store.GetSecurityStampsAsync(users, CancellationToken.None);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task SetSecurityStampsAsync()
    {
        // Arrange
        using var store = new BulkUserStore(_context);

        var users = new List<IdentityUser<string>> { new(), new() };
        var expected = new List<string?> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        var tuples = users.Zip(expected).ToList();

        // Act
        await store.SetSecurityStampsAsync(tuples, CancellationToken.None);

        // Assert
        var actual = users.Select(user => user.SecurityStamp);

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
        var actual = await _context.Users
            .OrderBy(user => user.UserName)
            .ToListAsync();

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
        var actual = await _context.Users
            .OrderBy(user => user.UserName)
            .ToListAsync();

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
