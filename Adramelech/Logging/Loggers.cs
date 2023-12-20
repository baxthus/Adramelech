using Serilog;
using Serilog.Core;

namespace Adramelech.Logging;

public static class Loggers
{
    public static readonly Logger Default = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

    public static readonly Logger UserContext = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({User}) {Message:lj}{NewLine}{Exception}")
        .CreateLogger();
}