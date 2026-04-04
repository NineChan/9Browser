using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;

namespace _9Browser
{
    public class BrowserCore
    {
        public WebView2 WebView { get; private set; }
        public ExtensionManager ExtensionManager { get; private set; }
        public DownloadManager DownloadManager { get; private set; }
        private Form1 mainForm;
        private TabButton currentTabButton;

        public async void Initialize(Form1 form, BrowserUI ui)
        {
            mainForm = form;

            WebView = new WebView2();
            WebView.Dock = DockStyle.Fill;

            ExtensionManager = new ExtensionManager();
            DownloadManager = new DownloadManager();

            WebView.NavigationStarting += OnNavigationStarting;
            WebView.NavigationCompleted += OnNavigationCompleted;
            WebView.SourceChanged += OnSourceChanged;
            WebView.CoreWebView2InitializationCompleted += OnCoreWebView2InitComplete;

            WebView.CreationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = Path.Combine(Application.StartupPath, "WebView2Data")
            };

            await WebView.EnsureCoreWebView2Async(null);
        }

        private void OnCoreWebView2InitComplete(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                ConfigureWebViewSettings();
                ExtensionManager.LoadExtensions(WebView.CoreWebView2);
                DownloadManager.Initialize(WebView.CoreWebView2, mainForm);
                WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                WebView.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
            }
        }

        private async void LoadFaviconFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || currentTabButton == null)
                    return;

                Uri uri = new Uri(url);
                string domain = uri.Host;
                string baseUrl = $"{uri.Scheme}://{domain}";

                if (domain.Contains("rasko6.github.io"))
                {
                    string directIconUrl = "https://rasko6.github.io/9chan/icon.png";
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.Timeout = TimeSpan.FromSeconds(5);
                            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                            var bytes = await httpClient.GetByteArrayAsync(directIconUrl);
                            if (bytes != null && bytes.Length > 0)
                            {
                                using (var ms = new MemoryStream(bytes))
                                {
                                    var icon = Image.FromStream(ms);
                                    currentTabButton.SetFavicon(icon);
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Direct icon error: {ex.Message}");
                    }
                }

                string[] faviconPaths = {
                    "/icon.png",
                    "/favicon.ico",
                    "/favicon.png",
                    "/favicon.svg",
                    "/icon.ico",
                    "/icon.svg",
                    "/assets/favicon.ico",
                    "/assets/favicon.png",
                    "/static/favicon.ico",
                    "/static/favicon.png"
                };

                foreach (var path in faviconPaths)
                {
                    try
                    {
                        string fullUrl = baseUrl + path;
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.Timeout = TimeSpan.FromSeconds(3);
                            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                            var response = await httpClient.GetAsync(fullUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                var bytes = await response.Content.ReadAsByteArrayAsync();
                                if (bytes != null && bytes.Length > 0 && bytes.Length < 100000)
                                {
                                    using (var ms = new MemoryStream(bytes))
                                    {
                                        var icon = Image.FromStream(ms);
                                        currentTabButton.SetFavicon(icon);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                string[] apis = {
                    $"https://www.google.com/s2/favicons?domain={domain}&sz=32",
                    $"https://favicon.yandex.net/favicon/{domain}",
                    $"https://icons.duckduckgo.com/ip3/{domain}.ico"
                };

                foreach (var apiUrl in apis)
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.Timeout = TimeSpan.FromSeconds(3);
                            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                            var bytes = await httpClient.GetByteArrayAsync(apiUrl);
                            if (bytes != null && bytes.Length > 0 && bytes.Length < 100000)
                            {
                                using (var ms = new MemoryStream(bytes))
                                {
                                    var icon = Image.FromStream(ms);
                                    currentTabButton.SetFavicon(icon);
                                    return;
                                }
                            }
                        }
                    }
                    catch { }
                }

                ShowDefaultFavicon(domain);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Favicon error: {ex.Message}");
                ShowDefaultFavicon("");
            }
        }

        private void ShowDefaultFavicon(string domain)
        {
            try
            {
                var bitmap = new Bitmap(16, 16);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.FromArgb(66, 133, 244));
                    if (!string.IsNullOrEmpty(domain) && domain.Length > 0)
                    {
                        using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                        {
                            string letter = domain[0].ToString().ToUpper();
                            g.DrawString(letter, font, Brushes.White, 4, 2);
                        }
                    }
                    else
                    {
                        using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                        {
                            g.DrawString("?", font, Brushes.White, 5, 2);
                        }
                    }
                }
                currentTabButton?.SetFavicon(bitmap);
            }
            catch { }
        }

        private void OnDocumentTitleChanged(object sender, object e) { }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();

            if (message == "get-extensions")
            {
                var extensions = ExtensionManager.GetExtensionsList();
                WebView.CoreWebView2.PostWebMessageAsJson(extensions);
            }
            else if (message == "manage-extensions")
            {
                ExtensionManager.ShowExtensionsDialog();
            }
            else if (message == "open-downloads-folder")
            {
                DownloadManager.ShowDownloadsDialog();
            }
            else if (message == "clear-history")
            {
                WebView.CoreWebView2.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.BrowsingHistory);
            }
            else if (message == "clear-cache")
            {
                WebView.CoreWebView2.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.DiskCache);
            }
        }

        private void ConfigureWebViewSettings()
        {
            var settings = WebView.CoreWebView2.Settings;
            settings.IsStatusBarEnabled = true;
            settings.IsWebMessageEnabled = true;
            settings.AreDefaultScriptDialogsEnabled = true;
            settings.IsPasswordAutosaveEnabled = true;
            settings.IsGeneralAutofillEnabled = true;
            settings.IsScriptEnabled = true;

            WebView.CoreWebView2.NewWindowRequested += (s, e) => { e.Handled = true; WebView.CoreWebView2.Navigate(e.Uri); };
            WebView.CoreWebView2.PermissionRequested += (s, e) => { e.State = CoreWebView2PermissionState.Allow; };
        }

        private void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri != null && InternalPages.IsInternalUrl(e.Uri))
            {
                e.Cancel = true;
                var html = InternalPages.GetInternalPage(e.Uri);
                WebView.CoreWebView2.NavigateToString(html);
                return;
            }

            var browserUI = BrowserUI.GetInstance(mainForm);
            browserUI.UpdateProgress(true);
            browserUI.UpdateStatus($"Загрузка: {e.Uri}");
        }

        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var browserUI = BrowserUI.GetInstance(mainForm);
            browserUI.UpdateProgress(false);
            browserUI.UpdateStatus("Готов");
            browserUI.UpdateNavigationButtons();

            if (WebView.CoreWebView2?.Source != null)
            {
                LoadFaviconFromUrl(WebView.CoreWebView2.Source.ToString());
            }
        }

        private void OnSourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            if (WebView.CoreWebView2?.Source != null)
            {
                var browserUI = BrowserUI.GetInstance(mainForm);
                browserUI.UpdateUrlBox(WebView.CoreWebView2.Source.ToString());
                browserUI.UpdateNavigationButtons();
            }
        }

        public void SetTabButton(TabButton button)
        {
            currentTabButton = button;
        }

        public void Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                url = "https://ninechan.github.io/9search/";

            url = url.Trim();

            if (!url.StartsWith("http") && !url.StartsWith("https") &&
                !url.StartsWith("9browser://") && !url.StartsWith("chrome://") &&
                !url.StartsWith("about:") && !url.StartsWith("file:"))
            {
                if (url.Contains(".") && Regex.IsMatch(url, @"^[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}"))
                    url = "https://" + url;
                else
                    url = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
            }

            try
            {
                WebView.CoreWebView2?.Navigate(url);
            }
            catch
            {
                WebView.CoreWebView2?.Navigate("https://ninechan.github.io/9search/");
            }
        }

        public void GoBack() { try { WebView.CoreWebView2?.GoBack(); } catch { } }
        public void GoForward() { try { WebView.CoreWebView2?.GoForward(); } catch { } }
        public void Refresh() { try { WebView.CoreWebView2?.Reload(); } catch { } }
        public void Stop() { try { WebView.CoreWebView2?.Stop(); } catch { } }
        public void GoHome() { try { WebView.CoreWebView2?.Navigate("https://ninechan.github.io/9search/"); } catch { } }

        public bool CanGoBack { get { try { return WebView.CoreWebView2?.CanGoBack ?? false; } catch { return false; } } }
        public bool CanGoForward { get { try { return WebView.CoreWebView2?.CanGoForward ?? false; } catch { return false; } } }
    }
}