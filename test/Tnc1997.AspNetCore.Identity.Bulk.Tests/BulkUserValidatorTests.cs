using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Tnc1997.AspNetCore.Identity.Bulk.Tests
{
    public class BulkUserValidatorTests
    {
        private BulkUserManager<IdentityUser<string>> _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = Substitute.For<BulkUserManager<IdentityUser<string>>>(Substitute.For<IBulkUserStore<IdentityUser<string>>>(), Substitute.For<IOptions<IdentityOptions>>(), null, null, null, null, null, null, null);
        }

        [TestCaseSource(nameof(ValidateAsyncTestCases))]
        public async Task ValidateAsync(IEnumerable<IdentityUser<string>> entities, IEnumerable<IdentityUser<string>> users, IEnumerable<IdentityResult> expected)
        {
            // Arrange
            var validator = new BulkUserValidator<IdentityUser<string>>();

            _manager
                .Options
                .Returns(new IdentityOptions { User = new UserOptions { RequireUniqueEmail = true } });

            _manager
                .FindByEmailsAsync(Arg.Any<IEnumerable<string>>())
                .Returns(info => entities.Where(entity => info.Arg<IEnumerable<string>>().Contains(entity.Email)));

            _manager
                .FindByNamesAsync(Arg.Any<IEnumerable<string>>())
                .Returns(info => entities.Where(entity => info.Arg<IEnumerable<string>>().Contains(entity.UserName)));

            _manager
                .GetUserIdsAsync(Arg.Any<IEnumerable<IdentityUser<string>>>())
                .Returns(info => info.Arg<IEnumerable<IdentityUser<string>>>().Select(user => user.Id));

            _manager
                .GetEmailsAsync(Arg.Any<IEnumerable<IdentityUser<string>>>())
                .Returns(info => info.Arg<IEnumerable<IdentityUser<string>>>().Select(user => user.Email));

            _manager
                .GetUserNamesAsync(Arg.Any<IEnumerable<IdentityUser<string>>>())
                .Returns(info => info.Arg<IEnumerable<IdentityUser<string>>>().Select(user => user.UserName));

            // Act
            var actual = await validator.ValidateAsync(_manager, users);

            // Assert
            Assert.That(actual, Is.EqualTo(expected).Using(new IdentityResultComparer()));
        }

        public static IEnumerable<TestCaseData> ValidateAsyncTestCases
        {
            get
            {
                yield return new TestCaseData(
                    Array.Empty<IdentityUser<string>>(),
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", UserName = "", Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        IdentityResult.Failed(new IdentityErrorDescriber().InvalidUserName("")), IdentityResult.Success
                    }
                ).SetName("ValidateAsync_EmptyUserName_InvalidUserNameError");

                yield return new TestCaseData(
                    Array.Empty<IdentityUser<string>>(),
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", UserName = " ", Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        IdentityResult.Failed(new IdentityErrorDescriber().InvalidUserName(" ")), IdentityResult.Success
                    }
                ).SetName("ValidateAsync_WhiteSpaceUserName_InvalidUserNameError");

                yield return new TestCaseData(
                    Array.Empty<IdentityUser<string>>(),
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "al i ce",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        IdentityResult.Failed(new IdentityErrorDescriber().InvalidUserName("al i ce")),
                        IdentityResult.Success
                    }
                ).SetName("ValidateAsync_UserNameWithNonAllowedCharacter_InvalidUserNameError");

                yield return new TestCaseData(
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "alice",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "3eb2207a-bb85-4110-9434-8d104fe1fa22",
                            UserName = "alice",
                            Email = "chloe@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "4f464ad9-eefb-416e-8b75-59cfa026d93e",
                            UserName = "daniel",
                            Email = "daniel@example.com"
                        }
                    },
                    new[]
                    {
                        IdentityResult.Failed(new IdentityErrorDescriber().DuplicateUserName("alice")),
                        IdentityResult.Success
                    }
                ).SetName("ValidateAsync_NonExistentUserWithExistingUserName_DuplicateUserNameError");

                yield return new TestCaseData(
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "alice",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "3eb2207a-bb85-4110-9434-8d104fe1fa22",
                            UserName = "chloe",
                            Email = "chloe@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "4f464ad9-eefb-416e-8b75-59cfa026d93e",
                            UserName = "daniel",
                            Email = "daniel@example.com"
                        }
                    },
                    new[] { IdentityResult.Success, IdentityResult.Success }
                ).SetName("ValidateAsync_NonExistentUserWithNonExistentUserName_SuccessIdentityResult");

                yield return new TestCaseData(
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "alice",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "alice",
                            Email = "chloe@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "4f464ad9-eefb-416e-8b75-59cfa026d93e",
                            UserName = "daniel",
                            Email = "daniel@example.com"
                        }
                    },
                    new[] { IdentityResult.Success, IdentityResult.Success }
                ).SetName("ValidateAsync_ExistingUserWithExistingUserName_SuccessIdentityResult");

                yield return new TestCaseData(
                    Array.Empty<IdentityUser<string>>(),
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", UserName = "alice", Email = ""
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[] { IdentityResult.Failed(new IdentityErrorDescriber().InvalidEmail("")), IdentityResult.Success }
                ).SetName("ValidateAsync_EmptyEmail_InvalidEmailError");

                yield return new TestCaseData(
                    Array.Empty<IdentityUser<string>>(),
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", UserName = "alice", Email = " "
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[] { IdentityResult.Failed(new IdentityErrorDescriber().InvalidEmail(" ")), IdentityResult.Success }
                ).SetName("ValidateAsync_WhiteSpaceEmail_InvalidEmailError");

                yield return new TestCaseData(
                    Array.Empty<IdentityUser<string>>(),
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678", UserName = "alice", Email = "al i ce"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        IdentityResult.Failed(new IdentityErrorDescriber().InvalidEmail("al i ce")),
                        IdentityResult.Success
                    }
                ).SetName("ValidateAsync_InvalidEmail_InvalidEmailError");

                yield return new TestCaseData(
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "alice",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "3eb2207a-bb85-4110-9434-8d104fe1fa22",
                            UserName = "chloe",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "4f464ad9-eefb-416e-8b75-59cfa026d93e",
                            UserName = "daniel",
                            Email = "daniel@example.com"
                        }
                    },
                    new[]
                    {
                        IdentityResult.Failed(new IdentityErrorDescriber().DuplicateEmail("alice@example.com")),
                        IdentityResult.Success
                    }
                ).SetName("ValidateAsync_NonExistentUserWithExistingEmail_DuplicateEmailError");

                yield return new TestCaseData(
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "alice",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "3eb2207a-bb85-4110-9434-8d104fe1fa22",
                            UserName = "chloe",
                            Email = "chloe@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "4f464ad9-eefb-416e-8b75-59cfa026d93e",
                            UserName = "daniel",
                            Email = "daniel@example.com"
                        }
                    },
                    new[] { IdentityResult.Success, IdentityResult.Success }
                ).SetName("ValidateAsync_NonExistentUserWithNonExistentEmail_SuccessIdentityResult");

                yield return new TestCaseData(
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "alice",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "26606db9-66b3-4ab0-a8d0-8bd5860e776a", UserName = "bob", Email = "bob@example.com"
                        }
                    },
                    new[]
                    {
                        new IdentityUser<string>
                        {
                            Id = "1a535a33-ae5d-4ecd-8067-47acf8b4b678",
                            UserName = "chloe",
                            Email = "alice@example.com"
                        },
                        new IdentityUser<string>
                        {
                            Id = "4f464ad9-eefb-416e-8b75-59cfa026d93e",
                            UserName = "daniel",
                            Email = "daniel@example.com"
                        }
                    },
                    new[] { IdentityResult.Success, IdentityResult.Success }
                ).SetName("ValidateAsync_ExistingUserWithExistingEmail_SuccessIdentityResult");
            }
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
        }
    }
}
