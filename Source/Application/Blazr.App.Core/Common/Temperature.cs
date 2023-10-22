/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public readonly record struct Temperature
{
    private readonly decimal _temperature;
    public decimal TemperatureC => _temperature;
    public decimal TemperatureF => ((_temperature * 5) / 8) + 32;
    public decimal TemperatureK => _temperature + 273;

    public Temperature(decimal temperatureC)
    {
        _temperature = temperatureC;
    }
}
