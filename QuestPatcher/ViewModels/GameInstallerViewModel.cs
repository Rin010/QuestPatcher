using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using QuestPatcher.Core;
using QuestPatcher.Core.Modding;
using QuestPatcher.Models;
using QuestPatcher.Services;
using ReactiveUI;
using Serilog;

namespace QuestPatcher.ViewModels
{
    public class GameInstallerViewModel : ViewModelBase
    {
        private readonly Window _window;
        private readonly QuestPatcherUiService _uiService;
        private readonly OperationLocker _locker;
        private readonly InstallManager _installManager;
        private readonly ModManager _modManager;

        private readonly TaskCompletionSource<bool> _newAppInstalledTaskSource = new();

        private bool _appInstallSucceeded = false;
        
        public bool IsVersionSwitching { get; }
        
        public Task<bool> NewAppInstalled => _newAppInstalledTaskSource.Task;
        
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                this.RaisePropertyChanged();
            }
        }

        private string _apkPath = "";
        public string ApkPath
        {
            get => _apkPath;
            private set
            {
                _apkPath = value;
                this.RaisePropertyChanged();
            }
        }

        private IReadOnlyList<string> _obbPaths = new List<string>();
        public IReadOnlyList<string> ObbPaths {
            get => _obbPaths;
            private set
            {
                _obbPaths = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(ObbSelected));
            }
        }

        public bool ObbSelected => ObbPaths.Count > 0;

        public GameInstallerViewModel(bool isVersionSwitching, Window window, QuestPatcherUiService uiService, OperationLocker locker, InstallManager installManager, ModManager modManager)
        {
            IsVersionSwitching = isVersionSwitching;
            _window = window;
            _uiService = uiService;
            _locker = locker;
            _installManager = installManager;
            _modManager = modManager;
            
            window.Closing += (sender, args) =>
            {
                if (IsLoading)
                {
                    args.Cancel = true;
                    return;
                }

                _newAppInstalledTaskSource.TrySetResult(_appInstallSucceeded);
            };
        }


        public async Task SelectApk()
        {
            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false, FileTypeFilter = new[] { new FilePickerFileType("Beat Saber APK") { Patterns = new[] { "*.apk" } } }
            };
            var files = await _window.StorageProvider.OpenFilePickerAsync(options);
            if (files.Count == 0)
            {
                ApkPath = "";
                return;
            }
            string file = files[0].Path.LocalPath;
            if (!file.EndsWith(".apk"))
            {
                DialogBuilder builder1 = new()
                {
                    Title = "你选择的文件有误",
                    Text = "你选择的文件有误，请重新选择。",
                    HideCancelButton = true
                };
                await builder1.OpenDialogue(_window);
                return;
            }
            ApkPath = file;
        }

        public async Task SelectObb()
        {
            var options = new FilePickerOpenOptions
            {
                AllowMultiple = true, FileTypeFilter = new[] { new FilePickerFileType("Obb 文件") { Patterns = new[] { "*.obb" } } }
            };
            var files = await _window.StorageProvider.OpenFilePickerAsync(options);
            if (files.Count == 0)
            {
                ObbPaths = new List<string>();
                return;
            }

            ObbPaths = files.Select(file => file.Path.LocalPath).ToList();
        }

        public async Task InstallGame()
        {
            if (string.IsNullOrWhiteSpace(ApkPath)) return;
            
            var builder1 = new DialogBuilder
            {
                Title = "即将开始安装",
                Text = "安装可能需要两分钟左右，该过程中将暂时无法点击软件窗口，请耐心等待，\n点击下方“好的”按钮，即可开始安装。",
            };
            if (!await builder1.OpenDialogue(_window)) return;
            
            Log.Debug("Installing game with {ApkPath} and {ObbCount} obb files", ApkPath, ObbPaths.Count);
            _locker.StartOperation();
            IsLoading = true;
            try
            {
                if (IsVersionSwitching)
                {
                    await _modManager.DeleteAllMods();
                    await _installManager.ReplaceApp(ApkPath, ObbPaths);
                }
                else
                {
                    await _installManager.InstallApp(ApkPath, ObbPaths);
                }

                _appInstallSucceeded = true;
                // close the window after we're done, caller will handle success prompt
            }
            catch (Exception e)
            {
                Log.Error(e, "Install game failed with exception: {Exception}", e.Message);
                _appInstallSucceeded = false;
                var dialog = new DialogBuilder
                {
                    Title = "出错了！",
                    Text = (IsVersionSwitching ? "更换版本的过程中" : "在安装游戏时") + "发生了意料之外的错误，检查日志以获取详细信息",
                    HideCancelButton = true
                };
                
                dialog.WithException(e);
                await dialog.OpenDialogue(_window);
            }
            finally
            {
                _locker.FinishOperation();
                IsLoading = false;
                _window.Close();
            }
        }
    }
}
