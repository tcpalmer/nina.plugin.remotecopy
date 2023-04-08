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

    [ExportMetadata("Name", "Auto Robocopy")]
    [ExportMetadata("Description", "Starts the robocopy background process and uses the source and destination paths from the settings")]
    [ExportMetadata("Icon", "RobocopyStart.RobocopyStartSVG")]
    [ExportMetadata("Category", "Remote Copy")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AutoRobocopy : SequenceItem, IValidatable {

        private readonly IProfileService profileService;

        public ICommand DstDialogCommand { get; private set; }

        [ImportingConstructor]
        public AutoRobocopy(IProfileService profileService) {
            this.profileService = profileService;
        }

        public AutoRobocopy(AutoRobocopy cloneMe) : this(cloneMe.profileService)
        {
            CopyMetaData(cloneMe);
        }

        [JsonProperty]
        public string RobocopyDst {
            get => Properties.Settings.Default.RobocopyDefaultDst;
        }

        [JsonProperty]
        public string RobocopySrc {
            get => string.IsNullOrEmpty(Properties.Settings.Default.RobocopyDefaultSrc) ? profileService?.ActiveProfile?.ImageFileSettings?.FilePath : Properties.Settings.Default.RobocopyDefaultSrc;
        }

        public override object Clone() {
            return new AutoRobocopy(this);
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

            // Trim any trailing backslashes from src and dst
            char[] trim = { '\\' };
            string src = RobocopySrc.TrimEnd(trim);
            string dst = RobocopyDst.TrimEnd(trim);

            return $"\"{src}\" \"{dst}\" {defaultOptions}{logging}";
        }

        private IList<string> issues = new List<string>();
        public IList<string> Issues { get => issues; set { issues = value; RaisePropertyChanged(); } }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(RobocopyStart)}, Src: {RobocopySrc}, Dst: {RobocopyDst}";
        }

        public bool Validate()
        {
            return !string.IsNullOrEmpty(RobocopyDst);
        }
    }
}
