namespace Enver.QuotaControl.Bootstrap;

internal static class BootstrapReporter
{
    public static void Write(TextWriter writer, BootstrapResult result)
    {
        if (result.Outcome == BootstrapOutcome.Passed)
        {
            writer.WriteLine("BOOTSTRAP PASSED: initialize + readback exact");
            return;
        }

        var detail = result.Detail.Count == 0 ? string.Empty : " (" + string.Join(',', result.Detail) + ")";
        writer.WriteLine("BOOTSTRAP STOPPED: " + result.Code + detail);
    }
}
