﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrojanShell.Model;
using TrojanShell.Properties;
using ZXing.QrCode.Internal;

namespace TrojanShell.View
{
    public partial class QRCodeForm : Form
    {
        private string code;

        public QRCodeForm(string code)
        {
            this.code = code;
            InitializeComponent();
            this.Icon = Resources.horse;
            // ReSharper disable once VirtualMemberCallInConstructor
            Text = I18N.GetString("Sit back and relax");
            Enabled = false;
        }

        private void GenQR(string ssconfig)
        {
            string qrText = ssconfig;
            QRCode code = ZXing.QrCode.Internal.Encoder.encode(qrText, ErrorCorrectionLevel.M);
            ByteMatrix m = code.Matrix;
            int blockSize = Math.Max(pictureBox1.Height/m.Height, 1);

            var qrWidth = m.Width*blockSize;
            var qrHeight = m.Height*blockSize;
            var dWidth = pictureBox1.Width - qrWidth;
            var dHeight = pictureBox1.Height - qrHeight;
            var maxD = Math.Max(dWidth, dHeight);
            pictureBox1.SizeMode = maxD >= 7*blockSize ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;

            Bitmap drawArea = new Bitmap((m.Width*blockSize), (m.Height*blockSize));
            using (Graphics g = Graphics.FromImage(drawArea))
            {
                g.Clear(Color.White);
                using (Brush b = new SolidBrush(Color.Black))
                {
                    for (int row = 0; row < m.Width; row++)
                    {
                        for (int col = 0; col < m.Height; col++)
                        {
                            if (m[row, col] != 0)
                            {
                                g.FillRectangle(b, blockSize*row, blockSize*col, blockSize, blockSize);
                            }
                        }
                    }
                }
            }
            pictureBox1.Image = drawArea;
        }

        private async void QRCodeForm_Load(object sender, EventArgs e)
        {
            var servers = await Configuration.LoadAsync();
            // ReSharper disable once JoinDeclarationAndInitializer
            List<KeyValuePair<string, string>> serverDatas;
            serverDatas = await Task.Run(() => serverDatas = servers.configs.Select(
                server =>
                    new KeyValuePair<string, string>(server.ToURL(), server.FriendlyName())
            ).ToList());
            Text = I18N.GetString("QRCode and URL");
            Enabled = true;
            listBox1.DataSource = serverDatas;
            var selectIndex = serverDatas.FindIndex(serverData => serverData.Key.StartsWith(code));
            if (selectIndex >= 0) listBox1.SetSelected(selectIndex, true);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var url = (sender as ListBox)?.SelectedValue.ToString();
            GenQR(url);
            textBoxURL.Text = url;
        }

        private void textBoxURL_Click(object sender, EventArgs e)
        {
            textBoxURL.SelectAll();
        }
    }
}
