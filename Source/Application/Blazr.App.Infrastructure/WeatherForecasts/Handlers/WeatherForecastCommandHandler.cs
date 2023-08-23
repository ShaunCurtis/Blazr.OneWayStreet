/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Infrastructure;

public sealed class WeatherForecastCommandHandler<TDbContext>
    : ICommandHandler<WeatherForecast>
    where TDbContext : DbContext
{
    private ILogger<WeatherForecastCommandHandler<TDbContext>> _logger;
    private readonly ICommandHandler _commandHandler;
    private readonly IDboEntityMap<DboWeatherForecast, WeatherForecast> _mapper;

    public WeatherForecastCommandHandler(ILogger<WeatherForecastCommandHandler<TDbContext>> logger, ICommandHandler commandHandler, IDboEntityMap<DboWeatherForecast, WeatherForecast> mapper)
    {
        _logger = logger;
        _commandHandler = commandHandler;
        _mapper = mapper;
    }

    public ValueTask<CommandResult> ExecuteAsync(CommandRequest<WeatherForecast> request)
    {
        var dbo = _mapper.MapTo(request.Item);
        if (dbo is null)
        {
            var message = $"Could not map {request.Item.GetType()} to it's Dbo object.";
            _logger.LogError(message);
            return ValueTask.FromResult(CommandResult.Failure(message));
        }
        var newRequest = new CommandRequest<DboWeatherForecast>(dbo, request.State);
        return  _commandHandler.ExecuteAsync(newRequest);
    }
}
