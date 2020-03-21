using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrojanShell.Services;
using TrojanShell.View;

namespace TrojanShell
{
    static class Program
    {
                /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Utils.ReleaseMemory(true);
            using (Mutex mutex = new Mutex(false, "Global\\TrojanShell_" + Global.PathHash))
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.ApplicationExit += Application_ApplicationExit;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var processesName = Process.GetCurrentProcess().MainModule?.ModuleName ?? "TrojanShell.exe";
                var processesNameWithoutExtension = Path.GetFileNameWithoutExtension(processesName);
                if (!mutex.WaitOne(0, false))
                {
                    Process[] oldProcesses = Process.GetProcessesByName(processesNameWithoutExtension);
                    if (oldProcesses.Length > 0)
                    {
                        Process oldProcess = oldProcesses[0];
                    }
                    MessageBox.Show(I18N.GetString("Find TrojanShell icon in your notify tray.") + "\n" +
                                    I18N.GetString("If you want to start multiple TrojanShell, make a copy in another directory."),
                        I18N.GetString("TrojanShell is already running."));
                    return;
                }

                Directory.SetCurrentDirectory(Global.AppPath);

                Logging.OpenLogFile();
                //check
                if (Trojan.CoreExsis)
                {
                    if (Trojan.Version != null)
                    {
                        Logging.Info("TrojanCore version : " + Trojan.Version);
                    }
                    else
                    {
                        throw new Exception(I18N.GetString("trojan.exe -v parse fail!"));
                    }
                }
                else
                {
                    //need do sth
                    var download = new DownloadProgress();
                    var result = download.ShowDialog();
                    if (result == DialogResult.Abort || result == DialogResult.Cancel)
                    {
                        MessageBox.Show(I18N.GetString("download fail!"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Logging.Info("TrojanCore version : " + Trojan.Version);
                }

                var controller = new TrojanShellController();
                var viewController = new MenuViewController(controller);
                Hotkeys.HotKeys.Init(controller, viewController);
                controller.StartAsync();
                Application.Run();
            }
        }

        #region HandleExceptions

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errMsg = e.ExceptionObject.ToString();
            Logging.Error(errMsg);
            MessageBox.Show(
                $"{I18N.GetString("Unexpected error, TrojanShell will exit. Please report to")} https://github.com/TkYu/TrojanShell/issues {Environment.NewLine}{errMsg}",
                "TrojanShell non-UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            string errorMsg = $"Exception Detail: {Environment.NewLine}{e.Exception}";
            Logging.Error(errorMsg);
            MessageBox.Show(
                $"{I18N.GetString("Unexpected error, TrojanShell will exit. Please report to")} https://github.com/TkYu/TrojanShell/issues {Environment.NewLine}{errorMsg}",
                "TrojanShell UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }


        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            Application.ApplicationExit -= Application_ApplicationExit;
            Application.ThreadException -= Application_ThreadException;
            try
            {
                Hotkeys.HotKeys.Destroy();
            }
            catch
            {
                //TODO
            }
        }

        #endregion
    }
}
