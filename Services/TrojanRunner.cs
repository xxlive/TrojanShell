using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrojanShell.Model;

namespace TrojanShell.Services
{
    class TrojanRunner
    {
        public static readonly string LogFile = Utils.GetTempPath($"TrojanShell_Core_{Global.PathHash}.log");

        private static readonly string _uniqueConfigFile;
        private static readonly Job _trojanShellJob;
        private Process _process;

        private CancellationTokenSource cts = null;
        static TrojanRunner()
        {
            try
            {
                _uniqueConfigFile = $"TrojanShell_{Global.PathHash}.json";
                _trojanShellJob = new Job();
            }
            catch (IOException e)
            {
                Logging.LogUsefulException(e);
            }
        }

        public static void KillAll()
        {
            var existingPrivoxy = Process.GetProcessesByName(Trojan.TROJAN_CORE_WITHOUTEXT);
            foreach (var p in existingPrivoxy.Where(IsChildProcess)) p.KillProcess();
        }

        public void Start(Configuration configuration)
        {
            cts = new CancellationTokenSource();
            if (_process != null) return;
            KillAll();
            var config = Utils.GetTempPath(_uniqueConfigFile);
            File.WriteAllText(config,Trojan.GenerateConf(configuration.GetCurrentServer(),configuration.corePort,configuration.shareOverLan ? "0.0.0.0" : "127.0.0.1"));
            _process = new Process
            {
                StartInfo =
                {
                    FileName = Trojan.TROJAN_CORE,
                    Arguments = $"-c {config}",
                    WorkingDirectory = Global.AppPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            _process.Start();
            _trojanShellJob.AddProcess(_process.Handle);
            Task.Run(() =>
            {
                using (var fs = new FileStream(LogFile, FileMode.Append))
                using (var sw = new StreamWriter(fs) { AutoFlush = true })
                using (var sr = _process.StandardError)
                {
                    while (!cts.IsCancellationRequested)
                    {
                        string textLine = sr.ReadLine();

                        if (textLine == null)
                            break;

                        sw.WriteLine(textLine);
                    }
                }
            },cts.Token);
        }

        public void Stop()
        {
            if (_process == null) return;
            cts.Cancel();
            _process.KillProcess();
            _process.Dispose();
            _process = null;
        }


        private static bool IsChildProcess(Process process)
        {
            try
            {
                var cmd = process?.GetCommandLine();
                if (cmd == null) return false;
                return cmd.Contains(_uniqueConfigFile);
            }
            catch (Exception ex)
            {
                Logging.LogUsefulException(ex);
                return false;
            }
        }
    }
}
