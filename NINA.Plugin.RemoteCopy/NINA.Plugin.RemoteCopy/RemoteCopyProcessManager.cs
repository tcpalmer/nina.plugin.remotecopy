using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using System;
using System.Diagnostics;
using System.Timers;

namespace RemoteCopy.NINAPlugin {

    public class RemoteCopyProcessManager {

        private static readonly int DEFAULT_TIMER_INTERVAL_MS = 10000;

        public static readonly int INACTIVE_PID = -1;

        private readonly string processName;

        private readonly string executable;

        private readonly string arguments;

        private readonly bool showProcessWindow;

        private readonly string runnable;

        private int interval = DEFAULT_TIMER_INTERVAL_MS;

        private Timer watchTimer = null;

        private int runningProcessId = INACTIVE_PID;

        private Process process;

        public RemoteCopyProcessManager(string processName, string executable, string arguments, bool showProcessWindow) {
            this.processName = processName;
            this.executable = executable;
            this.arguments = arguments;
            this.showProcessWindow = showProcessWindow;
            runnable = $"{executable} {arguments}";
        }

        public RemoteCopyProcessManager SetInterval(int interval) {
            this.interval = interval;
            return this;
        }

        public int StartProcessAndWatch() {
            if (runningProcessId != INACTIVE_PID) {
                Logger.Warning($"process is already running: '{runnable}', will not restart");
                return runningProcessId;
            }

            StartProcess();
            return process.Id;
        }

        public void StopProcess() {
            if (runningProcessId == INACTIVE_PID) {
                Logger.Debug($"watched process is not running, no need to stop: {runnable}");
                StopTimer();
                return;
            }

            Logger.Info($"stopping process: {runnable}");
            StopTimer();
            process.EnableRaisingEvents = false;

            try {
                // This is brutal but Windows doesn't provide a gentler mechanism
                process.Kill();
                runningProcessId = INACTIVE_PID;
                process = null;
            }
            catch (Exception e) {
                Logger.Error($"failed to kill watched process {runnable}: {e.Message}");
            }
        }

        private void StartProcess() {
            Logger.Info($"starting process: {runnable}");

            process = new Process();
            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.CreateNoWindow = !showProcessWindow;
            process.StartInfo.UseShellExecute = showProcessWindow;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(ProcessExited);
            bool isRunning = false;

            try {
                isRunning = process.Start();
            }
            catch (Exception) {
                Logger.Error($"failed to start process: {runnable} (check command and args)");
                Notification.ShowError($"Failed to start {processName} background process, check command and args");
                runningProcessId = INACTIVE_PID;
                isRunning = false;
            }

            if (isRunning) {
                Logger.Info($"started process: pid={process.Id}");
                Notification.ShowSuccess($"{processName} background process started");
                runningProcessId = process.Id;
                StartWatchTimer();
            }
            else {
                Logger.Error($"failed to start process: {runnable}");
            }
        }

        private void ProcessExited(object sender, System.EventArgs e) {
            Logger.Warning($"process '{runnable}' exited at {process.ExitTime} with exit code {process.ExitCode}, will attempt restart at next timer interval");
            runningProcessId = INACTIVE_PID;
        }

        private void StartWatchTimer() {
            StopTimer();
            Logger.Trace("starting timer");

            watchTimer = new Timer(interval);
            watchTimer.Elapsed += TimerElapsed;
            watchTimer.AutoReset = true;
            watchTimer.Start();
        }

        private void StopTimer() {
            if (watchTimer != null) {
                Logger.Trace("stopping timer");
                watchTimer.Stop();
                watchTimer.Dispose();
                watchTimer = null;
            }
        }

        private void TimerElapsed(Object source, ElapsedEventArgs e) {
            Logger.Debug($"checking on watched process {runnable}");

            if (!ProcessRunning()) {
                Logger.Warning($"restarting watched process: {runnable}");
                StopTimer();
                StartProcess();
            }
            else {
                Logger.Debug($"watched process is running: {runnable}");
            }
        }

        private bool ProcessRunning() {
            try {
                Process.GetProcessById(process.Id);
                return true;
            }
            catch (ArgumentException e) {
                Logger.Warning($"watched process not running ({e.Message})");
                Notification.ShowWarning($"{processName} background process not running, will attempt to restart", TimeSpan.FromSeconds(10));
                return false;
            }
            catch (InvalidOperationException e) {
                // We've lost touch with our process - we'll try to restart but this isn't a great situation
                Logger.Error($"failed to get process for watched process: {e.Message}, pid={process.Id}!");
                Notification.ShowWarning($"Failed to find process for background {processName}, will attempt to restart");
                return false;
            }
        }
    }
}
