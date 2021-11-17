using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;

namespace TiffJpegExtractor
{
    internal static class CommandDefinitions
    {
        public static void SetupMergeCommand(Command command)
        {
            command.Description = "Extract JPEG images from TIFF/JPEG file into a directory without decoding any image data.";

            command.AddOption(Output());

            command.AddArgument(new Argument<FileInfo>()
            {
                Name = "input",
                Description = "The TIFF/JPEG files to extract from.",
                Arity = ArgumentArity.ExactlyOne
            }.ExistingOnly());

            command.Handler = CommandHandler.Create<FileInfo, DirectoryInfo, CancellationToken>(ExtractAction.Extract);

            static Option Output() =>
                new Option<DirectoryInfo>(new[] { "--output", "--out", "-o" }, "Output directory.")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly();
        }
    }
}
