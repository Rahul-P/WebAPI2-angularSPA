
namespace Authentication.API
{
    using Authentication.API.Entities;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Data.Entity;

    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        // this class inherits from “IdentityDbContext” class, you can 
        // think about this class as special version of the 
        // traditional “DbContext” Class, it will provide all of the 
        // Entity Framework code-first mapping and DbSet properties 
        // needed to manage the identity tables in SQL Server.
        public AuthContext()
            : base("AuthContext")
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}