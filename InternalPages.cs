using System;
using System.Collections.Generic;
using System.Text;

namespace _9Browser
{
    public static class InternalPages
    {
        public static Dictionary<string, string> Pages = new Dictionary<string, string>
        {
            { "about", GetAboutPage() },
            { "version", GetVersionPage() },
            { "extensions", GetExtensionsPage() },
            { "downloads", GetDownloadsPage() },
            { "settings", GetSettingsPage() },
            { "history", GetHistoryPage() },
            { "help", GetHelpPage() },
            { "chrome-urls", GetChromeUrlsPage() },
            { "browser-urls", GetBrowserUrlsPage() },
            { "flags", GetFlagsPage() },
            { "dns", GetDnsPage() },
            { "cache", GetCachePage() }
        };

        public static string GetAboutPage()
        {
            return @"<html>
<head><title>9browser://about</title></head>
<body>
<h1>9Browser</h1>
<p>Version: 1.0.0</p>
<p>Built with WebView2</p>
<p>Fast, Secure, Modern Browser</p>
</body>
</html>";
        }

        public static string GetVersionPage()
        {
            return @"<html>
<head><title>9browser://version</title></head>
<body>
<h1>Version Information</h1>
<p><b>Browser:</b> 9Browser</p>
<p><b>Version:</b> 1.0.0.0</p>
<p><b>Engine:</b> WebView2 (Chromium based)</p>
</body>
</html>";
        }

        public static string GetExtensionsPage()
        {
            return @"<html>
<head><title>9browser://extensions</title></head>
<body>
<h1>Extensions</h1>
<div id='extensions-list'>Loading extensions...</div>
<button onclick='window.chrome.webview.postMessage(""get-extensions"")'>Refresh</button>
<button onclick='window.chrome.webview.postMessage(""manage-extensions"")'>Manage Extensions</button>
<script>
    window.chrome.webview.addEventListener('message', function(e) {
        if(e.data.type === 'extensions') {
            var list = document.getElementById('extensions-list');
            if(e.data.extensions.length === 0) {
                list.innerHTML = '<p>No extensions installed.</p>';
            } else {
                var html = '<table border=1 cellpadding=5>';
                html += '<tr><th>Name</th><th>Version</th><th>Status</th></tr>';
                e.data.extensions.forEach(function(ext) {
                    html += '<tr>';
                    html += '<td>' + ext.name + '</td>';
                    html += '<td>' + ext.version + '</td>';
                    html += '<td>' + (ext.enabled ? 'Enabled' : 'Disabled') + '</td>';
                    html += '</tr>';
                });
                html += '</table>';
                list.innerHTML = html;
            }
        }
    });
</script>
</body>
</html>";
        }

        public static string GetDownloadsPage()
        {
            return @"<html>
<head><title>9browser://downloads</title></head>
<body>
<h1>Downloads</h1>
<div id='downloads-list'>No active downloads</div>
<button onclick='window.chrome.webview.postMessage(""open-downloads-folder"")'>Open Downloads Folder</button>
</body>
</html>";
        }

        public static string GetSettingsPage()
        {
            return @"<html>
<head><title>9browser://settings</title></head>
<body>
<h1>Settings</h1>
<p><b>Homepage:</b> <input type='text' id='homepage' size='50' /> <button onclick='saveHomepage()'>Save</button></p>
<p><b>Search Engine:</b> 
<select id='searchEngine'>
    <option value='google'>Google</option>
    <option value='bing'>Bing</option>
    <option value='duckduckgo'>DuckDuckGo</option>
</select>
<button onclick='saveSearchEngine()'>Save</button>
</p>
<script>
    window.chrome.webview.postMessage('get-settings');
    window.chrome.webview.addEventListener('message', function(e) {
        if(e.data.type === 'settings') {
            document.getElementById('homepage').value = e.data.homepage;
            document.getElementById('searchEngine').value = e.data.searchEngine;
        }
    });
    function saveHomepage() {
        window.chrome.webview.postMessage({
            type: 'save-homepage',
            value: document.getElementById('homepage').value
        });
    }
    function saveSearchEngine() {
        window.chrome.webview.postMessage({
            type: 'save-search-engine',
            value: document.getElementById('searchEngine').value
        });
    }
</script>
</body>
</html>";
        }

        public static string GetHistoryPage()
        {
            return @"<html>
<head><title>9browser://history</title></head>
<body>
<h1>History</h1>
<div id='history-list'>Loading history...</div>
<button onclick='window.chrome.webview.postMessage(""clear-history"")'>Clear History</button>
<button onclick='window.chrome.webview.postMessage(""get-history"")'>Refresh</button>
<script>
    window.chrome.webview.postMessage('get-history');
    window.chrome.webview.addEventListener('message', function(e) {
        if(e.data.type === 'history') {
            var list = document.getElementById('history-list');
            if(e.data.history.length === 0) {
                list.innerHTML = '<p>No browsing history.</p>';
            } else {
                var html = '<ul>';
                e.data.history.forEach(function(item) {
                    html += '<li><a href=\'' + item.url + '\'>' + item.title + '</a> - ' + item.date + '</li>';
                });
                html += '</ul>';
                list.innerHTML = html;
            }
        }
    });
</script>
</body>
</html>";
        }

        public static string GetHelpPage()
        {
            return @"<html>
<head><title>9browser://help</title></head>
<body>
<h1>Help</h1>
<p><b>Navigation:</b></p>
<ul>
<li>Back: Alt + Left Arrow</li>
<li>Forward: Alt + Right Arrow</li>
<li>Refresh: F5 or Ctrl + R</li>
<li>Home: Alt + Home</li>
</ul>
<p><b>Tabs:</b></p>
<ul>
<li>New Tab: Ctrl + T</li>
<li>Close Tab: Ctrl + W</li>
<li>Next Tab: Ctrl + Tab</li>
</ul>
<p><b>Browser URLs:</b></p>
<ul>
<li>9browser://about - About Browser</li>
<li>9browser://version - Version Info</li>
<li>9browser://extensions - Extensions Manager</li>
<li>9browser://downloads - Downloads</li>
<li>9browser://settings - Settings</li>
<li>9browser://history - History</li>
<li>9browser://chrome-urls - Chrome URLs</li>
<li>9browser://browser-urls - Browser URLs</li>
<li>9browser://flags - Experimental Features</li>
<li>9browser://dns - DNS Information</li>
<li>9browser://cache - Cache Info</li>
</ul>
</body>
</html>";
        }

        public static string GetChromeUrlsPage()
        {
            return @"<html>
<head><title>9browser://chrome-urls</title></head>
<body>
<h1>Chrome URLs</h1>
<p>List of Chrome internal pages:</p>
<ul>
<li><a href='chrome://version'>chrome://version</a></li>
<li><a href='chrome://settings'>chrome://settings</a></li>
<li><a href='chrome://extensions'>chrome://extensions</a></li>
<li><a href='chrome://history'>chrome://history</a></li>
<li><a href='chrome://downloads'>chrome://downloads</a></li>
<li><a href='chrome://flags'>chrome://flags</a></li>
<li><a href='chrome://dns'>chrome://dns</a></li>
<li><a href='chrome://cache'>chrome://cache</a></li>
<li><a href='chrome://gpu'>chrome://gpu</a></li>
<li><a href='chrome://media-internals'>chrome://media-internals</a></li>
</ul>
</body>
</html>";
        }

        public static string GetBrowserUrlsPage()
        {
            return @"<html>
<head><title>9browser://browser-urls</title></head>
<body>
<h1>9Browser URLs</h1>
<p>List of 9Browser internal pages:</p>
<ul>
<li><a href='9browser://about'>9browser://about</a> - About Browser</li>
<li><a href='9browser://version'>9browser://version</a> - Version Information</li>
<li><a href='9browser://extensions'>9browser://extensions</a> - Extensions Manager</li>
<li><a href='9browser://downloads'>9browser://downloads</a> - Downloads</li>
<li><a href='9browser://settings'>9browser://settings</a> - Browser Settings</li>
<li><a href='9browser://history'>9browser://history</a> - Browsing History</li>
<li><a href='9browser://help'>9browser://help</a> - Help</li>
<li><a href='9browser://flags'>9browser://flags</a> - Experimental Features</li>
<li><a href='9browser://dns'>9browser://dns</a> - DNS Information</li>
<li><a href='9browser://cache'>9browser://cache</a> - Cache Information</li>
</ul>
</body>
</html>";
        }

        public static string GetFlagsPage()
        {
            return @"<html>
<head><title>9browser://flags</title></head>
<body>
<h1>Experimental Features</h1>
<p><b>Warning:</b> These features may be unstable.</p>
<p><input type='checkbox' id='smooth-scrolling' /> Smooth Scrolling</p>
<p><input type='checkbox' id='dark-mode' /> Force Dark Mode</p>
<p><button onclick='applyFlags()'>Apply Changes</button> <button onclick='resetFlags()'>Reset All</button></p>
<script>
    window.chrome.webview.postMessage('get-flags');
    window.chrome.webview.addEventListener('message', function(e) {
        if(e.data.type === 'flags') {
            document.getElementById('smooth-scrolling').checked = e.data.smoothScrolling;
            document.getElementById('dark-mode').checked = e.data.darkMode;
        }
    });
    function applyFlags() {
        window.chrome.webview.postMessage({
            type: 'apply-flags',
            smoothScrolling: document.getElementById('smooth-scrolling').checked,
            darkMode: document.getElementById('dark-mode').checked
        });
    }
    function resetFlags() {
        window.chrome.webview.postMessage({ type: 'reset-flags' });
    }
</script>
</body>
</html>";
        }

        public static string GetDnsPage()
        {
            return @"<html>
<head><title>9browser://dns</title></head>
<body>
<h1>DNS Information</h1>
<div id='dns-info'>Loading...</div>
<button onclick='window.chrome.webview.postMessage(""clear-dns-cache"")'>Clear DNS Cache</button>
<button onclick='window.chrome.webview.postMessage(""get-dns-info"")'>Refresh</button>
<script>
    window.chrome.webview.postMessage('get-dns-info');
    window.chrome.webview.addEventListener('message', function(e) {
        if(e.data.type === 'dns-info') {
            var info = document.getElementById('dns-info');
            var html = '<table border=1 cellpadding=5>';
            html += '<tr><th>Host</th><th>IP Address</th><th>TTL</th></tr>';
            e.data.entries.forEach(function(entry) {
                html += '<tr><td>' + entry.host + '</td><td>' + entry.ip + '</td><td>' + entry.ttl + 's</td></tr>';
            });
            html += '</table>';
            info.innerHTML = html;
        }
    });
</script>
</body>
</html>";
        }

        public static string GetCachePage()
        {
            return @"<html>
<head><title>9browser://cache</title></head>
<body>
<h1>Cache Information</h1>
<div id='cache-info'>Loading...</div>
<button onclick='window.chrome.webview.postMessage(""clear-cache"")'>Clear Cache</button>
<button onclick='window.chrome.webview.postMessage(""get-cache-info"")'>Refresh</button>
<script>
    window.chrome.webview.postMessage('get-cache-info');
    window.chrome.webview.addEventListener('message', function(e) {
        if(e.data.type === 'cache-info') {
            var info = document.getElementById('cache-info');
            info.innerHTML = '<p><b>Cache Size:</b> ' + e.data.size + '</p>' +
                            '<p><b>Items in Cache:</b> ' + e.data.items + '</p>';
        }
    });
</script>
</body>
</html>";
        }

        public static bool IsInternalUrl(string url)
        {
            return url != null && (url.StartsWith("9browser://") || url.StartsWith("chrome://"));
        }

        public static string GetInternalPage(string url)
        {
            var pageName = url.Replace("9browser://", "").Replace("chrome://", "").Replace("/", "").ToLower();

            if (Pages.ContainsKey(pageName))
                return Pages[pageName];

            return @"<html><head><title>Page Not Found</title></head><body>" +
                   "<h1>404 - Page Not Found</h1>" +
                   "<p>The page " + url + " does not exist.</p>" +
                   "<p>Available pages: <a href='9browser://browser-urls'>9browser://browser-urls</a></p>" +
                   "</body></html>";
        }
    }
}