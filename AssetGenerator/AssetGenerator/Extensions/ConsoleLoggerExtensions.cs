using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace AssetGenerator.Extensions
{
    /// <summary>
    /// Custom logging formatter for console logs.
    /// It formats the log messages in a specific way, including the log level, timestamp, and message.
    /// </summary>
    public static class ConsoleLoggerExtensions
    {
        /// <summary>
        /// Adds a custom console log formatter to the logging builder, enabling the use of a formatter named
        /// "CustomFormatter" for console log output.
        /// </summary>
        /// <remarks>Use this method to register and configure a custom console formatter for logging
        /// output. The formatter is identified by the name "CustomFormatter" and can be further customized using the
        /// provided configuration action.</remarks>
        /// <param name="builder">The logging builder to which the custom formatter will be added. Cannot be null.</param>
        /// <param name="configure">An action that configures the options for the custom console formatter. This allows customization of how log
        /// messages are formatted in the console.</param>
        /// <returns>The logging builder instance with the custom console formatter configured. This enables further chaining of
        /// logging configuration methods.</returns>
        public static ILoggingBuilder AddCustomFormatter(
            this ILoggingBuilder builder,
            Action<ConsoleFormatterOptions> configure)
        {
            return builder
                .AddConsole(options => options.FormatterName = "CustomFormatter")
                .AddConsoleFormatter<CustomFormatter, ConsoleFormatterOptions>(configure);
        }


        private class CustomFormatter : ConsoleFormatter
        {
            public CustomFormatter() : base("CustomFormatter")
            {
            }

            public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
            {
                textWriter.WriteLine($"[{logEntry.LogLevel.ToString()[0]}] {DateTime.Now.ToString("HH:mm:ss")} {logEntry.Formatter(logEntry.State, logEntry.Exception)}");
            }
        }
    }
}
