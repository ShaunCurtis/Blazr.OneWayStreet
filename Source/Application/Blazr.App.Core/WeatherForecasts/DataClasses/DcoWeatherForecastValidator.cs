/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class DcoWeatherForecastValidator : AbstractValidator<DcoWeatherForecastEditContext>
{
    public DcoWeatherForecastValidator()
    {
        this.RuleFor(p => p.Summary)
            .MinimumLength(3)
            .WithState(p => p);
    }
}
