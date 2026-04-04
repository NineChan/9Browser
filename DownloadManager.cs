using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows.Forms;

namespace _9Browser
{
    public class DownloadManager
    {
        private CoreWebView2 webView;
        private string downloadsPath;
        private Form1 mainForm;

        public void Initialize(CoreWebView2 webView, Form1 form)
        {
            this.webView = webView;
            this.mainForm = form;
            downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            Directory.CreateDirectory(downloadsPath);

            webView.DownloadStarting += OnDownloadStarting;
        }

        private void OnDownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var downloadOperation = e.DownloadOperation;
            string fileName = downloadOperation.ResultFilePath;
            if (string.IsNullOrEmpty(fileName))
                fileName = Path.GetFileName(e.DownloadOperation.Uri);

            e.ResultFilePath = Path.Combine(downloadsPath, fileName);

            var downloadForm = new Form
            {
                Text = $"Downloading: {fileName}",
                Size = new System.Drawing.Size(450, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 35,
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100
            };

            var lblStatus = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            var lblSpeed = new Label
            {
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Dock = DockStyle.Bottom,
                Height = 35
            };

            downloadForm.Controls.AddRange(new Control[] { progressBar, lblStatus, lblSpeed, btnCancel });

            DateTime startTime = DateTime.Now;
            long lastBytes = 0;
            bool isCompleted = false;

            downloadOperation.BytesReceivedChanged += (s, args) =>
            {
                downloadForm.Invoke(new Action(() =>
                {
                    if (isCompleted) return;

                    long totalBytes = (long)(downloadOperation.TotalBytesToReceive ?? 0);
                    long receivedBytes = (long)(downloadOperation.BytesReceived);
                    double percentComplete = totalBytes > 0 ? (receivedBytes * 100.0 / totalBytes) : 0;

                    progressBar.Value = Math.Min(100, Math.Max(0, (int)percentComplete));
                    lblStatus.Text = $"{percentComplete:F1}% - {FormatBytes(receivedBytes)} / {FormatBytes(totalBytes)}";

                    var elapsed = (DateTime.Now - startTime).TotalSeconds;
                    if (elapsed > 0 && receivedBytes > lastBytes)
                    {
                        var speed = (receivedBytes - lastBytes) / elapsed;
                        lblSpeed.Text = $"Speed: {FormatBytes((long)speed)}/s";
                    }
                    lastBytes = receivedBytes;
                    startTime = DateTime.Now;
                }));
            };

            downloadOperation.StateChanged += (s, args) =>
            {
                downloadForm.Invoke(new Action(() =>
                {
                    if (downloadOperation.State == CoreWebView2DownloadState.Completed)
                    {
                        isCompleted = true;
                        lblStatus.Text = "✓ Download completed!";
                        progressBar.Value = 100;
                        btnCancel.Text = "Open Folder";
                        btnCancel.Click -= (c, ce) => { };
                        btnCancel.Click += (c, ce) => System.Diagnostics.Process.Start("explorer.exe", downloadsPath);
                    }
                    else if (downloadOperation.State == CoreWebView2DownloadState.Interrupted)
                    {
                        isCompleted = true;
                        lblStatus.Text = $"✗ Interrupted: {downloadOperation.InterruptReason}";
                        btnCancel.Text = "Close";
                    }
                }));
            };

            btnCancel.Click += (s, args) =>
            {
                if (downloadOperation.State == CoreWebView2DownloadState.InProgress)
                    downloadOperation.Cancel();
                downloadForm.Close();
            };

            downloadForm.ShowDialog(mainForm);
        }

        private string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public void ShowDownloadsDialog()
        {
            MessageBox.Show("Downloads folder: " + downloadsPath, "Downloads",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}