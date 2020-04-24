using System;
using System.Windows.Forms;
using TrojanShell.Model;
using TrojanShell.Properties;
using TrojanShell.Services;

namespace TrojanShell.View
{
    public partial class ConfigForm : Form
    {
        private TrojanShellController controller;

        // this is a copy of configuration that we are working on
        private Configuration _modifiedConfiguration;
        private int _lastSelectedIndex = -1;

        public ConfigForm(TrojanShellController controller)
        {
            InitializeComponent();
            PerformLayout();
            UpdateTexts();
            Icon = Resources.horse;
            this.controller = controller;
            controller.ConfigChanged += Controller_ConfigChanged;
            LoadCurrentConfiguration();
        }

        private void Controller_ConfigChanged(object sender, EventArgs e)
        {
            LoadCurrentConfiguration();
        }

        private void UpdateTexts()
        {
            Font = Global.Font;
            AddButton.Text = I18N.GetString("&Add");
            DeleteButton.Text = I18N.GetString("&Delete");
            DuplicateButton.Text = I18N.GetString("Dupli&cate");
            ClipboardButton.Text = I18N.GetString("Cli&pboard");
            IPLabel.Text = I18N.GetString("Server Address");
            ServerPortLabel.Text = I18N.GetString("Server Port");
            PasswordLabel.Text = I18N.GetString("Password");
            ShowPasswdCheckBox.Text = I18N.GetString("Show Password");
            ProxyPortLabel.Text = I18N.GetString("Proxy Port");
            RemarksLabel.Text = I18N.GetString("Remarks");
            CorePortLabel.Text = I18N.GetString("Core Port");
            ServerGroupBox.Text = I18N.GetString("Server");
            OKButton.Text = I18N.GetString("OK");
            MyCancelButton.Text = I18N.GetString("Cancel");
            MoveUpButton.Text = I18N.GetString("Move &Up");
            MoveDownButton.Text = I18N.GetString("Move D&own");
            this.Text = I18N.GetString("Edit Servers");
        }

        private void LoadCurrentConfiguration()
        {
            _modifiedConfiguration = controller.GetConfigurationCopy();
            LoadConfiguration(_modifiedConfiguration);
            _lastSelectedIndex = _modifiedConfiguration.index;
            if (_lastSelectedIndex < 0 || _lastSelectedIndex >= ServersListBox.Items.Count)
            {
                _lastSelectedIndex = 0;
            }
            ServersListBox.SelectedIndex = _lastSelectedIndex;
            UpdateMoveUpAndDownButton();
            LoadSelectedServer();
        }

        private void UpdateMoveUpAndDownButton()
        {
            MoveUpButton.Enabled = ServersListBox.SelectedIndex != 0;
            MoveDownButton.Enabled = ServersListBox.SelectedIndex != ServersListBox.Items.Count - 1;
        }

        private void LoadSelectedServer()
        {
            if (ServersListBox.SelectedIndex >= 0 && ServersListBox.SelectedIndex < _modifiedConfiguration.configs.Count)
            {
                var server = _modifiedConfiguration.configs[ServersListBox.SelectedIndex];

                PortNum.Value = _modifiedConfiguration.localPort;
                CorePortNum.Value = _modifiedConfiguration.corePort;

                IPTextBox.Text = server.server;
                ServerPortText.Text = server.server_port.ToString();
                PasswordTextBox.Text = server.password;
                RemarksTextBox.Text = server.remarks;
            }
        }

        private void MoveConfigItem(int step)
        {
            int index = ServersListBox.SelectedIndex;
            var server = _modifiedConfiguration.configs[index];
            object item = ServersListBox.Items[index];

            _modifiedConfiguration.configs.Remove(server);
            _modifiedConfiguration.configs.Insert(index + step, server);
            _modifiedConfiguration.index += step;

            ServersListBox.BeginUpdate();
            ServersListBox.Enabled = false;
            _lastSelectedIndex = index + step;
            ServersListBox.Items.Remove(item);
            ServersListBox.Items.Insert(index + step, item);
            ServersListBox.Enabled = true;
            ServersListBox.SelectedIndex = index + step;
            ServersListBox.EndUpdate();

            UpdateMoveUpAndDownButton();
        }

        private bool SaveOldSelectedServer()
        {
            try
            {
                if (_lastSelectedIndex == -1 || _lastSelectedIndex >= _modifiedConfiguration.configs.Count)
                {
                    return true;
                }
                var server = new Server();

                if (Uri.CheckHostName(server.server = IPTextBox.Text.Trim()) == UriHostNameType.Unknown)
                {
                    MessageBox.Show(I18N.GetString("Invalid server address"));
                    IPTextBox.Focus();
                    return false;
                }

                var old = _modifiedConfiguration.configs[_lastSelectedIndex];
                server.server = IPTextBox.Text;
                if (int.TryParse(ServerPortText.Text, out var port) && port > 1000 && port < 65536)
                    server.server_port = port;
                server.password = PasswordTextBox.Text;
                server.remarks = RemarksTextBox.Text;

                var localPort = (int) PortNum.Value;
                var corePort = (int) CorePortNum.Value;
                Configuration.CheckServer(server.server);
                Configuration.CheckLocalPort(localPort);

                if (old != null) server.group = old.group;

                _modifiedConfiguration.configs[_lastSelectedIndex] = server;
                _modifiedConfiguration.localPort = localPort;
                _modifiedConfiguration.corePort = corePort;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return false;
        }

        private void LoadConfiguration(Configuration configuration)
        {
            ServersListBox.Items.Clear();
            foreach (var server in _modifiedConfiguration.configs)
            {
                ServersListBox.Items.Add(server.FriendlyName());
            }
        }

        private void ConfigForm_Shown(object sender, EventArgs e)
        {
            IPTextBox.Focus();
        }

        private void ConfigForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            controller.ConfigChanged -= Controller_ConfigChanged;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ConfigForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Sometimes the users may hit enter key by mistake, and the form will close without saving entries.

            if (e.KeyCode == Keys.Enter)
            {
                Server server = controller.GetCurrentServer();
                if (!SaveOldSelectedServer())
                {
                    return;
                }
                if (_modifiedConfiguration.configs.Count == 0)
                {
                    MessageBox.Show(I18N.GetString("Please add at least one server"));
                    return;
                }
                controller.SaveServers(_modifiedConfiguration.configs, _modifiedConfiguration.localPort, _modifiedConfiguration.corePort);
                controller.SelectServerIndex(_modifiedConfiguration.configs.IndexOf(server));
            }

        }

        private void ClipboardButton_Click(object sender, EventArgs e)
        {
            if (!SaveOldSelectedServer())
            {
                return;
            }
            var txt = Clipboard.GetText(TextDataFormat.Text);
            if (Server.TryParse(txt, out var saba))
            {
                _modifiedConfiguration.configs.Add(saba);
                LoadConfiguration(_modifiedConfiguration);
                ServersListBox.SelectedIndex = _modifiedConfiguration.configs.Count - 1;
                _lastSelectedIndex = ServersListBox.SelectedIndex;
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            _lastSelectedIndex = ServersListBox.SelectedIndex;
            if (_lastSelectedIndex >= 0 && _lastSelectedIndex < _modifiedConfiguration.configs.Count)
            {
                _modifiedConfiguration.configs.RemoveAt(_lastSelectedIndex);
            }
            if (_lastSelectedIndex >= _modifiedConfiguration.configs.Count)
            {
                _lastSelectedIndex = _modifiedConfiguration.configs.Count - 1;
            }
            ServersListBox.SelectedIndex = _lastSelectedIndex;
            LoadConfiguration(_modifiedConfiguration);
            ServersListBox.SelectedIndex = _lastSelectedIndex;
            LoadSelectedServer();
        }

        private void ShowPasswdCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.PasswordTextBox.UseSystemPasswordChar = !this.ShowPasswdCheckBox.Checked;
        }

        private void ServersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ServersListBox.CanSelect)
            {
                return;
            }
            if (_lastSelectedIndex == ServersListBox.SelectedIndex)
            {
                // we are moving back to oldSelectedIndex or doing a force move
                return;
            }
            if (!SaveOldSelectedServer())
            {
                // why this won't cause stack overflow?
                ServersListBox.SelectedIndex = _lastSelectedIndex;
                return;
            }
            if (_lastSelectedIndex >= 0)
            {
                ServersListBox.Items[_lastSelectedIndex] = _modifiedConfiguration.configs[_lastSelectedIndex].FriendlyName();
            }
            UpdateMoveUpAndDownButton();
            LoadSelectedServer();
            _lastSelectedIndex = ServersListBox.SelectedIndex;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!SaveOldSelectedServer())
            {
                return;
            }
            var server = Configuration.GetDefaultServer();
            _modifiedConfiguration.configs.Add(server);
            LoadConfiguration(_modifiedConfiguration);
            ServersListBox.SelectedIndex = _modifiedConfiguration.configs.Count - 1;
            _lastSelectedIndex = ServersListBox.SelectedIndex;
        }

        private void DuplicateButton_Click(object sender, EventArgs e)
        {
            if (!SaveOldSelectedServer())
            {
                return;
            }
            var currServer = _modifiedConfiguration.configs[_lastSelectedIndex];
            var currIndex = _modifiedConfiguration.configs.IndexOf(currServer);
            _modifiedConfiguration.configs.Insert(currIndex + 1, currServer);
            LoadConfiguration(_modifiedConfiguration);
            ServersListBox.SelectedIndex = currIndex + 1;
            _lastSelectedIndex = ServersListBox.SelectedIndex;
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            if (!SaveOldSelectedServer())
            {
                return;
            }
            if (ServersListBox.SelectedIndex > 0)
            {
                MoveConfigItem(-1);  // -1 means move backward
            }
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            if (!SaveOldSelectedServer())
            {
                return;
            }
            if (ServersListBox.SelectedIndex < ServersListBox.Items.Count - 1)
            {
                MoveConfigItem(+1);  // +1 means move forward
            }
        }

        private async void OKButton_Click(object sender, EventArgs e)
        {
            if (!SaveOldSelectedServer())
            {
                return;
            }
            if (_modifiedConfiguration.configs.Count == 0)
            {
                MessageBox.Show(I18N.GetString("Please add at least one server"));
                return;
            }

            OKButton.Enabled = false;
            OKButton.Text = I18N.GetString("Busy...");
            controller.SaveServers(_modifiedConfiguration.configs, _modifiedConfiguration.localPort,_modifiedConfiguration.corePort);
            await controller.SelectServerIndex(ServersListBox.SelectedIndex);
            Close();
        }

        private void ServerPortText_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }
    }
}
