using Sentry.Serilog;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Adramelech.Logging;

public static class Loggers
{
    private static readonly Action<SentrySerilogOptions> SentryOptions = o =>
    {
        o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
        o.MinimumEventLevel = LogEventLevel.Error;
        o.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
        o.AttachStacktrace = true;
        o.SendDefaultPii = true;
    };

    public static readonly Logger Default = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console()
// Don't send to Sentry in debug mode
#if !DEBUG
.WriteTo.Sentry(SentryOptions)
#endif
        .CreateLogger();

    public static readonly Logger UserContext = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({User}) {Message:lj}{NewLine}{Exception}")
        .CreateLogger();
}