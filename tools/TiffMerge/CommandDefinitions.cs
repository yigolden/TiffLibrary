using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;

namespace TiffMerge
{
    internal static class CommandDefinitions
    {
        public static void SetupMergeCommand(Command command)
        {
            command.Description = "Merge multiple TIFF files into a single file by copying IFDs into the new file.";

            command.AddOption(Output());

            command.AddArgument(new Argument<FileInfo[]>()
            {
                Name = "source",
                Description = "The TIFF files to copy from.",
                Arity = ArgumentArity.OneOrMore
            });

            command.Handler = CommandHandler.Create<FileInfo[], FileInfo, CancellationToken>(MergeAction.Merge);

            static Option Output() =>
                new Option(new[] { "--output", "--out", "-o" }, "Output TIFF file.")
                {
                    Argument = new Argument<FileInfo>() { Arity = ArgumentArity.ExactlyOne }
                };
        }
    }
}
