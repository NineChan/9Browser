using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace _9Browser
{
    public class ExtensionManager
    {
        private CoreWebView2 webView;
        private List<Extension> extensions = new List<Extension>();
        private string extensionsPath;
        private HttpListener httpListener;
        private int httpPort = 8080;
        private bool isServerRunning = false;

        public ExtensionManager()
        {
            extensionsPath = Path.Combine(Application.StartupPath, "Extensions");
            Directory.CreateDirectory(extensionsPath);
            StartHttpServer();
        }

        private void StartHttpServer()
        {
            try
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://localhost:{httpPort}/Extensions/");
                httpListener.Start();
                isServerRunning = true;

                Thread serverThread = new Thread(() =>
                {
                    while (isServerRunning)
                    {
                        try
                        {
                            var context = httpListener.GetContext();
                            ProcessRequest(context);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"HTTP Server error: {ex.Message}");
                        }
                    }
                });
                serverThread.IsBackground = true;
                serverThread.Start();

                System.Diagnostics.Debug.WriteLine($"HTTP Server started on port {httpPort}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start HTTP server: {ex.Message}");
                isServerRunning = false;
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string path = context.Request.Url.AbsolutePath;
                string filePath = Path.Combine(Application.StartupPath, path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));

                if (File.Exists(filePath))
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    context.Response.ContentType = GetContentType(filePath);
                    context.Response.ContentLength64 = fileBytes.Length;
                    context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                    context.Response.StatusCode = 200;
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
                context.Response.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessRequest error: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        }

        [Obsolete("Сломанная функция")]
        private string GetContentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext = "broken_function";
            /* return ext switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif"
            }; */
        }

        public void LoadExtensions(CoreWebView2 webView)
        {
            this.webView = webView;

            foreach (var dir in Directory.GetDirectories(extensionsPath))
            {
                var manifestPath = Path.Combine(dir, "manifest.json");
                if (File.Exists(manifestPath))
                {
                    try
                    {
                        var manifestJson = File.ReadAllText(manifestPath);
                        var manifest = JsonSerializer.Deserialize<ManifestV3>(manifestJson);

                        if (manifest == null) continue;

                        var extension = new Extension
                        {
                            Id = Path.GetFileName(dir),
                            Name = manifest.Name ?? "Unknown",
                            Version = manifest.Version ?? "1.0",
                            Path = dir,
                            IsEnabled = true,
                            ManifestVersion = manifest.ManifestVersion
                        };

                        extensions.Add(extension);

                        // Создаем скрипт для эмуляции chrome.runtime API
                        string initScript = $@"
                            (function() {{
                                if (typeof window.chrome === 'undefined') window.chrome = {{}};
                                if (typeof window.chrome.runtime === 'undefined') window.chrome.runtime = {{}};
                                
                                window.chrome.runtime.id = '{extension.Id}';
                                window.chrome.runtime.getURL = function(path) {{
                                    return 'http://localhost:{httpPort}/Extensions/{extension.Id}/' + path;
                                }};
                                window.chrome.runtime.getManifest = function() {{
                                    return {{
                                        name: '{manifest.Name}',
                                        version: '{manifest.Version}',
                                        manifest_version: {manifest.ManifestVersion}
                                    }};
                                }};
                                
                                // Эмуляция chrome.storage
                                if (typeof window.chrome.storage === 'undefined') {{
                                    window.chrome.storage = {{}};
                                    window.chrome.storage.local = {{
                                        data: {{}},
                                        get: function(keys, callback) {{
                                            var result = {{}};
                                            if (typeof keys === 'string') {{
                                                if (this.data[keys]) result[keys] = this.data[keys];
                                            }} else if (Array.isArray(keys)) {{
                                                keys.forEach(key => {{
                                                    if (this.data[key]) result[key] = this.data[key];
                                                }});
                                            }} else if (typeof keys === 'object') {{
                                                Object.keys(keys).forEach(key => {{
                                                    result[key] = this.data[key] || keys[key];
                                                }});
                                            }}
                                            if (callback) callback(result);
                                        }},
                                        set: function(items, callback) {{
                                            Object.assign(this.data, items);
                                            if (callback) callback();
                                        }}
                                    }};
                                }}
                            }})();
                        ";
                        webView.AddScriptToExecuteOnDocumentCreatedAsync(initScript);

                        // Загружаем content scripts
                        if (manifest.ContentScripts != null)
                        {
                            foreach (var contentScript in manifest.ContentScripts)
                            {
                                if (contentScript.Js != null)
                                {
                                    foreach (var jsFile in contentScript.Js)
                                    {
                                        var jsPath = Path.Combine(dir, jsFile);
                                        if (File.Exists(jsPath))
                                        {
                                            var jsContent = File.ReadAllText(jsPath);
                                            webView.AddScriptToExecuteOnDocumentCreatedAsync(jsContent);
                                            System.Diagnostics.Debug.WriteLine($"Loaded JS: {jsFile}");
                                        }
                                    }
                                }

                                if (contentScript.Css != null)
                                {
                                    foreach (var cssFile in contentScript.Css)
                                    {
                                        var cssPath = Path.Combine(dir, cssFile);
                                        if (File.Exists(cssPath))
                                        {
                                            var cssContent = File.ReadAllText(cssPath);
                                            var script = $"var style = document.createElement('style'); style.textContent = `{cssContent}`; document.head.appendChild(style);";
                                            webView.AddScriptToExecuteOnDocumentCreatedAsync(script);
                                            System.Diagnostics.Debug.WriteLine($"Loaded CSS: {cssFile}");
                                        }
                                    }
                                }
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Loaded extension: {manifest.Name} v{manifest.Version}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load extension {dir}: {ex.Message}");
                    }
                }
            }
        }

        public string GetExtensionsList()
        {
            var list = extensions.Select(e => new
            {
                name = e.Name,
                version = e.Version,
                enabled = e.IsEnabled,
                description = ""
            }).ToList();

            return $"{{\"type\":\"extensions\",\"extensions\":{JsonSerializer.Serialize(list)}}}";
        }

        public void ShowExtensionsDialog()
        {
            var form = new Form
            {
                Text = "Управление расширениями",
                Size = new System.Drawing.Size(600, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };

            listView.Columns.Add("Название", 250);
            listView.Columns.Add("Версия", 100);
            listView.Columns.Add("Статус", 100);
            listView.Columns.Add("ID", 150);

            foreach (var ext in extensions)
            {
                var item = new ListViewItem(ext.Name);
                item.SubItems.Add(ext.Version);
                item.SubItems.Add(ext.IsEnabled ? "Включено" : "Отключено");
                item.SubItems.Add(ext.Id.Substring(0, Math.Min(8, ext.Id.Length)) + "...");
                item.Checked = ext.IsEnabled;
                item.Tag = ext;
                listView.Items.Add(item);
            }

            var panel = new Panel { Dock = DockStyle.Bottom, Height = 80 };

            var btnInstall = new Button
            {
                Text = "Установить расширение",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(150, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(66, 133, 244),
                ForeColor = System.Drawing.Color.White
            };

            var btnRemove = new Button
            {
                Text = "Удалить",
                Location = new System.Drawing.Point(170, 10),
                Size = new System.Drawing.Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69),
                ForeColor = System.Drawing.Color.White
            };

            var btnClose = new Button
            {
                Text = "Закрыть",
                Location = new System.Drawing.Point(280, 10),
                Size = new System.Drawing.Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(108, 117, 125),
                ForeColor = System.Drawing.Color.White
            };

            btnInstall.Click += (s, e) => InstallExtension();
            btnRemove.Click += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0)
                {
                    var result = MessageBox.Show("Удалить выбранное расширение?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        var ext = (Extension)listView.SelectedItems[0].Tag;
                        RemoveExtension(ext);
                        listView.Items.Remove(listView.SelectedItems[0]);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите расширение для удаления", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnClose.Click += (s, e) => form.Close();

            listView.ItemChecked += (s, e) =>
            {
                var ext = (Extension)e.Item.Tag;
                ext.IsEnabled = e.Item.Checked;
                e.Item.SubItems[2].Text = ext.IsEnabled ? "Включено" : "Отключено";
            };

            panel.Controls.Add(btnInstall);
            panel.Controls.Add(btnRemove);
            panel.Controls.Add(btnClose);

            form.Controls.Add(listView);
            form.Controls.Add(panel);
            form.ShowDialog();
        }

        private void InstallExtension()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку с расширением (содержит manifest.json)";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var manifestPath = Path.Combine(dialog.SelectedPath, "manifest.json");
                    if (File.Exists(manifestPath))
                    {
                        try
                        {
                            var manifestJson = File.ReadAllText(manifestPath);
                            var manifest = JsonSerializer.Deserialize<ManifestV3>(manifestJson);

                            if (manifest == null || string.IsNullOrEmpty(manifest.Name) || string.IsNullOrEmpty(manifest.Version))
                            {
                                MessageBox.Show("Неверный manifest.json: отсутствуют Name или Version",
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            var extId = Guid.NewGuid().ToString();
                            var targetPath = Path.Combine(extensionsPath, extId);
                            CopyDirectory(dialog.SelectedPath, targetPath);

                            var extension = new Extension
                            {
                                Id = extId,
                                Name = manifest.Name,
                                Version = manifest.Version,
                                Path = targetPath,
                                IsEnabled = true,
                                ManifestVersion = manifest.ManifestVersion
                            };
                            extensions.Add(extension);

                            MessageBox.Show($"Расширение \"{manifest.Name}\" успешно установлено!\nПерезапустите браузер для активации.",
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка установки: {ex.Message}",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не найден manifest.json в выбранной папке.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RemoveExtension(Extension extension)
        {
            try
            {
                if (Directory.Exists(extension.Path))
                {
                    Directory.Delete(extension.Path, true);
                }
                extensions.Remove(extension);
                MessageBox.Show($"Расширение \"{extension.Name}\" удалено.",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);

            foreach (var file in Directory.GetFiles(source))
            {
                var destFile = Path.Combine(target, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                var destDir = Path.Combine(target, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

        public void StopHttpServer()
        {
            isServerRunning = false;
            httpListener?.Stop();
            httpListener?.Close();
        }

        private class ManifestV3
        {
            [System.Text.Json.Serialization.JsonPropertyName("manifest_version")]
            public int ManifestVersion { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Description { get; set; }
            public List<string> Permissions { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("content_scripts")]
            public List<ContentScript> ContentScripts { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("web_accessible_resources")]
            public List<WebAccessibleResource> WebAccessibleResources { get; set; }
        }

        private class ContentScript
        {
            public List<string> Matches { get; set; }
            public List<string> Js { get; set; }
            public List<string> Css { get; set; }
        }

        private class WebAccessibleResource
        {
            public List<string> Resources { get; set; }
            public List<string> Matches { get; set; }
        }

        private class Extension
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Path { get; set; }
            public bool IsEnabled { get; set; }
            public int ManifestVersion { get; set; }
        }
    }
}