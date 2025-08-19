# ASP.NET Core Identity Bulk

ASP.NET Core Identity Bulk allows you to perform bulk ASP.NET Core Identity operations.

## Getting Started

1. Add the Identity services to your application.

    ```csharp
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>();
    ```

2. Install the Identity Bulk package.

    ```shell
    dotnet add package Tnc1997.AspNetCore.Identity.Bulk
    ```

3. Add the Identity Bulk services to your application.

    ```csharp
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddBulk();
    ```

### Entity Framework Core

1. Install the Identity Entity Framework Core package.

    ```shell
    dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
    ```

2. Add the Entity Framework stores to your application.

    ```csharp
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddBulk()
        .AddEntityFrameworkStores<ApplicationDbContext>();
    ```

3. Install the Identity Bulk Entity Framework Core package.

    ```shell
    dotnet add package Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore
    ```

4. Add the Bulk Entity Framework stores to your application.

    ```csharp
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddBulk()
        .AddBulkEntityFrameworkStores<ApplicationDbContext>()
        .AddEntityFrameworkStores<ApplicationDbContext>();
    ```
