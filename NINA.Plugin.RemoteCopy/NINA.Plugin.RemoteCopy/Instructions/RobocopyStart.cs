using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Profile.Interfaces;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteCopy.NINAPlugin.Instructions {

    [ExportMetadata("Name", "Robocopy Start")]
    [ExportMetadata("Description", "Starts the robocopy background process")]
    [ExportMetadata("Icon", "RobocopyStart.RobocopyStartSVG")]
    [ExportMetadata("Category", "Remote Copy")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class RobocopyStart : SequenceItem, IValidatable {

        private readonly IProfileService profileService;

        public ICommand SrcDialogCommand { get; private set; }
        public ICommand DstDialogCommand { get; private set; }

        [ImportingConstructor]
        public RobocopyStart(IProfileService profileService) {
            this.profileService = profileService;
            SrcDialogCommand = new RelayCommand(showSrcDialog);
            DstDialogCommand = new RelayCommand(showDstDialog);
        }

        public RobocopyStart(RobocopyStart cloneMe) : this(cloneMe.profileService) {
            CopyMetaData(cloneMe);
        }

        private string robocopySrc = "";

        [JsonProperty]
        public string RobocopySrc {
            get => robocopySrc;
            set {
                robocopySrc = String.IsNullOrEmpty(value) ? profileService?.ActiveProfile?.ImageFileSettings?.FilePath : value;
                RaisePropertyChanged();
            }
        }

        private string robocopyDst = "";

        [JsonProperty]
        public string RobocopyDst {
            get => robocopyDst;
            set {
                robocopyDst = value;
                RaisePropertyChanged();
            }
        }

        public override object Clone() {
            return new RobocopyStart(this) {
                RobocopySrc = RobocopySrc,
                RobocopyDst = RobocopyDst
            };
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            bool showProcessWindow = Properties.Settings.Default.RobocopyShowWindowEnabled;
            Logger.Debug($"execution: src={RobocopySrc} dst={RobocopyDst} showProcessWindow={showProcessWindow}");
            RobocopyProcessManager.Start("robocopy", $"{GetOptions(showProcessWindow)}", showProcessWindow);
            return Task.CompletedTask;
        }

        private string GetOptions(bool showProcessWindow) {

            string defaultOptions = Properties.Settings.Default.RobocopyOptions;
            string logging = "";

            // Automatically add log output if robocopy cmd window is not shown and user didn't override options and add logging
            Regex re = new Regex(@"/(uni)?log\+?:", RegexOptions.IgnoreCase);
            if (!showProcessWindow && !re.IsMatch(defaultOptions)) {
                string logFileName = $"robocopy-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.log";
                string logFilePath = Path.Combine(CoreUtil.APPLICATIONTEMPPATH, "Logs", logFileName);
                logging = $" /log+:\"{logFilePath}\"";
                Logger.Debug($"robocopy log: {logFilePath}");
            }
            else {
                Logger.Debug($"no automatic log added for robocopy");
            }

            return $"\"{RobocopySrc}\" \"{RobocopyDst}\" {defaultOptions}{logging}";
        }

        private IList<string> issues = new List<string>();
        public IList<string> Issues { get => issues; set { issues = value; RaisePropertyChanged(); } }

        public bool Validate() {
            var i = new List<string>();
            string srcPath = null;
            string dstPath = null;

            if (String.IsNullOrEmpty(RobocopySrc)) {
                i.Add("source folder required");
            }
            else if (!Directory.Exists(RobocopySrc)) {
                i.Add("source folder does not exit");
            }
            else {
                try {
                    srcPath = Path.GetFullPath(RobocopySrc);
                }
                catch {
                    i.Add("source is not a valid path");
                }
            }

            if (String.IsNullOrEmpty(RobocopyDst)) {
                i.Add("destination folder required");
            }
            else if (!Directory.Exists(RobocopyDst)) {
                i.Add("destination folder does not exit");
            }
            else {
                try {
                    dstPath = Path.GetFullPath(RobocopyDst);
                }
                catch {
                    i.Add("destination is not a valid path");
                }
            }

            if (srcPath != null && dstPath != null) {
                if (string.Equals(srcPath, dstPath, StringComparison.OrdinalIgnoreCase)) {
                    i.Add("source and destination folders cannot be the same");
                }
            }

            Issues = i;
            return i.Count == 0;
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(RobocopyStart)}, Src: {RobocopySrc}, Dst: {RobocopyDst}";
        }

        private void showSrcDialog(object obj) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Robocopy Source Folder";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = RobocopySrc;

            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) {
                RobocopySrc = dialog.FileName;
            }
        }

        private void showDstDialog(object obj) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Robocopy Destination Folder";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = RobocopyDst;

            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) {
                RobocopyDst = dialog.FileName;
            }
        }
    }
}
