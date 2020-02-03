using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace TiffMerge
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new CommandLineBuilder();

            CommandDefinitions.SetupMergeCommand(builder.Command);

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
