using RELOD_Tools.WebSearch;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using OpenQA.Selenium.Support.UI;
using RELOD_Tools.Logic;
using OpenQA.Selenium.Chrome;

namespace RELOD_Tools.WebParsing.WebSearch.Site
{
    class PubEasy : SiteSearchModel
    {
        ChromeDriver cd = new ChromeDriver();
        public PubEasy(string[] isbns)
        {
            string loginPage    = "https://beta.pubeasy.com/static/pubeasy/index.html";
            string login        = "Логин";
            string username     = "Имя пользователя";
            string password     = "Пароль";
            bool notFound       = false;

            // Настраиваем Progress Bar
            PB.Show();
            int isbnsLength         = isbns.Length;
            PB.progressBar.Minimum  = 0;
            PB.progressBar.Maximum  = isbnsLength;
            PB.progressBar.Value    = 0;
            double progressvalue    = 1;

            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(PB.progressBar.SetValue);
            Application.Current.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, progressvalue });
            //==================================================================================================

            WebDriverWait wait5 = new WebDriverWait(cd, TimeSpan.FromSeconds(5));
            WebDriverWait wait10 = new WebDriverWait(cd, TimeSpan.FromSeconds(10));
            WebDriverWait wait16 = new WebDriverWait(cd, TimeSpan.FromSeconds(16));

            try
            {
                cd.Url = loginPage;
                IWebElement element;

                element = cd.FindElement(By.XPath("//li/a[contains(@href, 'login')]"));
                element.Click();

                // КОД для логина и пароля
                element = cd.FindElement(By.Id("login-id"));
                element.SendKeys(login);
                element = cd.FindElement(By.Id("user-id"));
                element.SendKeys(username);
                element = cd.FindElement(By.Id("login-password"));
                element.SendKeys(password);

                element = cd.FindElement(By.XPath("//input[contains(@type, 'submit')]"));
                element.Click();
                element = cd.FindElement(By.XPath("//a[contains(text(), 'SEARCH NOW >')]"));
                element.Click();

                try
                {
                    for (int i = 0; i < isbns.Length; i++)
                    {
                        // Присваиваем порядковый номер
                        number = (i + 1).ToString();

                        // Присваиваем ISBN
                        isbn = isbns[i].Replace("\n", "");
                        isbn = isbn.Replace("\r", "");

                        // Передаем данные в Progress Bar для увеличения шкалы и обновления UI
                        PB.Title = $"Поиск по сайту PubEasy. Обработано {i + 1} из {isbnsLength}";
                        PB.progressBar.Value++;
                        progressvalue++;
                        Application.Current.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background,
                        new object[] { System.Windows.Controls.ProgressBar.ValueProperty, progressvalue });
                        //==================================================================================================


                        if (isbn != string.Empty)
                        {
                            // Переключаем драйвер на первую вкладку браузера
                            cd.SwitchTo().Window(cd.WindowHandles.First());

                            element = cd.FindElement(By.XPath("//*[@id = 'search']/input"));
                            element.Clear();
                            element.SendKeys(isbn);
                            element.SendKeys(Keys.Enter);

                            try
                            {
                                // КОД для определения нашлась ли искомая позиция
                                element = cd.FindElement(By.XPath("//strong[contains(text(), 'No records found')]"));
                                notFound = true;
                                cd.Navigate().Back();
                            }
                            catch { }

                            if (notFound != true)
                            {
                                try // Поиск наименования
                                {
                                    element = cd.FindElement(By.XPath("//tr[@class = 'odd']//a[2]/font"));
                                    title = element.Text;
                                    title = title.Replace("\n", " ");
                                    title = title.Replace("\r", " ");
                                }
                                catch { }

                                try // Поиск автора
                                {
                                    element = cd.FindElement(By.XPath("//tr[@class = 'odd']/td[2]"));
                                    author = element.Text;
                                }
                                catch { }

                                try // Поиск даты издания
                                {
                                    element = cd.FindElement(By.XPath("//tr[@class = 'odd']/td[6]"));
                                    pubDate = element.Text;
                                    pubDate = pubDate.Replace("\n", " ");
                                    pubDate = pubDate.Replace("\r", " ");
                                }
                                catch { }

                                try // Поиск издательства
                                {
                                    element = cd.FindElement(By.XPath("//tr[@class = 'odd']/td[4]"));
                                    publisher = element.Text;
                                }
                                catch { }

                                try // Поиск поставщика
                                {
                                    element = cd.FindElement(By.XPath("//tr[@class = 'odd']/td[5]"));
                                    supplier = element.Text;
                                }
                                catch { }

                                try // Поиск цены
                                {
                                    element = cd.FindElement(By.XPath("//tr[@class = 'odd']/td[7]"));
                                    priceWithCurrency = element.Text;
                                    priceWithCurrency = priceWithCurrency.Replace("\r", " ");
                                    priceWithCurrency = priceWithCurrency.Replace("\n", " ");
                                    priceWithCurrency = priceWithCurrency.Replace(" (Retail)", "");
                                    priceWithCurrency = priceWithCurrency.Replace(" (List)", "");
                                    price = priceWithCurrency.Replace(" GBP", "");
                                    price = price.Replace(".", ",");
                                }
                                catch { }

                                try // Поиск доступности
                                {
                                    element = cd.FindElement(By.XPath("//tr[@class = 'odd']/td[8]"));
                                    availability = element.Text;
                                    availability = availability.Replace("\n", " ");
                                    availability = availability.Replace("\r", " ");
                                }
                                catch { }

                                try // Поиск обложки
                                {
                                    element = cd.FindElement(By.XPath("//tr[@class = 'odd']/td[3]"));
                                    bookCover = element.Text;
                                }
                                catch { }

                                if (supplier.StartsWith("bertram", StringComparison.OrdinalIgnoreCase) |
                                    supplier.StartsWith("gardners", StringComparison.OrdinalIgnoreCase))
                                {
                                    ClearBookList();
                                }

                                try
                                {
                                    // Пытаемся перейти по ссылке Availability ==========
                                    element = cd.FindElement(By.XPath("//*[@class = 'odd']/td[8]/a"));
                                    element.Click();
                                    // ==================================================

                                    try
                                    {
                                        cd.SwitchTo().Window(cd.WindowHandles.Last());

                                        element = cd.FindElement(By.XPath("//iframe[@class = 'embed-responsive-item']"));
                                        cd.SwitchTo().Frame(element);

                                        wait16.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath("//*[@class = 'table-responsive']/table/tbody/tr[6]/td[2]")));

                                        //Меняем наименование ===============================
                                        title = cd.FindElement(By.XPath("//*[@class = 'table-responsive']/table/tbody/tr/td[2]")).Text;
                                        // ==================================================

                                        //Проверка соответсвия цены =========================

                                        try
                                        {
                                            element = cd.FindElement(By.XPath("//tr[@valign = 'TOP' and .//td[contains(text(), 'Price')]]/td[2]"));
                                            priceComparision = element.Text;
                                            priceComparision = priceComparision.Replace("\r\n", " ");
                                            priceComparision = priceComparision.Replace(" (Retail)", "");
                                            priceComparision = priceComparision.Replace("(Retail)", "");
                                            priceComparision = priceComparision.Replace(" (British Pounds)", "");
                                            priceComparision = priceComparision.Replace("(British Pounds)", "");
                                            priceComparision = priceComparision.Replace(" (List)", "");
                                            priceComparision = priceComparision.Replace("(List)", "");
                                            priceComparision = priceComparision.Replace(" GBP", "");
                                            priceComparision = priceComparision.Replace(".", ",");
                                            priceComparision = priceComparision.Replace("Not applicable", "");

                                            if (price == priceComparision)
                                            {
                                                priceComparision = string.Empty;
                                            }

                                        }
                                        catch { }
                                        // ==================================================

                                        //Поиск скидки ==========================================
                                        try
                                        {
                                            element = cd.FindElement(By.XPath("//tr[@valign = 'TOP' and .//td[contains(text(), 'Base Discount')]]/td[2]"));
                                            discount = element.Text;
                                            discount = discount.Replace("%", "");
                                            discount = discount.Replace(".", ",");
                                        }
                                        catch { }
                                        // ==================================================

                                        // Market Restrictions ==============================
                                        try
                                        {
                                            element = cd.FindElement(By.XPath("//tr[@valign = 'TOP' and .//td[contains(text(), 'Market Restrictions')]]/td[2]"));
                                            marketRestrictions = element.Text;
                                        }
                                        catch { }
                                        // ==================================================

                                        // Availability со второй страницы ==================
                                        try
                                        {
                                            element = cd.FindElement(By.XPath("//tr[@valign = 'TOP' and .//td[contains(text(), 'Availability')]]/td[2]"));
                                            availability2 = element.Text;
                                            availability2 = availability2.Replace("\n", "");
                                            availability2 = availability2.Replace("\r", "");
                                        }
                                        catch { }
                                        // ==================================================

                                        cd.Close();
                                    }
                                    catch
                                    {
                                        priceComparision = "Страница не загрузилась.";
                                        cd.Close();
                                    }
                                }
                                catch
                                { }
                            }
                            notFound = false;
                            AddBookToList();
                            ClearBookList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    cd.Quit();
                    PB.Close();
                    Errors.CustomError(ex);
                }
                cd.Quit();
                PB.Close();
                WorkWithFile.SaveFile(book);
            }
            catch (Exception ex)
            {
                cd.Quit();
                PB.Close();
                Errors.LoginPageError(ex);
            }
        }
    }
}
