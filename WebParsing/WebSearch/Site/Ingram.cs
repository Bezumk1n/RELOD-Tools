using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using RELOD_Tools.Logic;
using RELOD_Tools.WebSearch;
using System;
using System.Windows;
using System.Windows.Threading;

namespace RELOD_Tools.WebParsing.WebSearch.Site
{
    class Ingram : SiteSearchModel
    {
        IWebDriver cd = new ChromeDriver();
        bool AdvancedSearch = false;
        public Ingram(string[] isbns)
        {
            string loginPage = "https://ipage.ingramcontent.com/ipage/li001.jsp";
            string username = "lrelod";
            string password = "jGH4xABv";

            // Настраиваем Progress Bar
            PB.Show();
            int isbnsLength = isbns.Length;
            PB.progressBar.Minimum = 0;
            PB.progressBar.Maximum = isbnsLength;
            PB.progressBar.Value = 0;
            double progressvalue = 1;

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

                // КОД для логина и пароля
                element = cd.FindElement(By.Id("userIDText"));
                element.SendKeys(username);
                element = cd.FindElement(By.Id("passwordText"));
                element.SendKeys(password);

                element = cd.FindElement(By.XPath("//button[@class = 'btn btn-primary btn-block']"));
                element.Click();

                try
                {
                    for (int i = 0; i < isbns.Length; i++)
                    {
                        // Передаем данные в Progress Bar для увеличения шкалы и обновления UI
                        PB.Title = $"Поиск по сайту Ingram. Обработано {i + 1} из {isbnsLength}";
                        PB.progressBar.Value++;
                        progressvalue++;
                        Application.Current.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background,
                        new object[] { System.Windows.Controls.ProgressBar.ValueProperty, progressvalue });
                        //==================================================================================================

                        number = (i + 1).ToString();
                        isbn = isbns[i].Replace("\n", "");
                        isbn = isbn.Replace("\r", "");

                        if (isbns[i] != string.Empty)
                        {
                            element = cd.FindElement(By.XPath("//*[@id = 'searchText']"));
                            element.Clear();
                            element.SendKeys(isbns[i]);
                            element.SendKeys(Keys.Enter);

                            try
                            {
                                // КОД для определения нашлась ли искомая позиция
                                element = cd.FindElement(By.XPath("//div[@class = 'toggleBox, searchCorrect  top-row breadcrum-section']"));
                                AdvancedSearch = true;

                            }
                            catch { }

                            if (AdvancedSearch != true)
                            {
                                try // Поиск наименования
                                {
                                    element = cd.FindElement(By.XPath("//span[@class = 'productDetailTitle']"));
                                    title = element.Text;
                                }
                                catch { }

                                try // Поиск автора
                                {
                                    element = cd.FindElement(By.XPath("//a[@class = 'doContributorSearch']/span"));
                                    author = element.Text;
                                }
                                catch { }

                                try // Поиск даты издания
                                {
                                    element = cd.FindElement(By.XPath("//div[@class = 'productDetailElements' and ./strong[contains(text(), 'Pub Date:')]]"));
                                    pubDate = element.Text;
                                    pubDate = pubDate.Replace("Pub Date: ", "");
                                }
                                catch { }

                                try // Поиск издательства
                                {
                                    element = cd.FindElement(By.XPath("//a[@class = 'doSid']"));
                                    publisher = element.Text;
                                }
                                catch { }

                                try // Поиск цены
                                {
                                    element = cd.FindElement(By.XPath("//div[@class = 'productDetailElements' and ./strong[contains(text(), 'US SRP:')]]"));
                                    priceWithCurrency = element.Text;
                                    priceWithCurrency = priceWithCurrency.Replace("US SRP: $", "");
                                    priceWithCurrency = priceWithCurrency.Remove(priceWithCurrency.IndexOf(" ")) + " USD"; // удалить весть текст после " "
                                    price = priceWithCurrency.Replace(" USD", "");
                                    price = price.Replace(".", ",");
                                }
                                catch { }

                                try // Поиск скидки
                                {
                                    element = cd.FindElement(By.XPath("//div[@class = 'productDetailElements' and ./strong[contains(text(), 'US SRP:')]]"));
                                    discount = element.Text;
                                    discount = discount.Replace("REG", "42%");
                                    discount = discount.Remove(discount.IndexOf(")")); // удалить весть текст после (и включительно) "%", если % нужно оставить, то проставить +1
                                    discount = discount.Substring(discount.IndexOf("Discount: ")); // удалить все до символа
                                    discount = discount.Replace("Discount: ", "");
                                    discount = discount.Replace("%", "");
                                }
                                catch { }

                                try // Поиск доступности
                                {
                                    element = cd.FindElement(By.XPath("//span[@class = 'productDetailTitle']/following-sibling::strong"));
                                    availability = element.Text;
                                    availability = availability.Replace("- ", "");
                                }
                                catch { }

                                try // Поиск доступности с количествами по разным складам
                                {
                                    bool result;
                                    int temp1 = 0;
                                    int temp2 = 0;
                                    int temp3 = 0;
                                    int temp4 = 0;
                                    int temp5 = 0;
                                    int temp6 = 0;
                                    int temp;

                                    IWebElement availabilityTN = cd.FindElement(By.XPath("//table[@class = 'newStockCheckTable']//tr[3]//td[@class = 'scTabledata']"));
                                    availability = "In stock TN: " + availabilityTN.Text;
                                    availability = availability.Replace(",", "");

                                    try
                                    {
                                        IWebElement availabilityPAC = cd.FindElement(By.XPath("//table[@class = 'newStockCheckTable']//tr[2]//td[@class = 'scTabledata']"));
                                        result = Int32.TryParse(availabilityPAC.Text.Replace(",", ""), out temp1);
                                    }
                                    catch { }

                                    try
                                    {
                                        IWebElement availabilityCA = cd.FindElement(By.XPath("//table[@class = 'newAltStockCheckTable']//tr[1]//td[@class = 'scTabledata']"));
                                        result = Int32.TryParse(availabilityCA.Text.Replace(",", ""), out temp2);
                                    }
                                    catch { }

                                    try
                                    {
                                        IWebElement availabilityIN = cd.FindElement(By.XPath("//table[@class = 'newAltStockCheckTable']//tr[2]//td[@class = 'scTabledata']"));
                                        result = Int32.TryParse(availabilityIN.Text.Replace(",", ""), out temp3);
                                    }
                                    catch { }

                                    try
                                    {
                                        IWebElement availabilityOH = cd.FindElement(By.XPath("//table[@class = 'newAltStockCheckTable']//tr[3]//td[@class = 'scTabledata']"));
                                        result = Int32.TryParse(availabilityOH.Text.Replace(",", ""), out temp4);
                                    }
                                    catch { }

                                    try
                                    {
                                        IWebElement availabilityOR = cd.FindElement(By.XPath("//table[@class = 'newAltStockCheckTable']//tr[4]//td[@class = 'scTabledata']"));
                                        result = Int32.TryParse(availabilityOR.Text.Replace(",", ""), out temp5);
                                    }
                                    catch { }

                                    try
                                    {
                                        IWebElement availabilityPAA = cd.FindElement(By.XPath("//table[@class = 'newAltStockCheckTable']//tr[5]//td[@class = 'scTabledata']"));
                                        result = Int32.TryParse(availabilityPAA.Text.Replace(",", ""), out temp6);
                                    }
                                    catch { }

                                    temp = temp1 + temp2 + temp3 + temp4 + temp5 + temp6;
                                    availability2 = "Another warehouses: " + temp.ToString();
                                }
                                catch { }

                                try // Поиск Readership
                                {
                                    element = cd.FindElement(By.XPath("//td[@class = 'productDetailSmallElements' and ./strong[contains(text(), 'Target Age Group:')]]"));
                                    readership = element.Text;
                                    readership = readership.Replace("Target Age Group: ", "");
                                }
                                catch { }

                                try // Поиск веса
                                {
                                    element = cd.FindElement(By.XPath("//td[@class = 'productDetailSmallElements' and ./strong[contains(text(), 'Physical Info:')]]"));
                                    weight = element.Text;
                                    weight = weight.Substring(weight.IndexOf("(") + 1); // удалить все до символа
                                    weight = weight.Remove(weight.IndexOf(") ")); // удалить весть текст после (и включительно) ")", если ) нужно оставить, то проставить +1
                                }
                                catch { }

                                try // Поиск размеров
                                {
                                    element = cd.FindElement(By.XPath("//td[@class = 'productDetailSmallElements' and ./strong[contains(text(), 'Physical Info:')]]"));
                                    dimensions = element.Text;
                                    dimensions = dimensions.Replace("Physical Info: ", "");
                                    dimensions = dimensions.Remove(dimensions.IndexOf("(")); // удалить весть текст после (и включительно) "(", если ( нужно оставить, то проставить +1
                                    dimensions = dimensions.Replace(" H", "");
                                    dimensions = dimensions.Replace(" L", "");
                                    dimensions = dimensions.Replace(" W", "");

                                    string[] temp = dimensions.Split('x');
                                    temp[0] = temp[0].Replace(" cms", ""); //Width (mm)
                                    temp[1] = temp[1].Replace(" cms", ""); //Height (mm)
                                    temp[2] = temp[2].Replace(" cms", ""); //Lenght (mm)

                                    temp[0] = temp[0].Replace(".", ",");
                                    temp[1] = temp[1].Replace(".", ",");
                                    temp[2] = temp[2].Replace(".", ",");

                                    double temp1 = Convert.ToDouble(temp[2]);
                                    temp1 = temp1 * 10;
                                    height = temp1.ToString();

                                    double temp2 = Convert.ToDouble(temp[1]);
                                    temp2 = temp2 * 10;
                                    width = temp2.ToString();

                                    double temp3 = Convert.ToDouble(temp[0]);
                                    temp3 = temp3 * 10;
                                    length = temp3.ToString();
                                }
                                catch { }

                                try // Поиск обложки
                                {
                                    element = cd.FindElement(By.XPath("//div[@class = 'productDetailElements' and ./strong[contains(text(), 'Binding:')]]"));
                                    bookCover = element.Text;
                                    bookCover = bookCover.Replace("Binding: ", "");
                                }
                                catch { }

                                try // Поиск количества страниц
                                {
                                    element = cd.FindElement(By.XPath("//td[@class = 'productDetailSmallElements' and ./strong[contains(text(), 'Physical Info:')]]"));
                                    pages = element.Text;
                                    pages = pages.Substring(pages.IndexOf(")") + 1);
                                }
                                catch { }

                                try // Поиск серии
                                {
                                    element = cd.FindElement(By.XPath("//td[@class = 'productDetailSmallElements' and ./strong[contains(text(), 'Series:')]]/a"));
                                    series = element.Text;
                                }
                                catch { }

                                try // Поиск описания
                                {
                                    element = cd.FindElement(By.XPath("//div[@class = 'productDetailElements' and .//strong[contains(text(), 'Annotation:')]]/div/div"));
                                    description = element.Text;
                                    description = description.Replace("\n", "");
                                    description = description.Replace("\r", "");
                                    description = description.Replace("\r\n", "");
                                    try
                                    {
                                        element = cd.FindElement(By.XPath("//div[@class = 'productDetailElements' and .//strong[contains(text(), 'Annotation:')]]/div/a"));
                                        element.Click();

                                        element = cd.FindElement(By.XPath("//div[@class = 'productDetailElements' and .//strong[contains(text(), 'Annotation:')]]/div[2]/div"));
                                        description = element.Text;
                                        description = description.Replace("\n", "");
                                        description = description.Replace("\r", "");
                                        description = description.Replace("\r\n", "");
                                    }
                                    catch { }
                                }
                                catch { }

                                try // Поиск второго ISBN
                                {
                                    element = cd.FindElement(By.XPath("//div[@class = 'newerVersionAvailable']/a"));
                                    element.Click();

                                    try
                                    {
                                        element = cd.FindElement(By.XPath("//div[@class = 'productDetailElements' and .//strong[contains(text(), 'EAN:')]]"));
                                        isbn2 = element.Text;
                                        isbn2 = isbn2.Substring(isbn2.IndexOf("EAN:"));
                                        isbn2 = isbn2.Replace("EAN:", "");
                                    }
                                    catch { }
                                }
                                catch { }
                            }
                            AdvancedSearch = false;
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
                WorkWithFile.SaveFile(book);
                PB.Close();
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
