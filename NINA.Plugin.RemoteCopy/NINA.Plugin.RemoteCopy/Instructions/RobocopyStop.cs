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
    [ExportMetadata("Icon", "RobocopyStop.RobocopyStopSVG")] // TODO: SVG ...
    [ExportMetadata("Category", "Remote Copy")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class RobocopyStop : SequenceItem, IValidatable {

        [ImportingConstructor]
        public RobocopyStop() {
        }

        // TODO: we should add a delay parameter so we wait and give it some time to finish

        public RobocopyStop(RobocopyStop cloneMe) : base(cloneMe) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new RobocopyStop(this) {
                RobocopyStopDelay = RobocopyStopDelay,
            };
        }

        private int robocopyStopDelay = -1;
        private readonly int DEFAULT_STOP_DELAY = 70;

        [JsonProperty]
        public int RobocopyStopDelay {
            get => robocopyStopDelay;
            set {
                robocopyStopDelay = robocopyStopDelay == -1 ? DEFAULT_STOP_DELAY : value;
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

            var progressRouter = new Progress<double>((p) => {
                progress.Report(new ApplicationStatus() {
                    Status = "robocopy stop delay",
                    Progress = p
                });
            });

            try {
                await CoreUtil.Wait(TimeSpan.FromSeconds(RobocopyStopDelay), token, progress);
            }
            catch (OperationCanceledException) {
                Logger.Debug("operation canceled, will not stop robocopy");
                throw;
            }

            Logger.Debug("duration elapsed, stopping robocopy");
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
