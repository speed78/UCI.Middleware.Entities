using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UCI.Middleware.Entities.Context
{
    public class UciDbContextFactory : IDesignTimeDbContextFactory<UciDbContext>
    {
        public UciDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UciDbContext>();
            var connectionString = "Server=tcp:uciivas.database.windows.net,1433;Initial Catalog=UCI;User ID=salvatore.cossu;Password=!Speed23522;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            optionsBuilder.UseSqlServer(connectionString);

            return new UciDbContext(optionsBuilder.Options);
        }
    }
}