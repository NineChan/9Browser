using System;
using System.Drawing;
using System.Windows.Forms;

namespace _9Browser
{
    public class BrowserUI
    {
        private ToolStrip mainStrip;
        private ToolStripButton backButton;
        private ToolStripButton forwardButton;
        private ToolStripButton refreshButton;
        private ToolStripButton stopButton;
        private ToolStripButton homeButton;
        private ToolStripButton downloadsButton;
        private ToolStripButton extensionsButton;
        private ToolStripButton bookmarksButton;
        private ToolStripButton newTabButton;
        private ToolStripTextBox urlBox;
        private ToolStripProgressBar progressBar;
        private ToolStripLabel statusLabel;

        private BrowserCore currentBrowserCore;
        private Form1 mainForm;

        private static BrowserUI instance;

        public static BrowserUI GetInstance(Form1 form)
        {
            if (instance == null)
                instance = new BrowserUI(form);
            return instance;
        }

        private BrowserUI(Form1 form)
        {
            mainForm = form;
            CreateUI();
        }

        public void SetCurrentBrowser(BrowserCore core)
        {
            currentBrowserCore = core;
            UpdateNavigationButtons();
            if (core?.WebView?.CoreWebView2?.Source != null)
                UpdateUrlBox(core.WebView.CoreWebView2.Source.ToString());
        }

        private void CreateUI()
        {
            mainStrip = new ToolStrip();
            mainStrip.Dock = DockStyle.Top;
            mainStrip.GripStyle = ToolStripGripStyle.Hidden;

            backButton = CreateButton("◀", "Назад", (s, e) => currentBrowserCore?.GoBack());
            forwardButton = CreateButton("▶", "Вперед", (s, e) => currentBrowserCore?.GoForward());
            refreshButton = CreateButton("⟳", "Обновить", (s, e) => currentBrowserCore?.Refresh());
            stopButton = CreateButton("✖", "Стоп", (s, e) => currentBrowserCore?.Stop());
            homeButton = CreateButton("🏠", "Домой", (s, e) => currentBrowserCore?.Navigate("https://ninechan.github.io/9search/"));
            newTabButton = CreateButton("+", "Новая вкладка", (s, e) =>
            {
                var field = typeof(Form1).GetField("tabManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var tabManager = field?.GetValue(mainForm) as dynamic;
                tabManager?.CreateNewTab("https://ninechan.github.io/9search/");
            });

            urlBox = new ToolStripTextBox();
            urlBox.Size = new Size(600, 25);
            urlBox.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) currentBrowserCore?.Navigate(urlBox.Text); };

            downloadsButton = CreateButton("⬇", "Загрузки", (s, e) => currentBrowserCore?.DownloadManager.ShowDownloadsDialog());
            extensionsButton = CreateButton("🧩", "Расширения", (s, e) => MessageBox.Show("К сожалению функция сломанна и пока что недоступна\n\nпростите :')")); // currentBrowserCore?.ExtensionManager.ShowExtensionsDialog())
            bookmarksButton = CreateButton("★", "Закладки", (s, e) =>
            {
                var field = typeof(Form1).GetField("bookmarkManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var bookmarkManager = field?.GetValue(mainForm) as dynamic;
                bookmarkManager?.ShowBookmarksManager();
            });

            progressBar = new ToolStripProgressBar();
            progressBar.Size = new Size(100, 20);
            progressBar.Visible = false;

            statusLabel = new ToolStripLabel("Готов");

            mainStrip.Items.AddRange(new ToolStripItem[] {
                newTabButton, backButton, forwardButton, refreshButton, stopButton, homeButton,
                new ToolStripSeparator(), urlBox, new ToolStripSeparator(),
                bookmarksButton, downloadsButton, extensionsButton, new ToolStripSeparator(),
                progressBar, statusLabel
            });

            mainForm.Controls.Add(mainStrip);
            mainStrip.BringToFront();
        }

        private ToolStripButton CreateButton(string text, string tooltip, EventHandler clickHandler)
        {
            var button = new ToolStripButton(text);
            button.ToolTipText = tooltip;
            button.Click += clickHandler;
            return button;
        }

        public void UpdateProgress(bool isNavigating)
        {
            if (mainForm.InvokeRequired)
            {
                mainForm.Invoke(new Action(() => UpdateProgress(isNavigating)));
                return;
            }
            progressBar.Visible = isNavigating;
            if (isNavigating)
                progressBar.Style = ProgressBarStyle.Marquee;
        }

        public void UpdateStatus(string status)
        {
            if (mainForm.InvokeRequired)
            {
                mainForm.Invoke(new Action(() => UpdateStatus(status)));
                return;
            }
            statusLabel.Text = status;
        }

        public void UpdateUrlBox(string url)
        {
            if (mainForm.InvokeRequired)
            {
                mainForm.Invoke(new Action(() => UpdateUrlBox(url)));
                return;
            }
            if (urlBox.Text != url)
                urlBox.Text = url ?? "";
        }

        public void UpdateNavigationButtons()
        {
            if (mainForm.InvokeRequired)
            {
                mainForm.Invoke(new Action(() => UpdateNavigationButtons()));
                return;
            }
            if (currentBrowserCore != null)
            {
                backButton.Enabled = currentBrowserCore.CanGoBack;
                forwardButton.Enabled = currentBrowserCore.CanGoForward;
            }
        }
    }
}