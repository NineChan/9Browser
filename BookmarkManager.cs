using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace _9Browser
{
    public class Bookmark
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime DateAdded { get; set; }
    }

    public class BookmarkManager
    {
        private List<Bookmark> bookmarks = new List<Bookmark>();
        private string bookmarksPath;
        private ToolStripMenuItem bookmarksMenu;
        private Form1 mainForm;
        private TabManager tabManager;
        private MenuStrip mainMenu;

        public BookmarkManager(Form1 form, TabManager manager)
        {
            mainForm = form;
            tabManager = manager;
            bookmarksPath = Path.Combine(Application.StartupPath, "bookmarks.json");
            LoadBookmarks();
            CreateBookmarksMenu();
        }

        private void LoadBookmarks()
        {
            if (File.Exists(bookmarksPath))
            {
                try
                {
                    var json = File.ReadAllText(bookmarksPath);
                    bookmarks = JsonSerializer.Deserialize<List<Bookmark>>(json) ?? new List<Bookmark>();
                }
                catch { bookmarks = new List<Bookmark>(); }
            }
        }

        private void SaveBookmarks()
        {
            var json = JsonSerializer.Serialize(bookmarks);
            File.WriteAllText(bookmarksPath, json);
        }

        private void CreateBookmarksMenu()
        {
            mainMenu = new MenuStrip();
            mainMenu.Dock = DockStyle.Top;

            bookmarksMenu = new ToolStripMenuItem("Закладки");
            mainMenu.Items.Add(bookmarksMenu);

            var addBookmarkItem = new ToolStripMenuItem("Добавить закладку", null, AddCurrentBookmark);
            bookmarksMenu.DropDownItems.Add(addBookmarkItem);
            bookmarksMenu.DropDownItems.Add(new ToolStripSeparator());

            RefreshBookmarksMenu();

            mainForm.Controls.Add(mainMenu);
            mainMenu.BringToFront();
        }

        private void RefreshBookmarksMenu()
        {
            while (bookmarksMenu.DropDownItems.Count > 2)
            {
                bookmarksMenu.DropDownItems.RemoveAt(2);
            }

            if (bookmarks.Count == 0)
            {
                var emptyItem = new ToolStripMenuItem("Нет закладок");
                emptyItem.Enabled = false;
                bookmarksMenu.DropDownItems.Add(emptyItem);
            }
            else
            {
                foreach (var bookmark in bookmarks)
                {
                    var item = new ToolStripMenuItem(bookmark.Title, null, (s, e) =>
                    {
                        var browser = tabManager.GetCurrentBrowser();
                        if (browser != null)
                        {
                            browser.Navigate(bookmark.Url);
                        }
                    });
                    item.ToolTipText = bookmark.Url;
                    bookmarksMenu.DropDownItems.Add(item);
                }
            }
        }

        private void AddCurrentBookmark(object sender, EventArgs e)
        {
            var browser = tabManager.GetCurrentBrowser();
            if (browser != null && browser.WebView.CoreWebView2 != null)
            {
                var url = browser.WebView.CoreWebView2.Source?.ToString();
                var title = browser.WebView.CoreWebView2.DocumentTitle;

                if (!string.IsNullOrEmpty(url) && url != "about:blank")
                {
                    var bookmark = new Bookmark
                    {
                        Title = string.IsNullOrEmpty(title) ? url : title,
                        Url = url,
                        DateAdded = DateTime.Now
                    };

                    bookmarks.Add(bookmark);
                    SaveBookmarks();
                    RefreshBookmarksMenu();

                    MessageBox.Show($"Закладка добавлена: {bookmark.Title}", "Закладки",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public void ShowBookmarksManager()
        {
            var form = new Form
            {
                Text = "Управление закладками",
                Size = new System.Drawing.Size(600, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };

            listView.Columns.Add("Название", 300);
            listView.Columns.Add("URL", 250);
            listView.Columns.Add("Дата", 120);

            foreach (var bookmark in bookmarks)
            {
                var item = new ListViewItem(bookmark.Title);
                item.SubItems.Add(bookmark.Url);
                item.SubItems.Add(bookmark.DateAdded.ToShortDateString());
                item.Tag = bookmark;
                listView.Items.Add(item);
            }

            var panel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var deleteBtn = new Button { Text = "Удалить", Location = new System.Drawing.Point(10, 5), Size = new System.Drawing.Size(100, 30) };
            var closeBtn = new Button { Text = "Закрыть", Location = new System.Drawing.Point(120, 5), Size = new System.Drawing.Size(100, 30) };

            deleteBtn.Click += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0)
                {
                    var bookmark = (Bookmark)listView.SelectedItems[0].Tag;
                    bookmarks.Remove(bookmark);
                    SaveBookmarks();
                    RefreshBookmarksMenu();
                    listView.Items.Remove(listView.SelectedItems[0]);
                }
            };

            closeBtn.Click += (s, e) => form.Close();

            panel.Controls.Add(deleteBtn);
            panel.Controls.Add(closeBtn);
            form.Controls.Add(listView);
            form.Controls.Add(panel);
            form.ShowDialog(mainForm);
        }
    }
}