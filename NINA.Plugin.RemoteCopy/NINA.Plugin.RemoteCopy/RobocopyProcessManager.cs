namespace RemoteCopy.NINAPlugin {
    public sealed class RobocopyProcessManager {

        private static readonly object lockObject = new object();
        private static RemoteCopyProcessManager instance = null;

        RobocopyProcessManager() { }

        public static void Start(string executable, string arguments, bool showProcessWindow) {
            lock (lockObject) {
                if (instance != null) {
                    instance.StopProcess();
                }

                instance = new RemoteCopyProcessManager("Robocopy", executable, arguments, showProcessWindow);
                instance.StartProcessAndWatch();
            }
        }

        public static void Stop() {
            lock (lockObject) {
                if (instance != null) {
                    instance.StopProcess();
                    instance = null;
                }
            }
        }
    }
}
