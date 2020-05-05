﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TrojanShell.Model;
using TrojanShell.Properties;
using TrojanShell.View;

namespace TrojanShell.Services
{
    public class MenuViewController : ApplicationContext
    {
        private bool _isFirstRun;

        private readonly TrojanShellController controller;
        private readonly NotifyIcon _notifyIcon;
        private ContextMenu contextMenu1;
        public MenuItem enableItem;
        private MenuItem modeItem;
        private MenuItem AutoStartupItem;
        public MenuItem ShareOverLANItem;
        private MenuItem SeperatorItem;
        private MenuItem ConfigItem;
        private MenuItem ServersItem;
        public MenuItem globalModeItem;
        public MenuItem PACModeItem;
        //private MenuItem localPACItem;
        private MenuItem editLocalPACItem;
        private MenuItem updateFromGFWListItem;
        private MenuItem editGFWUserRuleItem;
        private MenuItem autoCheckUpdatesToggleItem;
        private MenuItem verboseLoggingToggleItem;
        private MenuItem subscribeUpdateItem;
        public MenuItem ScanQR;
        public MenuItem ShowLog;
        public MenuItem ShowCoreLog;
        public MenuItem ShareQR;
        public MenuItem ImportFromClipboard;

        private ConfigForm configForm;
        private LogForm logForm;
        private HotkeySettingsForm hotkeySettingsForm;
        private SubscribeConfigForm subscribeConfigForm;

        private string _urlToOpen;
        private readonly UpdateChecker updateChecker;

        private Bitmap iconBase;
        private Icon trayIcon;

        public MenuViewController(TrojanShellController controller)
        {
            this.controller = controller;

            LoadMenu();

            controller.EnableStatusChanged += controller_EnableStatusChanged;
            controller.ConfigChanged += controller_ConfigChanged;
            controller.PACFileReadyToOpen += controller_FileReadyToOpen;
            controller.UserRuleFileReadyToOpen += controller_FileReadyToOpen;
            controller.ShareOverLANStatusChanged += controller_ShareOverLANStatusChanged;
            controller.VerboseLoggingStatusChanged += controller_VerboseLoggingStatusChanged;
            controller.EnableGlobalChanged += controller_EnableGlobalChanged;
            controller.Errored += controller_Errored;
            controller.UpdatePACFromGFWListCompleted += controller_UpdatePACFromGFWListCompleted;
            controller.UpdatePACFromGFWListError += controller_UpdatePACFromGFWListError;

            _notifyIcon = new NotifyIcon();
            UpdateTrayIcon();
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenu = contextMenu1;
            _notifyIcon.BalloonTipClicked += notifyIcon1_BalloonTipClicked;
            _notifyIcon.MouseClick += notifyIcon1_Click;
            _notifyIcon.MouseDoubleClick += notifyIcon1_DoubleClick;

            updateChecker = new UpdateChecker();

            LoadCurrentConfiguration();

            Configuration config = controller.GetConfigurationCopy();

            if (config.isDefault)
            {
                _isFirstRun = true;
                ShowConfigForm();
            }
            else if(config.autoCheckUpdate)
            {
                CheckUpdate(config);
            }
        }

        public void HideTray()
        {
            this._notifyIcon.Visible = false;
        }

        private void UpdateTrayIcon()
        {
            iconBase = null;
            if (Global.DPI < 96)
            {
                //72
                iconBase = Resources.T18;
            }
            else if (Global.DPI < 192)
            {
                //96
                iconBase = Resources.T24;
            }
            else
            {
                //192
                iconBase = Resources.T48;
            }
            var config = controller.GetConfigurationCopy();
            var enabled = config.enabled;
            var global = config.global;
            iconBase = getTrayIconByState(iconBase, enabled, global);
            trayIcon = Icon.FromHandle(iconBase.GetHicon());
            _notifyIcon.Icon = trayIcon;

            var serverInfo = config.GetCurrentServer().FriendlyName();
            var text = I18N.GetString("TrojanShell") + " " + Global.Version + "\n" +
                       (enabled ?
                           I18N.GetString("System Proxy On: ") + (global ? I18N.GetString("Global") : I18N.GetString("PAC")) :
                           string.Format(I18N.GetString("Running: Port {0}"), $"{config.corePort}/{config.localPort}"))
                       + "\n" + serverInfo;
            _notifyIcon.SetNotifyIconText(text);
        }

        private Bitmap getTrayIconByState(Bitmap originIcon, bool enabled, bool global)
        {
            Bitmap iconCopy = new Bitmap(originIcon);
            for (int x = 0; x < iconCopy.Width; x++)
            {
                for (int y = 0; y < iconCopy.Height; y++)
                {
                    Color color = originIcon.GetPixel(x, y);
                    if (color.A != 0)
                    {
                        if (!enabled)
                        {
                            Color flyBlue = Color.FromArgb(146, 146, 146);
                            int red = color.R * flyBlue.R / 255;
                            int green = color.G * flyBlue.G / 255;
                            int blue = color.B * flyBlue.B / 255;
                            iconCopy.SetPixel(x, y, Color.FromArgb(color.A, red, green, blue));
                        }
                        else if (global)
                        {
                            Color flyBlue = Color.FromArgb(84, 125, 248);
                            int red   = color.R * flyBlue.R / 255;
                            int green = color.G * flyBlue.G / 255;
                            int blue  = color.B * flyBlue.B / 255;
                            iconCopy.SetPixel(x, y, Color.FromArgb(color.A, red, green, blue));
                        }
                    }
                    else
                    {
                        iconCopy.SetPixel(x, y, Color.FromArgb(color.A, color.R, color.G, color.B));
                    }
                }
            }
            return iconCopy;
        }

        private async void CheckUpdate(Configuration config,bool noFoundNotify = false)
        {
            var result = await updateChecker.CheckUpdate(config);
            if (result.Item1 && result.Item2)
            {
                updateflag = 3;
                ShowBalloonTip(I18N.GetString("TrojanShell and TrojanCore Update Found :") + $"{result.Item3}/{result.Item4}", I18N.GetString("Click here to update"), ToolTipIcon.Info, 5000);
                return;
            }
            if (result.Item1)
            {
                updateflag = 1;
                ShowBalloonTip(I18N.GetString("TrojanShell Update Found :") + result.Item3, I18N.GetString("Click here to update"), ToolTipIcon.Info, 5000);
                return;
            }
            if (result.Item2)
            {
                updateflag = 2;
                ShowBalloonTip(I18N.GetString("TrojanCore Update Found :") + result.Item4, I18N.GetString("Click here to update"), ToolTipIcon.Info, 5000);
                return;
            }
            if(noFoundNotify)
                ShowBalloonTip(I18N.GetString("TrojanShell"), I18N.GetString("No update is available"), ToolTipIcon.Info, 5000);
        }

        public void ShowBalloonTip(string title, string content, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 1000)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = content;
            _notifyIcon.BalloonTipIcon = icon;
            _notifyIcon.ShowBalloonTip(timeout);
        }

        #region MenuItems and MenuGroups

        private MenuItem CreateMenuItem(string text, EventHandler click)
        {

            return new MenuItem(I18N.GetString(text), click);
        }

        private MenuItem CreateMenuGroup(string text, MenuItem[] items)
        {
            return new MenuItem(I18N.GetString(text), items);
        }


        private void LoadMenu()
        {
            contextMenu1 = new ContextMenu(new[] {
                enableItem = CreateMenuItem("Enable System Proxy", EnableItem_Click),
                modeItem = CreateMenuGroup("Mode", new[] {
                    PACModeItem = CreateMenuItem("PAC", PACModeItem_Click),
                    globalModeItem = CreateMenuItem("Global", GlobalModeItem_Click)
                }),
                ServersItem = CreateMenuGroup("Servers", new[] {
                    SeperatorItem = new MenuItem("-"),
                    ConfigItem = CreateMenuItem("Edit Servers...", Config_Click),
                    new MenuItem("-"),
                    ShareQR = CreateMenuItem("Share Server Config...", QRCodeItem_Click),
                    ScanQR = CreateMenuItem("Scan QRCode from Screen...", ScanQRCodeItem_Click),
                    ImportFromClipboard = CreateMenuItem("Import URL from Clipboard...", ImportURLItem_Click)
                }),
                CreateMenuGroup("PAC ", new MenuItem[] {
                    //localPACItem = CreateMenuItem("Local PAC", LocalPACItem_Click),
                    editLocalPACItem = CreateMenuItem("Edit Local PAC File...", EditPACFileItem_Click),
                    updateFromGFWListItem = CreateMenuItem("Update Local PAC from GFWList", UpdatePACFromGFWListItem_Click),
                    editGFWUserRuleItem = CreateMenuItem("Edit User Rule for GFWList...", EditUserRuleFileForGFWListItem_Click),
                    CreateMenuItem("Copy Local PAC URL", CopyLocalPacUrlItem_Click),
                }),
                new MenuItem("-"),
                AutoStartupItem = CreateMenuItem("Start on Boot", AutoStartupItem_Click),
                ShareOverLANItem = CreateMenuItem("Allow Clients from LAN", ShareOverLANItem_Click),
                new MenuItem("-"),
                CreateMenuItem("Subscriptions...", subscribeItem_Click),
                subscribeUpdateItem = CreateMenuItem("Update Subscriptions", subscribeUpdateItem_Click),
                new MenuItem("-"),
                CreateMenuItem("Edit Hotkeys...", hotKeyItem_Click),
                CreateMenuGroup("Help", new[] {
                    ShowLog = CreateMenuItem("Show Logs...", ShowLogItem_Click),
                    verboseLoggingToggleItem = CreateMenuItem( "Verbose Logging", VerboseLoggingToggleItem_Click ),
                    new MenuItem("-"),
                    ShowCoreLog = CreateMenuItem("Show Core Logs...", ShowCoreLogItem_Click),
                    new MenuItem("-"),
                    CreateMenuGroup("Updates...", new[] {
                        CreateMenuItem("Check for Updates...", checkUpdatesItem_Click),
                        new MenuItem("-"),
                        autoCheckUpdatesToggleItem = CreateMenuItem("Check for Updates at Startup", autoCheckUpdatesToggleItem_Click),
                    }),
                    CreateMenuItem("About...", AboutItem_Click),
                }),
                new MenuItem("-"),
                CreateMenuItem("Quit", Quit_Click)
            });
            //localPACItem.Visible = false;
        }
        #endregion

        #region ControllerEvents
        void controller_UpdatePACFromGFWListError(object sender, System.IO.ErrorEventArgs e)
        {
            ShowBalloonTip(I18N.GetString("Failed to update PAC file"), e.GetException().Message, ToolTipIcon.Error, 5000);
            Logging.LogUsefulException(e.GetException());
        }

        void controller_UpdatePACFromGFWListCompleted(object sender, GFWListUpdater.ResultEventArgs e)
        {
            string result = e.Success
                ? I18N.GetString("PAC updated")
                : I18N.GetString("No updates found. Please report to GFWList if you have problems with it.");
            ShowBalloonTip(I18N.GetString("TrojanShell"), result);
        }

        void controller_EnableGlobalChanged(object sender, EventArgs e)
        {
            globalModeItem.Checked = controller.GetConfigurationCopy().global;
            PACModeItem.Checked = !globalModeItem.Checked;
        }

        void controller_Errored(object sender, System.IO.ErrorEventArgs e)
        {
            MessageBox.Show(e.GetException().ToString(), I18N.GetString("TrojanShell Error:") + e.GetException().Message);
        }

        private void controller_EnableStatusChanged(object sender, EventArgs e)
        {
            enableItem.Checked = controller.GetConfigurationCopy().enabled;
            modeItem.Enabled = enableItem.Checked;
        }
        private void controller_ConfigChanged(object sender, EventArgs e)
        {
            LoadCurrentConfiguration();
            UpdateTrayIcon();
        }

        void controller_FileReadyToOpen(object sender, TrojanShellController.PathEventArgs e)
        {
            string argument = @"/select, " + e.Path;
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        void controller_ShareOverLANStatusChanged(object sender, EventArgs e)
        {
            ShareOverLANItem.Checked = controller.GetConfigurationCopy().shareOverLan;
        }

        void controller_VerboseLoggingStatusChanged(object sender, EventArgs e) {
            verboseLoggingToggleItem.Checked = controller.GetConfigurationCopy().isVerboseLogging;
        }

        #endregion

        #region MenuEvents

        private int updateflag;
        void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if (updateflag == 1)
            {
                updateflag = 0; /* Reset the flag */
                //System.Diagnostics.Process.Start(UpdateChecker.SHELL_URL);
                var download = new DownloadProgress(1);
                var dg = download.ShowDialog();
                if (dg == DialogResult.Abort || dg == DialogResult.Cancel) MessageBox.Show(I18N.GetString("download fail!"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (updateflag == 2)
            {
                updateflag = 0;
                var download = new DownloadProgress(2);
                var dg = download.ShowDialog();
                if (dg == DialogResult.Abort || dg == DialogResult.Cancel) MessageBox.Show(I18N.GetString("download fail!"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                controller.RestartCore();
            }
            else if (updateflag == 3)
            {
                updateflag = 0;
                var download = new DownloadProgress(3);
                var dg = download.ShowDialog();
                if (dg == DialogResult.Abort || dg == DialogResult.Cancel) MessageBox.Show(I18N.GetString("download fail!"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                controller.RestartCore();
            }
        }

        private void UpdateUpdateMenu()
        {
            Configuration configuration = controller.GetConfigurationCopy();
            autoCheckUpdatesToggleItem.Checked = configuration.autoCheckUpdate;
        }

        private async void autoCheckUpdatesToggleItem_Click(object sender, EventArgs e)
        {
            Configuration configuration = controller.GetConfigurationCopy();
            await controller.ToggleCheckingUpdate(!configuration.autoCheckUpdate);
            UpdateUpdateMenu();
        }


        private void checkUpdatesItem_Click(object sender, EventArgs e)
        {
            CheckUpdate(controller.GetConfigurationCopy(), true);
        }

        private void notifyIcon1_Click(object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Middle )
            {
                ShowLogForm(Logging.LogFilePath);
            }
        }

        private void notifyIcon1_DoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowConfigForm();
            }
        }

        private void AboutItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/TkYu/TrojanShell");
        }

        private async void Quit_Click(object sender, EventArgs e)
        {
            await controller.StopAsync();
            _notifyIcon.Visible = false;
            Application.Exit();
        }

        private async void EnableItem_Click(object sender, EventArgs e)
        {
            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await controller.ToggleEnable(!enableItem.Checked);
        }

        private async void GlobalModeItem_Click(object sender, EventArgs e)
        {
            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await controller.ToggleGlobal(true);
        }

        private async void PACModeItem_Click(object sender, EventArgs e)
        {
            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await controller.ToggleGlobal(false);
        }

        private async void ShareOverLANItem_Click(object sender, EventArgs e)
        {
            ShareOverLANItem.Checked = !ShareOverLANItem.Checked;
            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await controller.ToggleShareOverLAN(ShareOverLANItem.Checked);
        }

        private void hotKeyItem_Click(object sender, EventArgs e)
        {
            ShowHotKeySettingsForm();
        }

        private void subscribeItem_Click(object sender, EventArgs e)
        {
            ShowSubscribeForm();
        }

        private async void subscribeUpdateItem_Click(object sender, EventArgs e)
        {
            var config = controller.GetConfigurationCopy();
            if (config.subscribes == null || !config.subscribes.Any()) return;
            subscribeUpdateItem.Enabled = false;
            var before = config.configs.Count(c => !string.IsNullOrEmpty(c.@group));
            var currentSvc = controller.GetCurrentServer();
            foreach (var item in config.subscribes)
            {
                var wc = new System.Net.WebClient();
                if (item.useProxy) wc.Proxy = new System.Net.WebProxy(System.Net.IPAddress.Loopback.ToString(), config.localPort);
                var cts = new System.Threading.CancellationTokenSource();
                // ReSharper disable once AccessToDisposedClosure
                // ReSharper disable once ConvertToLambdaExpressionWhenPossible
                cts.Token.Register(() => { wc?.CancelAsync();});
                cts.CancelAfter(5000);
                var downloadString = await wc.DownloadStringTaskAsync(item.url);
                wc.Dispose();
                var debase64 = downloadString.IndexOf("trojan://", StringComparison.OrdinalIgnoreCase) > -1 ? downloadString : downloadString.DeBase64();
                var split = debase64.Split('\r', '\n');
                var lst = new System.Collections.Generic.List<Server>();
                foreach (var s in split)
                {
                    if (!s.StartsWith("trojan://", StringComparison.OrdinalIgnoreCase)) continue;
                    if (Server.TryParse(s, out Server svc))
                    {
                        svc.@group = item.name;
                        lst.Add(svc);
                    }
                }
                if (lst.Any())
                {
                    config.configs.RemoveAll(c => c.@group == item.name);
                    config.configs.AddRange(lst);
                }
            }
            controller.SaveServers(config.configs, config.localPort,config.corePort);
            var newIdx = config.configs.IndexOf(currentSvc);
            if (newIdx == -1)
                await controller.SelectServerIndex(0);
            else if (newIdx != config.index)
                await controller.SelectServerIndex(newIdx);
            var after = config.configs.Count(c => !string.IsNullOrEmpty(c.@group));
            ShowBalloonTip(I18N.GetString("Update finished"), I18N.GetString("{0} before, {1} after.", before, after));
            subscribeUpdateItem.Enabled = true;
        }

        private void EditPACFileItem_Click(object sender, EventArgs e)
        {
            controller.TouchPACFile();
        }

        private void UpdatePACFromGFWListItem_Click(object sender, EventArgs e)
        {
            controller.UpdatePACFromGFWList();
        }

        private void EditUserRuleFileForGFWListItem_Click(object sender, EventArgs e)
        {
            controller.TouchUserRuleFile();
        }

        private void AServerItem_Click(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            controller.SelectServerIndex((int)item.Tag);
        }

        private void ShowLogItem_Click(object sender, EventArgs e)
        {
            ShowLogForm(Logging.LogFilePath);
        }

        private void ShowCoreLogItem_Click(object sender, EventArgs e)
        {
            ShowLogForm(TrojanRunner.LogFile);
        }

        private async void VerboseLoggingToggleItem_Click( object sender, EventArgs e ) {
            verboseLoggingToggleItem.Checked = ! verboseLoggingToggleItem.Checked;
            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await controller.ToggleVerboseLogging(verboseLoggingToggleItem.Checked);
        }

        private void Config_Click(object sender, EventArgs e)
        {
            ShowConfigForm();
        }

        private void QRCodeItem_Click(object sender, EventArgs e)
        {
            var qrCodeForm = new QRCodeForm(controller.GetCurrentServer().ToURL());
            qrCodeForm.Show();
        }

        private async void ImportURLItem_Click(object sender, EventArgs e)
        {
            var success = await controller.AddServerByURL(Clipboard.GetText(TextDataFormat.Text));
            if (success)
            {
                ShowConfigForm();
            }
        }

        //private void LocalPACItem_Click(object sender, EventArgs e)
        //{
        //    // if (!localPACItem.Checked)
        //    // {
        //    //     localPACItem.Checked = true;
        //    //     UpdatePACItemsEnabledStatus();
        //    // }
        //}

        void splash_FormClosed(object sender, FormClosedEventArgs e)
        {
            ShowConfigForm();
        }

        void openURLFromQRCode(object sender, FormClosedEventArgs e)
        {
            if (MessageBox.Show($"{I18N.GetString("Current QR content is")}  \"{_urlToOpen}\"  {I18N.GetString("should i open it?")}", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var staThread = new System.Threading.Thread(x =>
                {
                    try
                    {
                        System.Diagnostics.Process.Start(_urlToOpen);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogUsefulException(ex);
                    }
                });
                staThread.SetApartmentState(System.Threading.ApartmentState.STA);
                staThread.Start();
            }
        }

        void copyFromQRCode(object sender, FormClosedEventArgs e)
        {
            if (MessageBox.Show($"{I18N.GetString("Current QR content is")}  \"{_urlToOpen}\"  {I18N.GetString("should i copy that to clipboard?")}", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var staThread = new System.Threading.Thread(x =>
                {
                    try
                    {
                        Clipboard.SetText(_urlToOpen);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogUsefulException(ex);
                    }
                });
                staThread.SetApartmentState(System.Threading.ApartmentState.STA);
                staThread.Start();
            }
        }

        private void CopyLocalPacUrlItem_Click(object sender, EventArgs e)
        {
            controller.CopyPacUrl();
        }

        private void AutoStartupItem_Click(object sender, EventArgs e)
        {
            AutoStartupItem.Checked = !AutoStartupItem.Checked;
            if (!AutoStartup.Set(AutoStartupItem.Checked))
            {
                MessageBox.Show(I18N.GetString("Failed to update registry"));
            }
        }


        private async void ScanQRCodeItem_Click(object sender, EventArgs e)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                using (var fullImage = new System.Drawing.Bitmap(screen.Bounds.Width,
                                                screen.Bounds.Height))
                {
                    using (var g = System.Drawing.Graphics.FromImage(fullImage))
                    {
                        g.CopyFromScreen(screen.Bounds.X,
                                         screen.Bounds.Y,
                                         0, 0,
                                         fullImage.Size,
                                         System.Drawing.CopyPixelOperation.SourceCopy);
                    }
                    int maxTry = 10;
                    for (int i = 0; i < maxTry; i++)
                    {
                        int marginLeft = (int)((double)fullImage.Width * i / 2.5 / maxTry);
                        int marginTop = (int)((double)fullImage.Height * i / 2.5 / maxTry);
                        var cropRect = new System.Drawing.Rectangle(marginLeft, marginTop, fullImage.Width - marginLeft * 2, fullImage.Height - marginTop * 2);
                        var target = new System.Drawing.Bitmap(screen.Bounds.Width, screen.Bounds.Height);

                        double imageScale = (double)screen.Bounds.Width / (double)cropRect.Width;
                        using (var g = System.Drawing.Graphics.FromImage(target))
                        {
                            g.DrawImage(fullImage, new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                                            cropRect,
                                            System.Drawing.GraphicsUnit.Pixel);
                        }
                        var source = new ZXing.BitmapLuminanceSource(target);
                        var bitmap = new ZXing.BinaryBitmap(new ZXing.Common.HybridBinarizer(source));
                        var reader = new ZXing.QrCode.QRCodeReader();
                        var result = reader.decode(bitmap);
                        if (result != null)
                        {
                            var splash = new QRCodeSplashForm();
                            if (result.Text.StartsWith("trojan://",StringComparison.OrdinalIgnoreCase))
                            {
                                var success = await controller.AddServerByURL(result.Text);
                                if (success)
                                    splash.FormClosed += splash_FormClosed;
                            }
                            else if (result.Text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || result.Text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                _urlToOpen = result.Text;
                                splash.FormClosed += openURLFromQRCode;
                            }
                            else
                            {
                                _urlToOpen = result.Text;
                                splash.FormClosed += copyFromQRCode;
                            }
                            double minX = Int32.MaxValue, minY = Int32.MaxValue, maxX = 0, maxY = 0;
                            foreach (ZXing.ResultPoint point in result.ResultPoints)
                            {
                                minX = Math.Min(minX, point.X);
                                minY = Math.Min(minY, point.Y);
                                maxX = Math.Max(maxX, point.X);
                                maxY = Math.Max(maxY, point.Y);
                            }
                            minX /= imageScale;
                            minY /= imageScale;
                            maxX /= imageScale;
                            maxY /= imageScale;
                            double margin = (maxX - minX) * 0.20f;
                            minX += -margin + marginLeft;
                            maxX += margin + marginLeft;
                            minY += -margin + marginTop;
                            maxY += margin + marginTop;
                            splash.Location = new System.Drawing.Point(screen.Bounds.X, screen.Bounds.Y);
                            splash.TargetRect = new System.Drawing.Rectangle((int)minX + screen.Bounds.X, (int)minY + screen.Bounds.Y, (int)maxX - (int)minX, (int)maxY - (int)minY);
                            splash.Size = new System.Drawing.Size(fullImage.Width, fullImage.Height);
                            splash.Show();
                            return;
                        }
                    }
                }
            }
            MessageBox.Show(I18N.GetString("No QRCode found. Try to zoom in or move it to the center of the screen."));
        }

        //private void UpdatePACItemsEnabledStatus()
        //{
        //    if (localPACItem.Checked)
        //    {
        //        editLocalPACItem.Enabled = true;
        //        updateFromGFWListItem.Enabled = true;
        //        editGFWUserRuleItem.Enabled = true;
        //    }
        //    else
        //    {
        //        editLocalPACItem.Enabled = false;
        //        updateFromGFWListItem.Enabled = false;
        //        editGFWUserRuleItem.Enabled = false;
        //    }
        //}

        private void LoadCurrentConfiguration()
        {
            Configuration config = controller.GetConfigurationCopy();
            UpdateServersMenu();
            enableItem.Checked = config.enabled;
            modeItem.Enabled = config.enabled;
            globalModeItem.Checked = config.global;
            PACModeItem.Checked = !config.global;
            ShareOverLANItem.Checked = config.shareOverLan;
            verboseLoggingToggleItem.Checked = config.isVerboseLogging;
            AutoStartupItem.Checked = AutoStartup.Check();
            //localPACItem.Checked = true;
            //UpdatePACItemsEnabledStatus();
            UpdateUpdateMenu();
        }

        private void UpdateServersMenu()
        {
            var items = ServersItem.MenuItems;
            while (items[0] != SeperatorItem)
            {
                items.RemoveAt(0);
            }
            int i = 0;

            Configuration configuration = controller.GetConfigurationCopy();
            var grp = configuration.configs.GroupBy(c => c.@group).ToArray();
            var defaultGroup = grp.Single(c => string.IsNullOrEmpty(c.Key)).ToArray();
            var otherGroup = grp.Where(c => !string.IsNullOrEmpty(c.Key)).ToArray();
            foreach (var server in defaultGroup)
            {
                MenuItem item = new MenuItem(server.FriendlyName());
                item.Tag = i;
                item.Click += AServerItem_Click;
                items.Add(i, item);
                i++;
            }

            if (otherGroup.Any())
            {
                items.Add( i++, new MenuItem("-") );
                int defaultSvcCount = i;
                foreach (var group in otherGroup)
                {
                    var grpItem = new MenuItem(group.Key);
                    items.Add( defaultSvcCount++,  grpItem);
                    foreach (var server in group)
                    {
                        MenuItem item = new MenuItem(server.FriendlyName());
                        item.Tag = i - 1;//remove seperator
                        item.Click += AServerItem_Click;
                        grpItem.MenuItems.Add(item);
                        i++;
                    }
                }
            }

            foreach (MenuItem item in items)
            {
                if (item.MenuItems.Count > 0)
                {
                    foreach (MenuItem itemMenuItem in item.MenuItems)
                    {
                        if (itemMenuItem.Tag != null && itemMenuItem.Tag.ToString() == configuration.index.ToString())
                        {
                            itemMenuItem.Checked = true;
                        }
                    }
                }
                else
                {
                    if (item.Tag != null && item.Tag.ToString() == configuration.index.ToString())
                    {
                        item.Checked = true;
                    }
                }
            }
        }

        private void CheckUpdateForFirstRun()
        {
            Configuration config = controller.GetConfigurationCopy();
            if (config.isDefault) return;
            //TODO why
            CheckUpdate(config);
        }

        private void ShowFirstTimeBalloon()
        {
            _notifyIcon.BalloonTipTitle = I18N.GetString("TrojanShell is here");
            _notifyIcon.BalloonTipText = I18N.GetString("You can turn on/off TrojanShell in the context menu");
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(0);
        }

        private void ShowLogForm(string logPath)
        {
            if (logForm != null)
            {
                logForm.Activate();
            }
            else
            {
                logForm = new LogForm(logPath);
                logForm.Show();
                logForm.Activate();
                logForm.FormClosed += (s, e) =>
                {
                    logForm.Dispose();
                    logForm = null;
                    Utils.ReleaseMemory(true);
                };
            }
        }

        private void ShowConfigForm()
        {
            if (configForm != null)
            {
                configForm.Activate();
            }
            else
            {
                configForm = new ConfigForm(controller);
                configForm.Show();
                configForm.Activate();
                configForm.FormClosed += (s, e) =>
                {
                    configForm.Dispose();
                    configForm = null;
                    RebuildMenu();
                    Utils.ReleaseMemory(true);
                    if (_isFirstRun)
                    {
                        CheckUpdateForFirstRun();
                        ShowFirstTimeBalloon();
                        _isFirstRun = false;
                    }
                };
            }
        }

        private void ShowHotKeySettingsForm()
        {
            if (hotkeySettingsForm != null)
            {
                hotkeySettingsForm.Activate();
            }
            else
            {
                hotkeySettingsForm = new HotkeySettingsForm(controller);
                hotkeySettingsForm.Show();
                hotkeySettingsForm.Activate();
                hotkeySettingsForm.FormClosed += (s, e) =>
                {
                    hotkeySettingsForm.Dispose();
                    hotkeySettingsForm = null;
                    Utils.ReleaseMemory(true);
                };
            }
        }

        private void ShowSubscribeForm()
        {
            if (subscribeConfigForm != null)
            {
                subscribeConfigForm.Activate();
            }
            else
            {
                subscribeConfigForm = new SubscribeConfigForm(controller);
                subscribeConfigForm.Show();
                subscribeConfigForm.Activate();
                subscribeConfigForm.FormClosed += (s, e) =>
                {
                    subscribeConfigForm.Dispose();
                    subscribeConfigForm = null;
                    //rebuild menu
                    RebuildMenu();
                    Utils.ReleaseMemory(true);
                };
            }
        }

        private void RebuildMenu()
        {
            ServersItem.MenuItems.Clear();
            ServersItem.MenuItems.AddRange(new[]
            {
                SeperatorItem,
                ConfigItem,
                new MenuItem("-"),
                ShareQR,
                ScanQR,
                ImportFromClipboard
            });
            UpdateServersMenu();
        }
        #endregion
    }
}
