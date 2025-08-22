using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;

namespace Tnc1997.AspNetCore.Identity.Bulk.Tests
{
    public class BulkPasswordHasherTests
    {
        private IPasswordHasher<IdentityUser<string>> _inner;

        [SetUp]
        public void SetUp()
        {
            _inner = Substitute.For<IPasswordHasher<IdentityUser<string>>>();
        }

        [Test]
        public void HashPasswords()
        {
            // Arrange
            var hasher = new BulkPasswordHasher<IdentityUser<string>>(_inner);

            var tuples = new List<(IdentityUser<string>, string)>
            {
                (
                    new IdentityUser<string>(),
                    "alice"
                ),
                (
                    new IdentityUser<string>(),
                    "bob"
                )
            };

            // Act
            hasher.HashPasswords(tuples);

            // Assert
            foreach (var (user, password) in tuples)
            {
                _inner.Received().HashPassword(Arg.Is(user), Arg.Is(password));
            }
        }
    }
}
