using RELOD_Tools.Logic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System;
using RELOD_Tools.WebParsing.WebSearch;
using System.Text;

namespace RELOD_Tools.WebSearch.Site
{
    class Gardners : SiteSearchModel
    {
        HttpWebRequest  request;
        HttpWebResponse response;
        StreamReader    sr;
        CookieContainer cookieContainer = new CookieContainer();
        public Gardners(string[] isbns)
        {
            string loginPage    = "https://www.gardners.com/Account/LogOn";
            string isbnUrl      = "https://www.gardners.com/Product/";
            string accountNumber= "Номер аккаунта";
            string username     = "Имя пользователя";
            string password     = "Пароль";

            string pageSource = string.Empty;
            string temp = string.Empty;

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

            var cookieContainer = GetCookie(loginPage, accountNumber, username, password);

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
                    PB.Title = $"Поиск по сайту Gardners. Обработано {i + 1} из {isbnsLength}";
                    PB.progressBar.Value++;
                    progressvalue++;
                    Application.Current.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background,
                    new object[] { System.Windows.Controls.ProgressBar.ValueProperty, progressvalue });
                    //==================================================================================================

                    // Отправляем первый запрос по ISBN. Ответом нам будет страница с карточкой товара
                    if (isbn != "")
                    {
                        request = (HttpWebRequest)WebRequest.Create(isbnUrl + isbn);
                        request.CookieContainer = cookieContainer;

                        response = (HttpWebResponse)request.GetResponse();
                        sr = new StreamReader(response.GetResponseStream());
                        pageSource = sr.ReadToEnd();
                    }
                    //==================================================================================================

                    // Проверка, не слетел ли наш логин
                    if (pageSource.Contains("class=\"unauthenticated\">"))
                    {
                        MessageBox.Show("Не удалось авторизоваться. Поиск остановлен.");
                        break;
                    }
                    //==================================================================================================

                    // Блок обработки страницы с ответом
                    if (pageSource != string.Empty && isbn != "")
                    {
                        //pageSource = AlphabetCheck.Check(pageSource);

                        // Проверяем, есть ли замена
                        temp = "<span class=\"replacedByLink\">";
                        if (pageSource.Contains(temp))
                        {
                            isbn2 = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            isbn2 = isbn2.Substring(isbn2.IndexOf(">") + 1);
                            isbn2 = isbn2.Remove(isbn2.IndexOf("<"));
                        }

                        // Присваиваем наименование
                        temp = "<div class=\"titleContributor\">";
                        if (pageSource.Contains(temp))
                        {
                            title = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            title = title.Substring(title.IndexOf(">") + 1);
                            title = title.Remove(title.IndexOf("<"));
                            title = AlphabetCheck.Check(title);
                        }

                        // Присваиваем автора. Авторов может быть несколько ищем все строки
                        temp = "<span class=\"contributorRole\">";
                        if (pageSource.Contains(temp))
                        {

                            temp = pageSource.Substring(pageSource.IndexOf("<h2>") + 4);
                            temp = temp.Remove(temp.IndexOf("</h2>"));

                            string[] rows = temp.Split('\n');

                            for (int j = 0; j < rows.Length; j++)
                            {
                                if (rows[j].Contains("<a href"))
                                {
                                    temp = rows[j].Substring(rows[j].IndexOf("\">") + 2);
                                    temp = temp.Remove(temp.IndexOf("<"));
                                    author += temp + ", ";
                                }
                            }
                            author = author.Remove(author.LastIndexOf(","));
                            author = AlphabetCheck.Check(author);
                        }

                        // Присваиваем дату издания
                        temp = "<span>Published:</span>";
                        if (pageSource.Contains(temp))
                        {
                            pubDate = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            pubDate = pubDate.Replace("<span>", "");
                            pubDate = pubDate.Remove(pubDate.IndexOf("<"));
                            pubDate = AlphabetCheck.Check(pubDate);
                        }

                        // Присваиваем издательство
                        temp = "<span>Publisher:</span>";
                        if (pageSource.Contains(temp))
                        {
                            publisher = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            publisher = publisher.Substring(publisher.IndexOf("\">") + 2);
                            publisher = publisher.Remove(publisher.IndexOf("<"));
                            publisher = AlphabetCheck.Check(publisher);
                        }

                        // Присваиваем импринт
                        temp = "<span>Imprint:</span>";
                        if (pageSource.Contains(temp))
                        {
                            imprint = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            imprint = imprint.Substring(imprint.IndexOf("\">") + 2);
                            imprint = imprint.Remove(imprint.IndexOf("<"));
                            imprint = AlphabetCheck.Check(imprint);
                        }

                        // Присваиваем цену с валютой и без нее
                        temp = "<span class=\"retailPrice\">";
                        if (pageSource.Contains(temp))
                        {
                            priceWithCurrency = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            priceWithCurrency = priceWithCurrency.Remove(priceWithCurrency.IndexOf("<"));
                            priceWithCurrency = priceWithCurrency.Replace("&#163;", "") + " GBP";
                            priceWithCurrency = priceWithCurrency.Replace(".", ",");
                            price = priceWithCurrency.Remove(priceWithCurrency.IndexOf(" "));
                        }

                        // Присваиваем скидку
                        temp = "<p class=\"hideCustomer\"><span>";
                        if (pageSource.Contains(temp))
                        {
                            discount = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            discount = discount.Remove(discount.IndexOf(" "));
                        }

                        // Присваиваем доступность
                        temp = "<div class=\"availability\"";
                        if (pageSource.Contains(temp))
                        {
                            temp = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            temp = temp.Remove(temp.IndexOf("</div>"));

                            string[] rows = temp.Split('\n');

                            for (int j = 0; j < rows.Length; j++)
                            {
                                if (rows[j].Contains("class=\"icon-text"))
                                {
                                    temp = rows[j].Substring(rows[j].IndexOf("icon-text"));
                                    temp = temp.Substring(temp.IndexOf(">") + 1);
                                    temp = temp.Remove(temp.IndexOf("<"));
                                    availability += temp + ", ";
                                }
                            }
                            availability = availability.Remove(availability.LastIndexOf(","));
                        }

                        // Присваиваем читательскую группу (Readership)
                        temp = "<span>Readership:</span>";
                        if (pageSource.Contains(temp))
                        {
                            temp = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            temp = temp.Remove(temp.IndexOf("</ul>"));

                            string[] rows = temp.Split('\n');

                            for (int j = 0; j < rows.Length; j++)
                            {
                                if (rows[j].Contains("<li>"))
                                {
                                    temp = rows[j].Substring(rows[j].IndexOf(">") + 1);
                                    temp = temp.Remove(temp.IndexOf("<"));
                                    readership += temp + ", ";
                                }
                            }
                            readership = readership.Remove(readership.LastIndexOf(","));
                            readership = AlphabetCheck.Check(readership);
                        }

                        // Присваиваем издание
                        temp = "<span class=\"edition\">";
                        if (pageSource.Contains(temp))
                        {
                            edition = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            edition = edition.Remove(edition.IndexOf("<"));
                        }

                        // Присваиваем вес
                        temp = "<span>Weight:</span>";
                        if (pageSource.Contains(temp))
                        {
                            weight = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            weight = weight.Replace("<span>", "");
                            weight = weight.Remove(weight.IndexOf("g"));
                            weight = ((float.Parse(weight)) / 1000).ToString();
                        }

                        // Присваиваем размеры
                        temp = "<span>Dimensions:</span>";
                        if (pageSource.Contains(temp))
                        {
                            dimensions = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            dimensions = dimensions.Replace("<span>", "");
                            dimensions = dimensions.Remove(dimensions.IndexOf(" <"));

                            string[] dim = dimensions.Split('x');
                            try
                            {
                                length  = dim[0];
                                width   = dim[1];
                                height  = dim[2];
                            }
                            catch { }
                        }

                        // Присваиваем страну происхождения
                        temp = "<span>Pub. Country:</span>";
                        if (pageSource.Contains(temp))
                        {
                            pubCountry = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            pubCountry = pubCountry.Replace("<span>", "");
                            pubCountry = pubCountry.Remove(pubCountry.IndexOf("<"));
                        }

                        // Присваиваем классификацию
                        temp = "<span>Classifications:</span>";
                        if (pageSource.Contains(temp))
                        {
                            temp = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            temp = temp.Remove(temp.IndexOf("</ul>"));

                            string[] rows = temp.Split('\n');

                            for (int j = 0; j < rows.Length; j++)
                            {
                                if (rows[j].Contains("<a href"))
                                {
                                    temp = rows[j].Substring(rows[j].IndexOf("\">") + 2);
                                    temp = temp.Remove(temp.IndexOf("<"));
                                    classification += temp + ", ";
                                }
                            }
                            classification = classification.Remove(classification.LastIndexOf(","));
                            classification = AlphabetCheck.Check(classification);
                        }

                        // Присваиваем обложку
                        temp = "<li class=\"format_title\">";
                        if (pageSource.Contains(temp))
                        {
                            bookCover = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            bookCover = bookCover.Remove(bookCover.IndexOf("<"));
                        }

                        // Присваиваем страницы
                        temp = "<li class=\"pagination\">";
                        if (pageSource.Contains(temp))
                        {
                            pages = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            pages = pages.Remove(pages.IndexOf("<"));
                        }

                        // Присваиваем серию
                        temp = "<span>Series:</span>";
                        if (pageSource.Contains(temp))
                        {
                            series = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            series = series.Replace("<span>", "");
                            series = series.Substring(series.IndexOf(">") + 1);
                            series = series.Remove(series.IndexOf("<"));
                        }

                        // Присваиваем описание
                        temp = "<div class=\"productDescription\">";
                        if (pageSource.Contains(temp))
                        {
                            description = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            description = description.Remove(description.IndexOf("</div>"));
                            description = description.Replace("<br>", " ");

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

                        // Присваиваем содержание
                        temp = "<span>Contents:</span>";
                        if (pageSource.Contains(temp))
                        {
                            contents = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            contents = contents.Replace("<span>", "");
                            contents = contents.Remove(contents.IndexOf("</span>"));
                            contents = AlphabetCheck.Check(contents);
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
        private CookieContainer GetCookie(string loginPage, string accountNumber, string username, string password)
        {
            string reqString    = "AccountNumber=" + accountNumber + "&UserName=" + username + "&Password=" + password;
            byte[] data         = Encoding.UTF8.GetBytes(reqString);
            try
            {
                request = (HttpWebRequest)WebRequest.Create(loginPage);
                request.CookieContainer = cookieContainer;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                Errors.LoginPageError(ex);
                PB.Close();
            }
            return cookieContainer;
        }
    }
}
