using MetroFramework.Forms;
using System;
using System.Windows.Forms;

namespace _9Browser
{
    public partial class Form1 : MetroForm
    {
        private TabManager tabManager;
        private BookmarkManager bookmarkManager;

        public Form1()
        {
            InitializeComponent();

            this.Padding = new Padding(0);
            this.Margin = new Padding(0);
            this.WindowState = FormWindowState.Maximized;

            InitializeBrowser();
        }

        private void InitializeBrowser()
        {
            // Сначала создаем менеджер вкладок (создаст панели)
            tabManager = new TabManager(this);

            // Потом создаем UI (панель навигации)
            var browserUI = BrowserUI.GetInstance(this);

            // Потом создаем менеджер закладок (меню)
            bookmarkManager = new BookmarkManager(this, tabManager);

            // Создаем первую вкладку
            tabManager.CreateNewTab("https://ninechan.github.io/9search/");
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }
        private void Zakladki_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }
        private void basicStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }
    }
}