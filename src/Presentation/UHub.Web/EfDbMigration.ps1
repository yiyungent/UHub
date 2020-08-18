dotnet ef migrations add InitialAspNetCoreIdentityDbMigration -c ApplicationDbContext -o Data/Migrations/AspNetCoreIdentityDb

dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb

dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb