/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Infrastructure;

public sealed class MappedCommandServerHandler<TDbContext, TDbo>
    : ICommandHandler
    where TDbContext : DbContext
    where TDbo : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<TDbContext> _factory;

    public MappedCommandServerHandler(IServiceProvider serviceProvider, IDbContextFactory<TDbContext> factory)
    {
        _serviceProvider = serviceProvider;
        _factory = factory;
    }

    public async ValueTask<CommandResult> ExecuteAsync<TDco>(CommandRequest<TDco> request)
        where TDco : class
    {
        // Try and get a registered custom handler
        var _customHandler = _serviceProvider.GetService<ICommandHandler<TDco>>();

        // If one exists execute it
        if (_customHandler is not null)
            return await _customHandler.ExecuteAsync(request);

        // If not run the base handler
        return await this.ExecuteCommandAsync<TDco>(request);
    }

    private async ValueTask<CommandResult> ExecuteCommandAsync<TDco>(CommandRequest<TDco> request)
    where TDco : class
    {
        // Check if command operations are allowed on the TDco object
        if ((request.Item is not ICommandEntity))
            return CommandResult.Failure($"{request.Item.GetType().Name} Does not implement ICommandEntity and therefore you can't Update/Add/Delete it directly.");

        // Check we have a mapper for converting the TDco domain object to TDco DbContext object
        IDboEntityMap<TDbo, TDco>? mapper = null;
        mapper = _serviceProvider.GetService<IDboEntityMap<TDbo, TDco>>();
        if (mapper is null)
            return CommandResult.Failure($"No mapper is defined for {this.GetType().FullName} for {(typeof(TDbo).FullName)}");

        var dboRecord = mapper.MapTo(request.Item);

        using var dbContext = _factory.CreateDbContext();

        string success = "Action completed";
        string failure = $"Nothing executed.  Unrecognised State.";
        int recordsAffected = 0;
        bool isAdd = false;

        // First check if it's new.
        if (request.State == CommandState.Add)
        {
            success = "Record Added";
            failure = "Error Adding Record";
            isAdd = true;

            dbContext.Add<TDbo>(dboRecord);
            recordsAffected = await dbContext.SaveChangesAsync(request.Cancellation);
        }

        // Check if we should delete it
        if (request.State == CommandState.Delete)
        {
            success = "Record Deleted";
            failure = "Error Deleting Record";

            dbContext.Remove<TDbo>(dboRecord);
            recordsAffected = await dbContext.SaveChangesAsync(request.Cancellation);
        }

        // Finally check if it's a update
        if (request.State == CommandState.Update)
        {
            success = "Record Updated";
            failure = "Error Updating Record";

            dbContext.Update<TDbo>(dboRecord);
            recordsAffected = await dbContext.SaveChangesAsync(request.Cancellation);
        }

        // We will have either 1 or 0 changed records
        if (recordsAffected == 1)
        {
            var isKeyed = dboRecord is IKeyedEntity;

            // Check if we need to return a database inserted key value
            if (isKeyed && isAdd)
            {
                var key = ((IKeyedEntity)dboRecord).KeyValue;
                return CommandResult.SuccessWithKey(key, success);
            }
            else
                return CommandResult.Success(success);
        }

        return CommandResult.Failure(failure);
    }
}
