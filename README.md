# Selenium UI Tests (C#)

Тесты покрывают базовые пользовательские сценарии:

* авторизация
* навигация по меню
* работа поиска
* выход из аккаунта
* открытие модального окна "Журнал изменений"

## ⚙️ Требования

Перед запуском убедитесь, что у вас установлено:

* .NET SDK (версия 8 или выше)
* Google Chrome

## 📦 Установка зависимостей

В корне проекта выполните:

```bash
dotnet restore
```

Если нужно установить пакеты вручную:

```bash
dotnet add package Selenium.WebDriver
dotnet add package Selenium.WebDriver.ChromeDriver
dotnet add package Selenium.Support
dotnet add package SeleniumExtras.WaitHelpers
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package DotNetEnv
```

---

## 🔐 Настройка переменных окружения (для локального запуска)

Для авторизации используется файл `.env`.

Создайте файл:

```
SeleniumTests/bin/Debug/netX.X/.env
```

И добавьте:

```env
TEST_LOGIN=your_login
TEST_PASSWORD=your_password
```


## 🚀 Запуск тестов

Выполните команду:

```bash
dotnet test
```
