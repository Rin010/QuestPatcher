using System.IO;
using System.Threading.Tasks;

namespace QuestPatcher.Core.Downgrading
{
    public static class FilePatcher
    {
        private static void PatchFile(string inputPath, string outputPath, string diffPath)
        {
            using var inStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
            using var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            BsDiff.BinaryPatch.Apply(inStream, () => new FileStream(diffPath, FileMode.Open, FileAccess.Read), outStream);
        }
        
        public static async Task PatchFileAsync(string inputPath, string outputPath, string diffPath)
        {
            await Task.Factory.StartNew(() => PatchFile(inputPath, outputPath, diffPath), TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
        }
    }
}
