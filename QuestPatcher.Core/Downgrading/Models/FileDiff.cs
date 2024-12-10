using System.Text.Json.Serialization;

namespace QuestPatcher.Core.Downgrading.Models
{
    /*
  {
  "diff_name": "bs1.36-1.35.apk.diff",
  "file_name": "bs136.apk",
  "file_crc": 1675847848,
  "output_file_name": "bs135.apk",
  "output_crc": 2088061822,
  "output_size": 49104123
}
 */
    

    public class FileDiff
    {
        [JsonPropertyName("diff_name")]
        public string DiffName { get; set; }

        [JsonPropertyName("file_name")]
        public string FileName { get; set; }

        [JsonPropertyName("file_crc")]
        public uint FileCrc { get; set; }

        [JsonPropertyName("output_file_name")]
        public string OutputFileName { get; set; }

        [JsonPropertyName("output_crc")]
        public uint OutputCrc { get; set; }

        [JsonPropertyName("output_size")]
        public long OutputSize { get; set; }
    }
}
