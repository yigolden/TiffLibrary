using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace TiffJpegWrapper
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new CommandLineBuilder();

            CommandDefinitions.SetupWrapCommand(builder.Command);

            builder.UseVersionOption();

            builder.UseHelp();
            builder.UseSuggestDirective();
            builder.RegisterWithDotnetSuggest();
            builder.UseParseErrorReporting();
            builder.UseExceptionHandler();

            Parser parser = builder.Build();
            await parser.InvokeAsync(args);
        }
    }
}
