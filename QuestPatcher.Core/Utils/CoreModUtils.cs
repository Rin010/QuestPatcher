using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace QuestPatcher.Core.Utils;

public class CoreModUtils
{
    public static readonly CoreModUtils Instance = new();
    
    private const string BeatSaberCoreModsUrl = @"https://raw.githubusercontent.com/QuestPackageManager/bs-coremods/main/core_mods.json";
    private const string BeatSaberCoreModsCnUrl = @"https://beatmods.wgzeyu.com/github/BMBFresources/com.beatgames.beatsaber/core-mods.json";
    private readonly HttpClient _client = new();
    private CoreModUtils()
    {
        
    }
    
    public bool IsCoreModsLoaded => _coreModsMap != null;

    public string PackageId
    {
        get => _coreModPackageId;
        set
        {
            if(value != _coreModPackageId)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _coreModsMap = null;
                _coreModPackageId = value;
            }
        }
    }

    private Dictionary<string, IList<CoreMod>>? _coreModsMap;  // game version: core mods
    private string _coreModPackageId = SharedConstants.BeatSaberPackageID;

    private CancellationTokenSource? _cancellationTokenSource;
    
    //TODO indicate loading success/failure
    public async Task RefreshCoreMods()
    {
        Log.Information("Refreshing Core Mods");

        if (PackageId != SharedConstants.BeatSaberPackageID)
        {
            Log.Warning("There are no core mods known for this game");
            // currently there are only core mods organized for Beat Saber.
            _coreModsMap = new Dictionary<string, IList<CoreMod>>();
            return;
        }
        
        // Cancel a previous refresh attempt in case it is not finished
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        // if (!await LoadCoreMods(BeatSaberCoreModsUrl, token) && !await LoadCoreMods(BeatSaberCoreModsCnUrl, token))
        if (!await LoadCoreMods(BeatSaberCoreModsUrl, token))  // disable cn core mods url
        {
            Log.Error("Cannot fetch core mods from any source");
        }
        else
        {
            Log.Information("Core mods refreshed");
            Log.Debug("Core mods: {CoreMods}", _coreModsMap);
        }
    }

    private async Task<bool> LoadCoreMods(string url, CancellationToken token)
    {
        Log.Debug("Fetching core mods from {Url}", url);
        try
        {
            var res = await _client.GetStringAsync(url, token);
            var jObj = JsonNode.Parse(res)!.AsObject();
            var coreModsMap = new Dictionary<string, IList<CoreMod>>();
            foreach (var pair in jObj)
            {
                var packageVersion = pair.Key;
                var coreModsNode = pair.Value?["mods"]?.AsArray() ?? new JsonArray();

                coreModsMap[packageVersion] = coreModsNode.Deserialize<IList<CoreMod>>() ?? new List<CoreMod>();
            }
            
            _coreModsMap = coreModsMap;
            return true;
        }
        catch(Exception e)
        {
            Log.Warning(e, "Cannot fetch core mods from {Url}", url);
            // we don't want to overwrite what we previously have 
            return false;
        }
    }

    public IList<CoreMod> GetCoreMods(string packageVersion)
    {
        if (_coreModsMap?.TryGetValue(packageVersion, out var coreMods) == true)
        {
            return coreMods;
        }

        return new List<CoreMod>();
    }
    
    public readonly struct CoreMod
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }
        
        [JsonPropertyName("version")]
        public string Version { get; init; }
        
        [JsonPropertyName("downloadLink")]
        public Uri DownloadUrl { get; init; }
        
        [JsonPropertyName("filename")]
        public string? Filename { get; init; }

        public CoreMod(string id, string version, string downloadLink, string filename)
        {
            Id = id;
            Version = version;
            DownloadUrl = new Uri(downloadLink);
            Filename = filename;
        }
        
        public override string ToString()
        {
            return $"{Id}@{Version}";
        }
    }
}
