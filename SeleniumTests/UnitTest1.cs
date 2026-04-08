using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using DotNetEnv;

namespace SeleniumTests;
// 1. Структура теста — есть Setup и Teardown, авторизация вынесена в отдельный метод
// 2. Переиспользование кода — повторяющиеся блоки вынесены в отдельные методы
// 3. Нет лишних UI-действий — например, используем переход по URL вместо клика по кнопкам меню,  если этого не требуется для проверки в рамках теста
// 4. Понятные сообщения в ассертах — при падении теста сразу ясно, что пошло не так
// 5. Человекочитаемые названия тестов — проверяющий понимает, что именно тестируется
// 6. Уникальные локаторы — используются там, где это возможно
// 7. Явные или неявные ожидания — тесты не падают из-за гонки с интерфейсом
public class Tests // объявление класса с тестами
{
    public IWebDriver Driver;
    public WebDriverWait Wait;
    public string BaseURL = "https://staff-testing.testkontur.ru/";
    public string NewsURL = "https://staff-testing.testkontur.ru/news";
    public string CommunitiesURL = "https://staff-testing.testkontur.ru/communities";

    [SetUp]
    public void Setup() // метод подготовки к тесту
    {
        // Загружаем .env файл, если он существует (для локальной разработки) p.s. в ходе проб и ошибок выяснил, что рабочая директория = папка с bin/Debug/netX.X/
        var EnvPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(EnvPath))
            Env.Load(EnvPath);

        Driver = new ChromeDriver();
        Driver.Manage().Window.Size = new System.Drawing.Size(1080, 1000); // задаю размеры окна так, чтобы отображался бургер и поле поиска
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5); // установка неявного ожиания
        Wait = new WebDriverWait (Driver, TimeSpan.FromSeconds(3)); // явное ожидание
    }

    [TearDown]
    public void TearDown() // что происходит после завершения каждого теста
    {
        Driver.Quit();
        Driver.Dispose();
    }

    private void Authorize() // вынесли в отдельный метод авторизацию
    {
        
        Driver.Navigate().GoToUrl(BaseURL); // переход на страницу

        var Login = Environment.GetEnvironmentVariable("TEST_LOGIN");
        var Password = Environment.GetEnvironmentVariable("TEST_PASSWORD");

        var LoginField = Driver.FindElement(By.Id("Username")); // поиск поля логина
        LoginField.SendKeys(Login!); // ввод
        var PasswordField = Driver.FindElement(By.Id("Password")); // поиск поля пароля
        PasswordField.SendKeys(Password!); // ввод

        var Enter = Driver.FindElement(By.Name("button")); // поиск кнопки "войти"
        Enter.Click(); // клик по кнопке

        Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-tid='Title']")));
    }

    private void OpenSidebar() // вынесли в отдельный метод открытие бургера
    {
        var SidebarMenuButton = Driver.FindElement(By.CssSelector("[data-tid='SidebarMenuButton']")); // кнопка открытия меню
        SidebarMenuButton.Click(); // клик по кнопке

        Wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='SidePage__root']"))); // ожидание появления боковой панели
    }

    [Test]
    public void AuthorizationTest()
    {
        Authorize();
        Assert.That(Driver.Title, Does.Contain("Новости"), "Ошибка авторизации"); // проверка, что открыта нужная страница
    }

    [Test]
    public void NavigationMenuItemTest() // тест перехода в меню "Сообщества"
    {
        Authorize();
        OpenSidebar();

        var CommunityElements = Driver.FindElements(By.CssSelector("[data-tid='Community']")) // получение всех элементов "Сообщество"
            .First(element => element.Displayed); // фильтрация отображаемого
        CommunityElements.Click(); // клик по "Сообщества"

        Wait.Until(ExpectedConditions.UrlToBe(CommunitiesURL)); // ожидание перехода на нужную страницу
        var TitlePageElement = Driver.FindElement(By.CssSelector("[data-tid='Title']")); // поиск заголовка

        Assert.That(TitlePageElement.Text, Does.Contain("Сообщества"), "При переходе на вкладку Сообщества мы не смогли найти заголовок Сообщества"); // проверка заголовка
    }

    [Test]

    public void SearchFieldTest() // тест поисковой строки
    {
        Authorize();

        var SearchField = Driver.FindElement(By.CssSelector("[data-tid='SearchBar']")); // поиск строки поиска
        SearchField.Click(); // клик по строке поиска

        var SearchInput = Driver.FindElement(By.CssSelector("[placeholder='Поиск сотрудника, подразделения, сообщества, мероприятия']"));
        SearchInput.SendKeys("Ильиных Сергей Алексеевич"); // ввод

        Assert.That(SearchInput.GetAttribute("value")!.Contains("Ильиных Сергей Алексеевич"),
            "Поле поиска должно содержать введённое значение"); // проверка ввода
    }

    [Test]

    public void LogoutTest() // тест выхода из профиля
    {
        Authorize();

        var ProfileMenuButton = Driver.FindElement(By.CssSelector("[data-tid='DropdownButton']")); // поиск кнопки открытия меню профиля
        ProfileMenuButton.Click(); // клик

        Wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='ScrollContainer__inner']"))); // ожидание появления меню профиля

        var LogoutButton = Driver.FindElement(By.CssSelector("[data-tid='Logout']")); // поиск кнопки выхода
        LogoutButton.Click(); // клик

        var LogoutText = Wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(text(),'Вы вышли из учетной записи')]"))); // дожидаемся появления "прощальной записи"

        Assert.That(LogoutText.Displayed, Is.True, 
            "Текст 'Вы вышли из учетной записи' не отображается на странице после логаута"); // проверка наличия текста
        
    }

    [Test]

    public void ChangelogWindowTest() // тест открытия модального окна с журналом изменений
    {
        Authorize();
        OpenSidebar();

        var VersionButton = Driver.FindElement(By.CssSelector("[data-tid='Version']")); // находим кнопку с текущей версией портала
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("arguments[0].click();", VersionButton); // клик через JS, ибо стандартный click() не захотел срабатывать

        var ChangelogText = Wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(text(),'Журнал изменений')]"))); // дожидаемся появления окна

        Assert.That(ChangelogText.Displayed, Is.True, 
            "Заголовок 'Журнал изменений' не отображается на странице changelog"); // проверка наличия текста
    }
}