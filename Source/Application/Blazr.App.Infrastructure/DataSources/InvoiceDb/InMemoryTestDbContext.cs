/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public sealed class InMemoryTestDbContext
    : DbContext
{
    public DbSet<WeatherForecast> weatherForecasts { get; set; } = default!;

    internal DbSet<DboWeatherForecast> dboWeatherForecasts { get; set; } = default!;

    public InMemoryTestDbContext(DbContextOptions<InMemoryTestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DboWeatherForecast>().ToTable("WeatherForecasts");

        modelBuilder.Entity<WeatherForecast>()
            .ToInMemoryQuery(()
            => from w in this.dboWeatherForecasts
               select new WeatherForecast
               {
                   WeatherForecastUid = new(w.Uid),
                   Summary = w.Summary,
                   Date = w.Date,
                   TemperatureC = w.TemperatureC,
               }).HasNoKey();
    }
}
