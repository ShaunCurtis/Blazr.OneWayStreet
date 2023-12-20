/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Infrastructure;

/// <summary>
///  Executes commands on the record and persists it to the data store 
/// </summary>
/// <typeparam name="TDbContext">Database Context to apply comnmand to</typeparam>
/// <typeparam name="TInRecord">Domain Record to be Mapped From</typeparam>
/// <typeparam name="TOutRecord">Infrasrtructure Record to be mapped to to persist</typeparam>
public sealed class MappedCommandServerHandler<TDbContext, TInRecord, TOutRecord>
    : ICommandHandler<TInRecord>
    where TDbContext : DbContext
    where TOutRecord : class
    where TInRecord : class, ICommandEntity
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<TDbContext> _factory;

    public MappedCommandServerHandler(IServiceProvider serviceProvider, IDbContextFactory<TDbContext> factory)
    {
        _serviceProvider = serviceProvider;
        _factory = factory;
    }

    public async ValueTask<CommandResult> ExecuteAsync(CommandRequest<TInRecord> request)
    {
        return await this.ExecuteCommandAsync(request);
    }

    private async ValueTask<CommandResult> ExecuteCommandAsync(CommandRequest<TInRecord> request)
    {
        // Check if command operations are allowed on the TInRecord object
        if ((request.Item is not ICommandEntity))
            return CommandResult.Failure($"{request.Item.GetType().Name} Does not implement ICommandEntity and therefore you can't Update/Add/Delete it directly.");

        // Check we have a mapper for converting the TInRecord domain object to TInRecord DbContext object
        IDboEntityMap<TOutRecord, TInRecord>? mapper = null;
        mapper = _serviceProvider.GetService<IDboEntityMap<TOutRecord, TInRecord>>();

        if (mapper is null)
            return CommandResult.Failure($"No mapper is defined for {this.GetType().FullName} for {(typeof(TOutRecord).FullName)}");

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

            dbContext.Add<TOutRecord>(dboRecord);
            recordsAffected = await dbContext.SaveChangesAsync(request.Cancellation);
        }

        // Check if we should delete it
        if (request.State == CommandState.Delete)
        {
            success = "Record Deleted";
            failure = "Error Deleting Record";

            dbContext.Remove<TOutRecord>(dboRecord);
            recordsAffected = await dbContext.SaveChangesAsync(request.Cancellation);
        }

        // Finally check if it's a update
        if (request.State == CommandState.Update)
        {
            success = "Record Updated";
            failure = "Error Updating Record";

            dbContext.Update<TOutRecord>(dboRecord);
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