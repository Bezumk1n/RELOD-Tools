using RELOD_Tools.WebSearch;
using RELOD_Tools.Logic;
using OpenQA.Selenium;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System;
using OpenQA.Selenium.Chrome;

namespace RELOD_Tools.WebParsing.WebSearch.Site
{
    class ABEIPS : SiteSearchModel
    {
        HttpWebRequest  request;
        HttpWebResponse response;
        StreamReader    sr;
        ChromeDriver    cd = new ChromeDriver();
        CookieContainer cookieContainer = new CookieContainer();
        public ABEIPS(string[] isbns)
        {
            string loginPage    = "https://biznes.abe.pl/login";
            string isbnUrl      = "https://biznes.abe.pl/search/?search_param=all&q=";
            string username     = "Имя пользователя";
            string password     = "Пароль";

            string pageSource   = string.Empty;
            string temp         = string.Empty;
            
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
            
            var cookieContainer = GetCookie(loginPage, username, password);

            try
            {
                for (int i = 0; i < isbns.Length; i++)
                {
                    // Пауза чтобы не нагружать сервер :)
                    Thread.Sleep(1000);

                    // Присваиваем порядковый номер
                    number = (i + 1).ToString();

                    // Присваиваем ISBN
                    isbn = isbns[i].Replace("\n", "");
                    isbn = isbn.Replace("\r", "");

                    // Передаем данные в Progress Bar для увеличения шкалы и обновления UI
                    PB.Title = $"Поиск по сайту ABE-IPS. Обработано {i + 1} из {isbnsLength}";
                    PB.progressBar.Value++;
                    progressvalue++;
                    Application.Current.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background,
                    new object[] { System.Windows.Controls.ProgressBar.ValueProperty, progressvalue });
                    //==================================================================================================

                    // Отправляем первый запрос по ISBN. Ответом нам будет страница со списком, которую мы записываем в переменную PageSource
                    if (isbn != "")
                    {
                        request = (HttpWebRequest)WebRequest.Create(isbnUrl + isbn);
                        request.CookieContainer = cookieContainer;

                        response = (HttpWebResponse)request.GetResponse();
                        sr = new StreamReader(response.GetResponseStream());
                        pageSource = sr.ReadToEnd();
                    }
                    //==================================================================================================

                    // Проверка, удалось ли залогиниться, если на сранице есть текст "Login" значит прерываем цикл.
                    if (pageSource.Contains("Login"))
                    {
                        MessageBox.Show("Не удалось авторизоваться. Поиск остановлен.");
                        break;
                    }
                    //==================================================================================================

                    // Блок обработки страницы с результатом первого запроса.
                    // Так как на странице с результатом может быть несколько ссылок на книги, пробуем найти нужную нам.
                    // Найденную ссылку передаем в следующий запрос
                    
                    string delimeter = "<div class=\"col-md-6\">";

                    if (pageSource.Contains(delimeter))
                    {
                        // Разбиваем текст страницы на строки и считаем сколько строк содержит наш delimeter
                        // Это нужно чтобы создать массив с нужными нам элементами
                        string[] rows       = pageSource.Split('\n');
                        int divCount        = 0;

                        for (int j = 0; j < rows.Length; j++)
                        {
                            if (rows[j].Contains(delimeter))
                            {
                                divCount++;
                            }
                        }

                        // Собираем все строки содержащие delimeter в массив divs,
                        // в них нужно найти ту строку в которой упоминается ISBN нашей книги.
                        // Правильная строка с обрабатывается и присваивается переменной href.
                        string[] div = new string[divCount];

                        int divStartIndex   = 0;
                        int divEndIndex     = 0;
                        string href         = "";

                        for (int j = 0; j < divCount; j++)
                        {
                            divStartIndex   = pageSource.IndexOf(delimeter, divEndIndex);
                            divEndIndex     = pageSource.IndexOf("</div>", divStartIndex);

                            div[j] = pageSource.Substring(divStartIndex);
                            div[j] = div[j].Remove(divEndIndex - divStartIndex);
                            
                            if (div[j].Replace("-", "").Contains(isbn) == true)
                            {
                                href = div[j].Substring(div[j].IndexOf(delimeter) + delimeter.Length);
                                href = href.Substring(href.IndexOf("\"") + 1);
                                href = href.Remove(href.IndexOf("\""));
                                href = "https://biznes.abe.pl" + href;
                                break;
                            }
                            else
                            {
                                // Если ни одна строка не содержит искомого ISBN тогда берем первую строку и забираем из нее ссылку
                                href = div[0].Substring(div[0].IndexOf(delimeter) + delimeter.Length);
                                href = href.Substring(href.IndexOf("\"") + 1);
                                href = href.Remove(href.IndexOf("\""));
                                href = "https://biznes.abe.pl" + href;
                            }
                        }
                        //==================================================================================================

                        request = (HttpWebRequest)WebRequest.Create(href);
                        request.CookieContainer = cookieContainer;

                        try
                        {
                            response = (HttpWebResponse)request.GetResponse();
                            sr = new StreamReader(response.GetResponseStream());
                            pageSource = sr.ReadToEnd();
                        }
                        catch
                        {
                            pageSource = string.Empty;
                        }
                        //==================================================================================================

                        // Блок обработки карточки товара. Пытаемся получить всю доступную информацию
                        if (pageSource != string.Empty)
                        {
                            pageSource = AlphabetCheck.Check(pageSource);

                            // Блок проверки соответствия ISBN. Сравниваем тот ISBN который был в списке с тем что нашли.
                            // Если они не равны, тогда в столбце ISBN2 указываем что ISBN не совпадает.
                            temp = "<dt>EAN</dt> <dd>";
                            string checkISBN = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            checkISBN = checkISBN.Remove(checkISBN.IndexOf("<"));

                            if (isbn != checkISBN)
                            {
                                isbn2 = "ISBN не совпадает: " + checkISBN;
                            }

                            // Присваиваем наименование
                            temp = "<div class=\"page-header\">";
                            title = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            title = title.Substring(title.IndexOf("class=\"\">") + 9);
                            title = title.Remove(title.IndexOf("<"));

                            // Присваиваем автора
                            temp = "<dt>Author</dt> <dd>";
                            if (pageSource.Contains(temp))
                            {
                                author = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                author = author.Remove(author.IndexOf("<"));
                            }

                            // Присваиваем дату издания
                            temp = "<dt>Date</dt> <dd>";
                            if (pageSource.Contains(temp))
                            {
                                pubDate = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                pubDate = pubDate.Remove(pubDate.IndexOf("<"));
                            }

                            // Присваиваем Division
                            temp = "<dt>Division</dt> <dd>";
                            if (pageSource.Contains(temp))
                            {
                                imprint = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                imprint = imprint.Substring(imprint.IndexOf(">") + 1);
                                imprint = imprint.Remove(imprint.IndexOf("<"));
                            }

                            // Присваиваем издательство
                            temp = "<dt>Publisher</dt> <dd>";
                            if (pageSource.Contains(temp))
                            {
                                publisher = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                publisher = publisher.Substring(publisher.IndexOf(">") + 1);
                                publisher = publisher.Remove(publisher.IndexOf("<"));
                            }

                            // Присваиваем цену с валютой и без нее
                            temp = "<small class='text-green'>List price</small>";
                            if (pageSource.Contains(temp))
                            {
                                priceWithCurrency = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                priceWithCurrency = priceWithCurrency.Substring(priceWithCurrency.IndexOf("> ") + 2);
                                priceWithCurrency = priceWithCurrency.Remove(priceWithCurrency.IndexOf("<"));
                                price = priceWithCurrency.Remove(priceWithCurrency.IndexOf(" "));
                                price = price.Replace(".", ",");
                            }

                            // Присваиваем цену со скидкой
                            temp = "<small class='text-green'>Your price</small>";
                            if (pageSource.Contains(temp))
                            {
                                discount = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                discount = discount.Substring(discount.IndexOf("> ") + 2);
                                discount = discount.Remove(discount.IndexOf("<"));
                                discount = discount.Remove(discount.IndexOf(" "));
                                discount = discount.Replace(".", ",");
                            }

                            // Присваиваем доступность
                            temp = "<span class=\"text-blue-first\">";
                            if (pageSource.Contains(temp))
                            {
                                availability = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                availability = availability.Substring(availability.IndexOf(">") + 1);
                                availability = availability.Remove(availability.IndexOf("<"));
                            }

                            // Присваиваем читательскую группу
                            temp = "<dt>Readership level</dt> <dd>";
                            if (pageSource.Contains(temp))
                            {
                                readership = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                readership = readership.Remove(readership.IndexOf("<"));
                            }

                            // Присваиваем издание
                            temp = "<dt>Edition</dt> <dd>";
                            if (pageSource.Contains(temp))
                            {
                                edition = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                edition = edition.Remove(edition.IndexOf("<"));
                            }

                            // Присваиваем обложку
                            temp = "<dt>Cover</dt> <dd>";
                            if (pageSource.Contains(temp))
                            {
                                bookCover = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                bookCover = bookCover.Remove(bookCover.IndexOf("<"));
                            }

                            // Присваиваем страницы
                            temp = "<dt>Pages</dt> <dd>";
                            if (pageSource.Contains(temp))
                            {
                                pages = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                pages = pages.Remove(pages.IndexOf("<"));
                            }

                            // Присваиваем описание, убираем все лишнее из текста
                            temp = "<h3>Description</h3>";
                            if (pageSource.Contains(temp))
                            {
                                description = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                description = description.Substring(description.IndexOf("<div class=\"col-md-12\">") + 23);
                                description = description.Remove(description.IndexOf("</div>"));

                                description = description.Replace("</p>", " "); // здесь меняем на пробел
                                description = description.Replace("</P>", " "); // здесь меняем на пробел

                                // Иногда в описании присутствуют ссылки (текст содержит <a href = ...>), ищем индексы символов "<" и ">"
                                // Когда нашли индексы убираем содержимое между ними методом Remove

                                int startIndex = 0;
                                int lastIndex = 0;
                                bool done = false;

                                while (done != true)
                                {
                                    char[] tempDescription = description.ToCharArray();

                                    for (int j = 0; j < description.Length; j++)
                                    {
                                        if (tempDescription[j] == '<')
                                        {
                                            startIndex = j;
                                            break;
                                        }
                                    }
                                    for (int k = startIndex; k < description.Length; k++)
                                    {
                                        if (tempDescription[k] == '>')
                                        {
                                            lastIndex = k - startIndex + 1;
                                            break;
                                        }
                                    }
                                    description = description.Remove(startIndex, lastIndex);
                                    if (!description.Contains('<') && !description.Contains('>'))
                                    {
                                        description = AlphabetCheck.Check(description);
                                        done = true;
                                    }
                                }
                                //==================================================================================================
                            }
                        }
                        AddBookToList();
                        ClearBookList();
                    }
                    else
                    {
                        AddBookToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Errors.CustomError(ex);
            }
            WorkWithFile.SaveFile(book);

            PB.Close();
        }

        private CookieContainer GetCookie(string loginPage, string username, string password)
        {
            try
            {
                cd.Url = loginPage;
                IWebElement element;

                element = cd.FindElement(By.Id("_username"));
                element.SendKeys(username);
                element = cd.FindElement(By.Id("_password"));
                element.SendKeys(password);
                
                element = cd.FindElement(By.XPath("//*[@class = 'btn btn-success']"));
                element.Click();

                foreach (OpenQA.Selenium.Cookie c in cd.Manage().Cookies.AllCookies)
                {
                    string name     = c.Name;
                    string value    = c.Value;
                    cookieContainer.Add(new System.Net.Cookie(name, value, c.Path, c.Domain));
                }
                cd.Quit();
            }
            catch (Exception ex)
            {
                cd.Quit();
                PB.Close();
                Errors.LoginPageError(ex);
            }
            return cookieContainer;
        }
    }
}
