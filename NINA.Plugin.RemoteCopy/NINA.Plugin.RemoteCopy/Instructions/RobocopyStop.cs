using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteCopy.NINAPlugin.Instructions {

    [ExportMetadata("Name", "Robocopy Stop")]
    [ExportMetadata("Description", "Stops the robocopy background process")]
    [ExportMetadata("Icon", "RobocopyStop.RobocopyStopSVG")]
    [ExportMetadata("Category", "Remote Copy")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class RobocopyStop : SequenceItem, IValidatable {

        [ImportingConstructor]
        public RobocopyStop() {
        }

        public RobocopyStop(RobocopyStop cloneMe) : base(cloneMe) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new RobocopyStop(this) {
                RobocopyStopDelay = RobocopyStopDelay,
            };
        }

        private const int DEFAULT_STOP_DELAY = 120;
        private int robocopyStopDelay = DEFAULT_STOP_DELAY;

        [JsonProperty]
        public int RobocopyStopDelay {
            get => robocopyStopDelay;
            set {
                robocopyStopDelay = value;
                RaisePropertyChanged();
            }
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            Logger.Info($"terminating background robocopy process");

            if (RobocopyStopDelay == 0) {
                RobocopyProcessManager.Stop();
                return Task.CompletedTask;
            }

            return WaitToStop(progress, token);
        }

        private async Task<bool> WaitToStop(IProgress<ApplicationStatus> progress, CancellationToken token) {
            try {
                await CoreUtil.Wait(TimeSpan.FromSeconds(RobocopyStopDelay), token, progress, "Robocopy Stop delay");
            } catch (OperationCanceledException) {
                Logger.Warning("operation canceled, will not stop robocopy");
                throw;
            }

            Logger.Info("duration elapsed, stopping robocopy");
            RobocopyProcessManager.Stop();

            return true;
        }

        private IList<string> issues = new List<string>();
        public IList<string> Issues { get => issues; set { issues = value; RaisePropertyChanged(); } }

        public bool Validate() {
            var i = new List<string>();

            if (RobocopyStopDelay < 0) {
                i.Add("delay must be >= 0");
            }

            Issues = i;
            return i.Count == 0;
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(RobocopyStop)}, Delay: {RobocopyStopDelay}";
        }
    }
}