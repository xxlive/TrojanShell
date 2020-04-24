using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrojanShell.Properties;
using TrojanShell.Services;
namespace TrojanShell.View
{
    public sealed partial class DownloadProgress : Form
    {
        #region WC
        private class MyWebClient : WebClient
        {
            public int Timeout { get; set; }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest lWebRequest = base.GetWebRequest(uri);
                if(lWebRequest == null)throw new Exception("cannot create WebRequest");
                lWebRequest.Timeout = Timeout;
                ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
                return lWebRequest;
            }
        }
        #endregion

        private readonly int act = 0;
        public DownloadProgress(int action)
        {
            InitializeComponent();
            Icon = Resources.horse;
            Font = Global.Font;
            act = action;
        }

        public const string TROJAN_URL = "https://github.com/trojan-gfw/trojan/releases/latest";
        public const string SHELL_URL = "https://github.com/TkYu/TrojanShell/releases/latest";

        #region InvokeMethod

        private delegate void GoodByeDelegate(DialogResult result);
        private void GoodBye(DialogResult result)
        {
            if (InvokeRequired)
            {
                Invoke(new GoodByeDelegate(GoodBye), result);
            }
            else
            {
                DialogResult = result;
                Close();
            }
        }

        private delegate void ChangeTextDelegate(string str);
        private void ChangeText(string str)
        {
            if (InvokeRequired)
                Invoke(new ChangeTextDelegate(ChangeText), str);
            else
                textBox1.Text = str;
        }

        private delegate void ChangeTitleDelegate(string str);
        private void ChangeTitle(string str)
        {
            if (InvokeRequired)
                Invoke(new ChangeTitleDelegate(ChangeTitle), str);
            else
                Text = str;
        }

        private delegate void ChangeProgressDelegate(int value);
        private void ChangeProgress(int value)
        {
            if (InvokeRequired)
            {
                Invoke(new ChangeProgressDelegate(ChangeProgress), value);
            }
            else
            {
                if (value > 100 || value<0)
                {
                    progressBar1.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    if (progressBar1.Style != ProgressBarStyle.Blocks)
                        progressBar1.Style = ProgressBarStyle.Blocks;
                    progressBar1.Value = value;
                }
            }
        }

        #endregion

        private async Task<string> GetCoreVersion(string proxy = null)
        {
            try
            {
                var request = (HttpWebRequest) WebRequest.Create(proxy == null ? "https://github.com.cnpmjs.org/trojan-gfw/trojan/releases/latest" : TROJAN_URL);
                if (proxy != null) request.Proxy = new WebProxy(new Uri(proxy));
                request.Timeout = 5000;
                request.AllowAutoRedirect = false;
                var response = await request.GetResponseAsync();
                return response.Headers["Location"].Split('/').Last().TrimStart('v');
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return null;
            }
        }

        private async Task<string> GetVersion(string proxy = null)
        {
            try
            {
                var request = (HttpWebRequest) WebRequest.Create(proxy == null ? "https://github.com.cnpmjs.org/TkYu/TrojanShell/releases/latest" : SHELL_URL);
                if (proxy != null) request.Proxy = new WebProxy(new Uri(proxy));
                request.Timeout = 5000;
                request.AllowAutoRedirect = false;
                var response = await request.GetResponseAsync();
                return response.Headers["Location"].Split('/').Last().TrimStart('v');
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return null;
            }
        }

        private async void DownloadProgress_Load(object sender, EventArgs e)
        {
            Text = I18N.GetString("Sit back and relax");
            ActiveControl = progressBar1;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//3072

            #region G2G

            if (act == 1)
            {
                if(await SelfUpdate())
                    GoodBye(DialogResult.OK);
                else
                    GoodBye(DialogResult.Abort);
            }
            else if (act == 2)
            {
                if(await DoUpdate())
                    GoodBye(DialogResult.OK);
                else
                    GoodBye(DialogResult.Abort);
            }
            else
            {
                if (!await DoUpdate())
                {
                    GoodBye(DialogResult.Abort);
                    return;
                }
                if (!await SelfUpdate())
                {
                    GoodBye(DialogResult.Abort);
                    return;
                }
                GoodBye(DialogResult.OK);
            }

            #endregion
        }

        private async Task<bool> SelfUpdate()
        {
            ChangeProgress(999);
            ChangeText(I18N.GetString("Get latest version..."));
            var newVersion = await GetVersion();
            string proxy = null;
            if (string.IsNullOrEmpty(newVersion))
            {
                proxy = Microsoft.VisualBasic.Interaction.InputBox(I18N.GetString("We need a proxy to download trojan core"), "Yo", "http://127.0.0.1:1080");
                if (Uri.IsWellFormedUriString(proxy,UriKind.Absolute)) newVersion = await GetVersion(proxy);
            }
            if (string.IsNullOrEmpty(newVersion))
            {
                System.Diagnostics.Process.Start(SHELL_URL);
                return false;
            }
            ChangeText(I18N.GetString("Upgrade {0} to {1} ...",Global.Version,newVersion));
            var webClient = new MyWebClient { Timeout = 20000 };
            if(!string.IsNullOrEmpty(proxy) && Uri.IsWellFormedUriString(proxy,UriKind.Absolute))
                webClient.Proxy = new WebProxy(new Uri(proxy));
            webClient.DownloadProgressChanged += (s, e) =>
            {
                ChangeProgress(e.ProgressPercentage);
            };
            var fileName = Utils.GetTempPath(Guid.NewGuid().ToString("N"));
            var downloadURL = $"https://github.wuyanzheshui.workers.dev/TkYu/TrojanShell/releases/download/v{newVersion}/TrojanShell{newVersion}.zip";
            ChangeTitle(I18N.GetString("Sit back and relax") + " " + I18N.GetString("Upgrade {0} to {1} ...", Global.Version, newVersion));
            ChangeText(I18N.GetString("Downloading file from {0}, You can download it manually and extract to same folder.", downloadURL));
            try
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.92 Safari/537.36");
                await webClient.DownloadFileTaskAsync(downloadURL, fileName);
            }
            catch
            {
                if (File.Exists(fileName)) File.Delete(fileName);
            }
            if (!File.Exists(fileName))
            {
                downloadURL = $"https://release.fastgit.org/TkYu/TrojanShell/releases/download/v{newVersion}/TrojanShell{newVersion}.zip";
                ChangeText(I18N.GetString("Downloading file from {0}, You can download it manually and extract to same folder.", downloadURL));
                try
                {
                    await webClient.DownloadFileTaskAsync(downloadURL, fileName);
                }
                catch
                {
                    if (File.Exists(fileName)) File.Delete(fileName);
                }
            }
            if (!File.Exists(fileName))
            {
                downloadURL = $"https://github.com/TkYu/TrojanShell/releases/download/v{newVersion}/TrojanShell{newVersion}.zip";
                ChangeText(I18N.GetString("Downloading file from {0}, You can download it manually and extract to same folder.", downloadURL));
                try
                {
                    await webClient.DownloadFileTaskAsync(downloadURL, fileName);
                }
                catch
                {
                    if (File.Exists(fileName)) File.Delete(fileName);
                }
            }
            if (!File.Exists(fileName))
            {
                return false;
            }
            ChangeProgress(100);
            ChangeText(newVersion + I18N.GetString("Extracting..."));
            var tempfile = Utils.GetTempPath(Guid.NewGuid().ToString("N"));
            try
            {
                using (var archive = ZipFile.OpenRead(fileName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.ToLower() == "trojanshell.exe")
                        {
                            entry.ExtractToFile(tempfile, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                if (File.Exists(tempfile)) File.Delete(tempfile);
                return false;
            }
            finally
            {
                File.Delete(fileName);
            }
            if (File.Exists(tempfile))
            {
                Program.ViewControllerInstance.HideTray();
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    Arguments = $"/C @echo off & Title Updating... & echo {I18N.GetString("Sit back and relax")} & taskkill /f /im {System.Diagnostics.Process.GetCurrentProcess().ProcessName}.exe & choice /C Y /N /D Y /T 1 & Del \"{Global.ProcessPath}\" & Move /y \"{tempfile}\" \"{Global.ProcessPath}\" & Start \"\" \"{Global.ProcessPath}\"",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                };
                System.Diagnostics.Process.Start(psi);
                Environment.Exit(0);
            }
            return true;
        }

        private async Task<bool> DoUpdate()
        {
            ChangeProgress(999);
            ChangeText(I18N.GetString("Get latest version..."));
            var newVersion = await GetCoreVersion();
            string proxy = null;
            if (string.IsNullOrEmpty(newVersion))
            {
                proxy = Microsoft.VisualBasic.Interaction.InputBox(I18N.GetString("We need a proxy to download trojan core"), "Yo", "http://127.0.0.1:1080");
                if (Uri.IsWellFormedUriString(proxy,UriKind.Absolute)) newVersion = await GetCoreVersion(proxy);
            }

            if (string.IsNullOrEmpty(newVersion))
            {
                System.Diagnostics.Process.Start(TROJAN_URL);
                return false;
            }

            ChangeText(I18N.GetString("Upgrade {0} to {1} ...",Trojan.Version?.ToString()??"0.0.0",newVersion));
            var webClient = new MyWebClient { Timeout = 20000 };
            if(!string.IsNullOrEmpty(proxy) && Uri.IsWellFormedUriString(proxy,UriKind.Absolute))
                webClient.Proxy = new WebProxy(new Uri(proxy));
            webClient.DownloadProgressChanged += (s, e) =>
            {
                ChangeProgress(e.ProgressPercentage);
                //ChangeText(newVersion + I18N.GetString("Downloading...") + $" {e.ProgressPercentage}%");
            };
            var fileName = Utils.GetTempPath(Guid.NewGuid().ToString("N"));
            var downloadURL = $"https://github.wuyanzheshui.workers.dev/trojan-gfw/trojan/releases/download/v{newVersion}/trojan-{newVersion}-win.zip";
            ChangeTitle(I18N.GetString("Sit back and relax") + " " + I18N.GetString("Upgrade {0} to {1} ...", Trojan.Version?.ToString() ?? "0.0.0", newVersion));
            ChangeText(I18N.GetString("Downloading file from {0}, You can download it manually and extract to same folder.", downloadURL));
            try
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.92 Safari/537.36");
                await webClient.DownloadFileTaskAsync(downloadURL, fileName);
            }
            catch
            {
                if (File.Exists(fileName)) File.Delete(fileName);
            }
            if (!File.Exists(fileName))
            {
                downloadURL = $"https://release.fastgit.org/trojan-gfw/trojan/releases/download/v{newVersion}/trojan-{newVersion}-win.zip";
                ChangeText(I18N.GetString("Downloading file from {0}, You can download it manually and extract to same folder.", downloadURL));
                try
                {
                    await webClient.DownloadFileTaskAsync(downloadURL, fileName);
                }
                catch
                {
                    if (File.Exists(fileName)) File.Delete(fileName);
                }
            }
            if (!File.Exists(fileName))
            {
                downloadURL = $"https://github.com/trojan-gfw/trojan/releases/download/v{newVersion}/trojan-{newVersion}-win.zip";
                ChangeText(I18N.GetString("Downloading file from {0}, You can download it manually and extract to same folder.", downloadURL));
                try
                {
                    await webClient.DownloadFileTaskAsync(downloadURL, fileName);
                }
                catch
                {
                    if (File.Exists(fileName)) File.Delete(fileName);
                }
            }
            if (!File.Exists(fileName))
            {
                return false;
            }
            ChangeProgress(100);
            ChangeText(newVersion + I18N.GetString("Extracting..."));
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(fileName))
                {
                    TrojanRunner.KillAll();
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.ToLower() == "trojan/trojan.exe")
                        {
                            entry.ExtractToFile(Path.Combine(Global.AppPath, Trojan.TROJAN_CORE), true);
                        }
                        else
                        {
                            //ignore
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return false;
            }
            finally
            {
                File.Delete(fileName);
            }
            return true;
        }
    }



}
