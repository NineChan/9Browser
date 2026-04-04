# 9Browser

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET Framework](https://img.shields.io/badge/.NET-4.7.2%2B-green.svg)](https://dotnet.microsoft.com/)
[![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6.svg)](https://www.microsoft.com/windows)
[![WebView2](https://img.shields.io/badge/WebView2-Chromium-4285F4.svg)](https://developer.microsoft.com/ru-ru/microsoft-edge/webview2/)

**Современный браузер на основе Microsoft WebView2 с поддержкой расширений, вкладок и закладок.**

---

## Возможности

| Функция | Описание |
|---------|----------|
| 📑 Много вкладок | Открывайте несколько сайтов одновременно |
| 🔖 Закладки | Сохраняйте любимые сайты |
| 📜 История | Просматривайте историю посещений |
| ⬇ Загрузки | Встроенный менеджер с прогресс-баром |
| 🧩 Расширения | Пока что сломаны :( |
| 🖼 Favicon | Иконки сайтов на вкладках |
| ⚙ Настройки | Персональные настройки браузера |

---

## Внутренние страницы

Доступны по адресу `9browser://`

| Страница | Описание |
|----------|----------|
| `9browser://about` | О браузере |
| `9browser://version` | Версия |
| `9browser://extensions` | Управление расширениями |
| `9browser://downloads` | Список загрузок |
| `9browser://settings` | Настройки |
| `9browser://history` | История посещений |
| `9browser://help` | Справка |
| `9browser://browser-urls` | Все внутренние страницы |
| `9browser://flags` | Экспериментальные функции |

---

## Горячие клавиши

| Комбинация | Действие |
|------------|----------|
| `Alt + ←` | Назад |
| `Alt + →` | Вперед |
| `F5` / `Ctrl + R` | Обновить |
| `Esc` | Остановить |
| `Alt + Home` | Домашняя страница |
| `Ctrl + T` | Новая вкладка |
| `Ctrl + W` | Закрыть вкладку |
| `Ctrl + Tab` | Следующая вкладка |
| `Ctrl + D` | Добавить закладку |
| `Ctrl + H` | История |
| `Ctrl + J` | Загрузки |

---

## Системные требования

- **ОС**: Windows 10 / 11 (64-bit)
- **.NET Framework**: 4.7.2 или выше
- **WebView2**: Устанавливается автоматически
- **ОЗУ**: 512 MB (рекомендуется 2 GB)
- **Место на диске**: 200 MB

---

## Установка

### Способ 1: Установщик

1. Скачайте `9Browser Installer.exe` из раздела [Releases](https://github.com/NineChan/9Browser/releases)
2. Запустите установщик
3. Следуйте инструкциям

### Способ 3: Сборка из исходников

```
git clone https://github.com/NineChan/9Browser.git
Откройте проект в Visual Studio
Build → Rebuild Solution
```

---

## Расширения
 
Пока что они сломаны и в будущем (может быть) они будут работать.

---

## Технологии

- **Microsoft WebView2** — движок браузера (Chromium)
- **.NET Framework 4.7.2** — платформа разработки
- **MetroFramework** — современный UI
- **C# WinForms** — интерфейс пользователя
- **System.Text.Json** — работа с JSON

---

## Структура проекта

```
9Browser/
├── Form1.cs                 # Главная форма
├── TabManager.cs            # Управление вкладками
├── BrowserCore.cs           # Ядро браузера (WebView2)
├── BrowserUI.cs             # Пользовательский интерфейс
├── BookmarkManager.cs       # Управление закладками
├── DownloadManager.cs       # Управление загрузками
├── ExtensionManager.cs      # Управление расширениями
├── InternalPages.cs         # Внутренние страницы
└── Program.cs               # Точка входа
```

---

## Лицензия

**MIT License**

Copyright (c) 2026 9Browser

Данное программное обеспечение предоставляется бесплатно. Разрешается использовать, копировать, изменять, объединять, публиковать, распространять, сублицензировать и/или продавать копии программного обеспечения.

ПРОГРАММА ПРЕДОСТАВЛЯЕТСЯ "КАК ЕСТЬ", БЕЗ КАКИХ-ЛИБО ГАРАНТИЙ.

---

<div align="center">
  <sub>Built with ❤️ for a better browsing experience</sub>
</div>
