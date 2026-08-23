using Microsoft.EntityFrameworkCore;

namespace Moon.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
