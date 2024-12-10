using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuestPatcher.Core.Downgrading.Models
{
    public class AppDiff
    {
        [JsonPropertyName("from_version")]
        public string FromVersion { get; set; }
        
        [JsonPropertyName("to_version")]
        public string ToVersion { get; set; }
        
        [JsonPropertyName("apk_diff")]
        public FileDiff ApkDiff { get; set; }
        
        [JsonPropertyName("obb_diffs")]
        public IList<FileDiff> ObbDiffs { get; set; }
    }
}
