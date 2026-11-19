namespace Enver.QuotaControl;

public sealed record ApplyOutcome(string Status, long ServerTimeMicroseconds, bool ClosedReadbackVerified);

public sealed class ControlService(IRedisCommandClient client)
{
    public async Task<InventoryResult> InventoryAsync(ApprovedDescriptor descriptor, CancellationToken token)
    {
        descriptor.Validate();
        return await new InventoryReader(client).ReadAsync(token).ConfigureAwait(false);
    }

    public async Task<InventoryResult> PrecheckAsync(ApprovedDescriptor descriptor, CancellationToken token)
    {
        var inventory = await InventoryAsync(descriptor, token).ConfigureAwait(false);
        if (!inventory.IsEmpty) throw new ControlException(Failure.ResidueFound);
        return inventory;
    }

    public async Task<ClosedPlan> ReadbackAsync(ApprovedDescriptor descriptor, string generation, CancellationToken token)
    {
        descriptor.Validate();
        Contract.ValidateGeneration(generation);
        var inventory = await InventoryAsync(descriptor, token).ConfigureAwait(false);
        if (inventory.Metadata.Type != "hash" || inventory.Metadata.Fields is null ||
            inventory.Registry.Type != "none" || inventory.BucketCount != 0 || inventory.Discrepancies != 0)
            throw new ControlException(Failure.ReadbackMismatch);
        return ClosedPlan.ValidateReadback(descriptor, generation, inventory.Metadata.Fields, inventory.Metadata.Pttl);
    }

    // Deliberadamente inaccesible a red: solo dobles locales, no flag CLI de autorizacion.
    // Un transporte operativo y evidencia humana deben implementarse/aprobarse en otro incremento.
    public async Task<ApplyOutcome> ApplyClosedAsync(ApprovedDescriptor descriptor, string generation, CancellationToken token)
    {
        descriptor.Validate();
        Contract.ValidateGeneration(generation);
        if (!client.IsLocalSimulation) throw new ControlException(Failure.ApplyDisabled);
        var session = client.SessionIdentity;
        await PrecheckAsync(descriptor, token).ConfigureAwait(false);
        var command = RedisCommand.Recover(descriptor, generation);
        if (!ReferenceEquals(session, client.SessionIdentity)) throw new ControlException(Failure.InventoryIncomplete);
        token.ThrowIfCancellationRequested();
        RedisReply response;
        try
        {
            // Ningun retry: una excepcion desde el despacho puede ocultar un HSET completado.
            response = await client.ExecuteAsync(command, token).ConfigureAwait(false);
        }
        catch { throw new ControlException(Failure.ResultUncertain); }
        if (token.IsCancellationRequested || !ReferenceEquals(session, client.SessionIdentity))
            throw new ControlException(Failure.ResultUncertain);
        var outcome = ParseApplyResponse(response);
        try
        {
            var readback = await ReadbackAsync(descriptor, generation, token).ConfigureAwait(false);
            if (readback.ServerTimeMicroseconds != outcome.ServerTimeMicroseconds)
                throw new ControlException(Failure.ReadbackMismatch);
            return outcome with { ClosedReadbackVerified = true };
        }
        catch { throw new ControlException(Failure.ResultUncertain); }
    }

    public static ApplyOutcome ParseApplyResponse(RedisReply response)
    {
        try
        {
            var parts = response.Items;
            if (parts.Count != 5 || parts[0].Text != "enver.auth.quota.recovery-closed" || parts[1].Text != "1")
                throw new ControlException(Failure.ResultUncertain);
            var status = parts[2].Text;
            var reason = parts[3].Text;
            if (status == "aborted" && parts[4].Text == "0")
                throw new ControlException(reason switch
                {
                    "metadata_exists" => Failure.MetadataExists,
                    "registry_exists" => Failure.RegistryExists,
                    "invalid" => Failure.InvalidArguments,
                    "time" => Failure.InvalidTime,
                    _ => Failure.ResultUncertain
                });
            if (status != "created_closed" || reason != "ok") throw new ControlException(Failure.ResultUncertain);
            long time;
            try
            {
                time = Contract.ParseUnsigned(parts[4].Text);
                Contract.ValidateTime(time);
            }
            catch { throw new ControlException(Failure.ResultUncertain); }
            return new("created_closed", time, false);
        }
        catch (ControlException exception) when (exception.Failure is Failure.MetadataExists or Failure.RegistryExists or Failure.InvalidArguments or Failure.InvalidTime)
        { throw; }
        catch { throw new ControlException(Failure.ResultUncertain); }
    }
}
