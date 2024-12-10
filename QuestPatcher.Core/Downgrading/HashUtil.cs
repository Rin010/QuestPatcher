using System.IO;
using System.IO.Hashing;
using System.Threading.Tasks;
using Serilog;

namespace QuestPatcher.Core.Downgrading
{
    public static class HashUtil
    {
        public static Task<bool> CheckCrc32Async(string filePath, uint expected)
        {
            return Task.Factory.StartNew(() =>
            {
                Log.Debug("Checking CRC32 of {FilePath}", filePath);
                var crc = new Crc32();
                using var stream = File.OpenRead(filePath);
                crc.Append(stream);
                return expected == crc.GetCurrentHashAsUInt32();
            }, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
        }
    }
}
