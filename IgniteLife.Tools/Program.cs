using IgniteLife.Tools.Commands;

if (args.Length == 0)
{
    SeedAdminCommand.WriteUsage();
    SeedBookingRulesCommand.WriteUsage();
    return;
}

var cmd = args[0].ToLowerInvariant();
var rest = args.Skip(1).ToArray();

switch (cmd)
{
    case var s when s == SeedAdminCommand.Name:
        await SeedAdminCommand.RunAsync(rest);
        break;

    case var s when s == SeedBookingRulesCommand.Name:
        await SeedBookingRulesCommand.RunAsync(rest);
        break;

    default:
        Console.WriteLine($"Unknown command '{cmd}'.");
        SeedAdminCommand.WriteUsage();
        SeedBookingRulesCommand.WriteUsage();
        break;
}
