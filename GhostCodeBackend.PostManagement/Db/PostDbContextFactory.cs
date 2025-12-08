using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GhostCodeBackend.PostManagement.Db;

public class PostsDbContextFactory : IDesignTimeDbContextFactory<PostsDbContext>
{
    public PostsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostsDbContext>();

        // Используем локальную БД ТОЛЬКО для миграций
        // Эта строка НЕ попадает в продакшен!
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=ghostcode;Username=postgres;Password=dev"
        );

        return new PostsDbContext(optionsBuilder.Options);
    }
}