using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace TiffJpegExtractor
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new CommandLineBuilder();

            CommandDefinitions.SetupMergeCommand(builder.Command);

            builder.UseDefaults();

            Parser parser = builder.Build();
            await parser.InvokeAsync(args);
        }
    }
}
