using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Serilog;
using Version = SemanticVersioning.Version;

namespace QuestPatcher.Core.Models
{
    public class ApkInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// The version of the APK
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Whether or not the APK is modded with a modloader that we recognise (QuestLoader or Scotland2)
        /// </summary>
        public bool IsModded => ModLoader != null && ModLoader != Models.ModLoader.Unknown;

        /// <summary>
        /// The modloader that the APK is modded with.
        /// Null if unmodded.
        /// </summary>
        public ModLoader? ModLoader
        {
            get => _modloader;
            set
            {
                if (_modloader != value)
                {
                    _modloader = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(IsModded));
                }
            }
        }

        /// <summary>
        /// Whether or not the APK uses 64 bit binary files.
        /// </summary>
        public bool Is64Bit { get; }

        /// <summary>
        /// The path of the local APK, downloaded from the quest
        /// </summary>
        public string Path { get; }

        public Version? SemVersion { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private ModLoader? _modloader;

        public ApkInfo(string version, ModLoader? modloader, bool is64Bit, string path)
        {
            Version = version;
            _modloader = modloader;
            Is64Bit = is64Bit;
            Path = path;

            SemVersion = ParseSemVer();
            
            Log.Debug("Parsed version {Version} to SemVer {SemVer}", Version, SemVersion);
        }

        private Version? ParseSemVer()
        {
            try
            {
                if (SemanticVersioning.Version.TryParse(Version, true, out var semVersion))
                {
                    return semVersion;
                }
                
                string cleanedVersion = Version.Replace(" ", "");
                int underscoreIndex = cleanedVersion.IndexOf('_');
                if (underscoreIndex >= 0)
                {
                    cleanedVersion = cleanedVersion[..underscoreIndex] + "+" + cleanedVersion[(underscoreIndex + 1)..];
                }

                return SemanticVersioning.Version.TryParse(cleanedVersion, true, out semVersion) ? semVersion : null;

            }
            catch (Exception e)
            {
                Log.Warning(e, "Failed to parse version {Version} to SemVer", Version);
                return null;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
