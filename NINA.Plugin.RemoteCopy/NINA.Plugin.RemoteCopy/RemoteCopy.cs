using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using RemoteCopy.NINAPlugin.Properties;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RemoteCopy.NINAPlugin {

    [Export(typeof(IPluginManifest))]
    public class RemoteCopyPlugin : PluginBase, INotifyPropertyChanged {

        [ImportingConstructor]
        public RemoteCopyPlugin() {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }
        }

        public override Task Teardown() {
            // final chance to stop this if not already stopped
            RobocopyProcessManager.Stop();
            return base.Teardown();
        }

        public bool RobocopyShowWindowEnabled {
            get => Settings.Default.RobocopyShowWindowEnabled;
            set {
                Settings.Default.RobocopyShowWindowEnabled = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string RobocopyOptions
        {
            get => Settings.Default.RobocopyOptions;
            set
            {
                Settings.Default.RobocopyOptions = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string RobocopyDefaultSrc
        {
            get => Settings.Default.RobocopyDefaultSrc;
            set
            {
                Settings.Default.RobocopyDefaultSrc = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string RobocopyDefaultDst
        {
            get => Settings.Default.RobocopyDefaultDst;
            set
            {
                Settings.Default.RobocopyDefaultDst = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
