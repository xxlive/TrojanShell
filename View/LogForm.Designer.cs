﻿namespace TrojanShell.View
{
    partial class LogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.LogMessageTextBox = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.FileMenuItem = new System.Windows.Forms.MenuItem();
            this.OpenLocationMenuItem = new System.Windows.Forms.MenuItem();
            this.ExitMenuItem = new System.Windows.Forms.MenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TopMostCheckBox = new System.Windows.Forms.CheckBox();
            this.ChangeFontButton = new System.Windows.Forms.Button();
            this.CleanLogsButton = new System.Windows.Forms.Button();
            this.WrapTextCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LogMessageTextBox
            // 
            this.LogMessageTextBox.BackColor = System.Drawing.Color.Black;
            this.LogMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogMessageTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogMessageTextBox.ForeColor = System.Drawing.Color.White;
            this.LogMessageTextBox.Location = new System.Drawing.Point(7, 100);
            this.LogMessageTextBox.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.LogMessageTextBox.MaxLength = 2147483647;
            this.LogMessageTextBox.Multiline = true;
            this.LogMessageTextBox.Name = "LogMessageTextBox";
            this.LogMessageTextBox.ReadOnly = true;
            this.LogMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogMessageTextBox.Size = new System.Drawing.Size(1171, 721);
            this.LogMessageTextBox.TabIndex = 0;
            this.LogMessageTextBox.WordWrap = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FileMenuItem});
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.Index = 0;
            this.FileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.OpenLocationMenuItem,
            this.ExitMenuItem});
            this.FileMenuItem.Text = "&File";
            // 
            // OpenLocationMenuItem
            // 
            this.OpenLocationMenuItem.Index = 0;
            this.OpenLocationMenuItem.Text = "&Open Location";
            this.OpenLocationMenuItem.Click += new System.EventHandler(this.OpenLocationMenuItem_Click);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Index = 1;
            this.ExitMenuItem.Text = "E&xit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.TopMostCheckBox);
            this.panel1.Controls.Add(this.ChangeFontButton);
            this.panel1.Controls.Add(this.CleanLogsButton);
            this.panel1.Controls.Add(this.WrapTextCheckBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(7, 7);
            this.panel1.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1171, 79);
            this.panel1.TabIndex = 1;
            // 
            // TopMostCheckBox
            // 
            this.TopMostCheckBox.AutoSize = true;
            this.TopMostCheckBox.Location = new System.Drawing.Point(674, 21);
            this.TopMostCheckBox.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.TopMostCheckBox.Name = "TopMostCheckBox";
            this.TopMostCheckBox.Size = new System.Drawing.Size(139, 32);
            this.TopMostCheckBox.TabIndex = 3;
            this.TopMostCheckBox.Text = "&Top most";
            this.TopMostCheckBox.UseVisualStyleBackColor = true;
            this.TopMostCheckBox.CheckedChanged += new System.EventHandler(this.TopMostCheckBox_CheckedChanged);
            // 
            // ChangeFontButton
            // 
            this.ChangeFontButton.Location = new System.Drawing.Point(232, 9);
            this.ChangeFontButton.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.ChangeFontButton.Name = "ChangeFontButton";
            this.ChangeFontButton.Size = new System.Drawing.Size(163, 54);
            this.ChangeFontButton.TabIndex = 2;
            this.ChangeFontButton.Text = "&Font";
            this.ChangeFontButton.UseVisualStyleBackColor = true;
            this.ChangeFontButton.Click += new System.EventHandler(this.ChangeFontButton_Click);
            // 
            // CleanLogsButton
            // 
            this.CleanLogsButton.Location = new System.Drawing.Point(20, 9);
            this.CleanLogsButton.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.CleanLogsButton.Name = "CleanLogsButton";
            this.CleanLogsButton.Size = new System.Drawing.Size(163, 54);
            this.CleanLogsButton.TabIndex = 1;
            this.CleanLogsButton.Text = "&Clean logs";
            this.CleanLogsButton.UseVisualStyleBackColor = true;
            this.CleanLogsButton.Click += new System.EventHandler(this.CleanLogsButton_Click);
            // 
            // WrapTextCheckBox
            // 
            this.WrapTextCheckBox.AutoSize = true;
            this.WrapTextCheckBox.Location = new System.Drawing.Point(453, 21);
            this.WrapTextCheckBox.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.WrapTextCheckBox.Name = "WrapTextCheckBox";
            this.WrapTextCheckBox.Size = new System.Drawing.Size(143, 32);
            this.WrapTextCheckBox.TabIndex = 0;
            this.WrapTextCheckBox.Text = "&Wrap text";
            this.WrapTextCheckBox.UseVisualStyleBackColor = true;
            this.WrapTextCheckBox.CheckedChanged += new System.EventHandler(this.WrapTextCheckBox_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.LogMessageTextBox, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1185, 828);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1185, 828);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Yahei", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.Menu = this.mainMenu1;
            this.Name = "LogForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Log Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogForm_FormClosing);
            this.Load += new System.EventHandler(this.LogForm_Load);
            this.Shown += new System.EventHandler(this.LogForm_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox LogMessageTextBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem FileMenuItem;
        private System.Windows.Forms.MenuItem OpenLocationMenuItem;
        private System.Windows.Forms.MenuItem ExitMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox WrapTextCheckBox;
        private System.Windows.Forms.Button CleanLogsButton;
        private System.Windows.Forms.Button ChangeFontButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox TopMostCheckBox;
    }
}