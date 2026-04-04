using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _9Browser
{
    public class TabManager
    {
        private Panel tabHeaderPanel;
        private Panel tabContentPanel;
        private Form1 mainForm;
        private int tabCounter = 1;
        private BrowserUI browserUI;
        private List<TabButton> tabButtons = new List<TabButton>();
        private TabButton activeTabButton = null;
        private int tabButtonX = 40;

        public TabManager(Form1 form)
        {
            mainForm = form;
            browserUI = BrowserUI.GetInstance(form);
            CreateTabContainer();
        }

        private void CreateTabContainer()
        {
            tabHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            tabContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            mainForm.Controls.Add(tabContentPanel);
            mainForm.Controls.Add(tabHeaderPanel);
            tabHeaderPanel.BringToFront();

            var newTabButton = new Button
            {
                Text = "+",
                Size = new Size(30, 28),
                Location = new Point(5, 3),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            newTabButton.FlatAppearance.BorderSize = 0;
            newTabButton.Click += (s, e) => CreateNewTab("https://ninechan.github.io/9search/");
            tabHeaderPanel.Controls.Add(newTabButton);
        }

        private void RelayoutTabs()
        {
            int x = 40;
            foreach (var button in tabButtons)
            {
                button.Location = new Point(x, 3);
                x += button.Width + 5;
            }
        }

        public void CreateNewTab(string url)
        {
            var tabId = tabCounter++;
            var tabButton = new TabButton(tabId, $"Новая вкладка {tabId}");
            tabButton.Size = new Size(110, 28);
            tabButton.Click += (s, e) => SwitchToTab(tabButton);
            tabButton.CloseClick += (s, e) => CloseTab(tabButton);

            tabHeaderPanel.Controls.Add(tabButton);
            tabButtons.Add(tabButton);
            RelayoutTabs();

            var browserCore = new BrowserCore();

            var browserPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            var innerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 70, 0, 0),
                Margin = new Padding(0),
                BackColor = Color.White
            };

            browserPanel.Controls.Add(innerPanel);
            tabContentPanel.Controls.Add(browserPanel);

            var tabData = new TabData
            {
                BrowserCore = browserCore,
                Panel = browserPanel,
                InnerPanel = innerPanel,
                Button = tabButton
            };

            tabButton.Tag = tabData;

            browserCore.SetTabButton(tabButton);
            browserCore.Initialize(mainForm, browserUI);

            browserCore.WebView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                if (e.IsSuccess)
                {
                    browserCore.WebView.Invoke(new Action(() =>
                    {
                        browserCore.WebView.Parent = innerPanel;
                        browserCore.WebView.Dock = DockStyle.Fill;
                        browserCore.WebView.Margin = new Padding(0);
                    }));

                    browserCore.WebView.CoreWebView2.DocumentTitleChanged += (s2, e2) =>
                    {
                        var title = browserCore.WebView.CoreWebView2.DocumentTitle;
                        if (!string.IsNullOrEmpty(title))
                        {
                            tabButton.Invoke(new Action(() =>
                            {
                                tabButton.Text = title.Length > 15 ? title.Substring(0, 12) + "..." : title;
                            }));
                        }
                    };

                    browserCore.Navigate(url);

                    if (activeTabButton == null)
                    {
                        SwitchToTab(tabButton);
                    }
                }
            };
        }

        private void SwitchToTab(TabButton button)
        {
            if (activeTabButton == button) return;

            if (activeTabButton != null)
            {
                activeTabButton.IsActive = false;
                var oldData = activeTabButton.Tag as TabData;
                if (oldData?.Panel != null)
                    oldData.Panel.Visible = false;
            }

            activeTabButton = button;
            activeTabButton.IsActive = true;

            var tabData = button.Tag as TabData;
            if (tabData?.Panel != null)
            {
                tabData.Panel.Visible = true;
                browserUI.SetCurrentBrowser(tabData.BrowserCore);
            }
        }

        private void CloseTab(TabButton button)
        {
            var index = tabButtons.IndexOf(button);
            var tabData = button.Tag as TabData;

            if (tabData?.BrowserCore?.WebView != null)
            {
                tabData.BrowserCore.WebView.Dispose();
            }

            if (tabData?.Panel != null)
            {
                tabContentPanel.Controls.Remove(tabData.Panel);
                tabData.Panel.Dispose();
            }

            tabHeaderPanel.Controls.Remove(button);
            tabButtons.Remove(button);
            button.Dispose();

            RelayoutTabs();

            if (tabButtons.Count == 0)
            {
                CreateNewTab("https://ninechan.github.io/9search/");
            }
            else if (activeTabButton == button)
            {
                var newActiveIndex = index > 0 ? index - 1 : 0;
                if (newActiveIndex < tabButtons.Count)
                {
                    SwitchToTab(tabButtons[newActiveIndex]);
                }
            }
        }

        public BrowserCore GetCurrentBrowser()
        {
            if (activeTabButton?.Tag is TabData tabData)
            {
                return tabData.BrowserCore;
            }
            return null;
        }

        private class TabData
        {
            public BrowserCore BrowserCore { get; set; }
            public Panel Panel { get; set; }
            public Panel InnerPanel { get; set; }
            public TabButton Button { get; set; }
        }
    }

    public class TabButton : UserControl
    {
        private Label titleLabel;
        private Label closeButton;
        private PictureBox faviconBox;
        private bool isActive = false;
        public event EventHandler CloseClick;

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                UpdateAppearance();
            }
        }

        public override string Text
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        public void SetFavicon(Image icon)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetFavicon(icon)));
                return;
            }

            try
            {
                if (icon != null)
                {
                    // Создаем новую иконку нужного размера
                    var newIcon = new Bitmap(icon, new Size(16, 16));
                    faviconBox.Image = newIcon;
                    faviconBox.Visible = true;
                    titleLabel.Location = new Point(25, 5);
                    titleLabel.Size = new Size(55, 20);
                    this.Refresh();
                }
            }
            catch { }
        }

        public TabButton(int id, string text)
        {
            InitializeComponent();
            titleLabel.Text = text;
        }

        private void InitializeComponent()
        {
            this.faviconBox = new PictureBox();
            this.titleLabel = new Label();
            this.closeButton = new Label();
            this.SuspendLayout();

            // Настройка иконки
            this.faviconBox.Size = new Size(16, 16);
            this.faviconBox.Location = new Point(5, 6);
            this.faviconBox.SizeMode = PictureBoxSizeMode.Zoom;
            this.faviconBox.Visible = true; // По умолчанию видима
            this.faviconBox.BackColor = Color.Transparent;

            // Временная иконка-заглушка
            var dummyBitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(dummyBitmap))
            {
                g.Clear(Color.LightGray);
            }
            faviconBox.Image = dummyBitmap;

            this.titleLabel.AutoSize = false;
            this.titleLabel.Location = new Point(25, 5);
            this.titleLabel.Size = new Size(55, 20);
            this.titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.titleLabel.Click += (s, e) => this.OnClick(e);

            this.closeButton.Text = "✖";
            this.closeButton.Font = new Font("Segoe UI", 8);
            this.closeButton.ForeColor = Color.Gray;
            this.closeButton.Size = new Size(20, 20);
            this.closeButton.Location = new Point(85, 4);
            this.closeButton.TextAlign = ContentAlignment.MiddleCenter;
            this.closeButton.Click += (s, e) => CloseClick?.Invoke(this, EventArgs.Empty);

            this.Controls.Add(this.faviconBox);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.closeButton);
            this.ResumeLayout();

            UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            if (isActive)
            {
                this.BackColor = Color.White;
                this.titleLabel.ForeColor = Color.Black;
                this.titleLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                this.BorderStyle = BorderStyle.FixedSingle;
            }
            else
            {
                this.BackColor = Color.FromArgb(240, 240, 240);
                this.titleLabel.ForeColor = Color.DimGray;
                this.titleLabel.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                this.BorderStyle = BorderStyle.None;
            }
        }
    }
}