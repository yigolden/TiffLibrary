using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;

namespace TiffDump
{
    internal static class CommandDefinitions
    {
        public static void SetupDumpCommand(Command command)
        {
            command.Description = "Dump TIFF file structure.";

            command.AddArgument(new Argument<FileInfo>()
            {
                Name = "file",
                Description = "The TIFF file to dump.",
                Arity = ArgumentArity.ExactlyOne
            }.ExistingOnly());

            command.Handler = CommandHandler.Create<FileInfo, CancellationToken>(DumpAction.Dump);
        }
    }
}
