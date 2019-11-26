using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;

namespace TiffJpegWrapper
{
    internal static class CommandDefinitions
    {
        public static void SetupWrapCommand(Command command)
        {
            command.Description = "Creates a single-strip TIFF file that wraps the specified JPEG image.";

            command.AddOption(Output());

            command.AddArgument(new Argument<FileInfo>()
            {
                Name = "source",
                Description = "The JPEG image to wrap.",
                Arity = ArgumentArity.ExactlyOne
            });

            command.Handler = CommandHandler.Create<FileInfo, FileInfo, CancellationToken>(WrapAction.Wrap);

            static Option Output() =>
                new Option(new[] { "--output", "--out", "-o" }, "Output TIFF file location.")
                {
                    Argument = new Argument<FileInfo>() { Arity = ArgumentArity.ExactlyOne }
                };
        }
    }
}
