[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(TestSetup),
    nameof(TestSetup.DisableLicenseReminder))]
public static class TestSetup
{
    public static void DisableLicenseReminder()
    {
        // Acknowledge license to suppress the warning
        FluentAssertions.License.Accepted = true;
    }
}
