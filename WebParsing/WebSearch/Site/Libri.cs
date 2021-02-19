using RELOD_Tools.Logic;
using RELOD_Tools.WebSearch;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace RELOD_Tools.WebParsing.WebSearch.Site
{
    class Libri:SiteSearchModel
    {
        HttpWebRequest  request;
        HttpWebResponse response;
        CookieContainer cookieContainer = new CookieContainer();
        StreamReader    sr;
        public Libri(string[] isbns)
        {
            string loginPage        = "https://mein.libri.de/en/Login.html";
            string isbnUrl          = "https://mein.libri.de/en/produkt/";
            string accountNumber    = "Номер аккаунта";
            string username         = "Имя пользователя";
            string password         = "Пароль";

            string pageSource = string.Empty;
            string temp = string.Empty;

            // Настраиваем Progress Bar
            PB.Show();
            int isbnsLength = isbns.Length;
            PB.progressBar.Minimum  = 0;
            PB.progressBar.Maximum  = isbnsLength;
            PB.progressBar.Value    = 0;
            double progressvalue    = 1;

            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(PB.progressBar.SetValue);
            Application.Current.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, progressvalue });
            //==================================================================================================
            
            cookieContainer = GetCookie(loginPage, accountNumber, username, password);

            try
            {
                for (int i = 0; i < isbns.Length; i++)
                {
                    pageSource = string.Empty;
                    // Пауза чтобы не нагружать сервер :)
                    Thread.Sleep(1000);

                    // Присваиваем порядковый номер
                    number = (i + 1).ToString();

                    // Присваиваем ISBN
                    isbn = isbns[i].Replace("\n", "");
                    isbn = isbn.Replace("\r", "");

                    // Передаем данные в Progress Bar для увеличения шкалы и обновления UI
                    PB.Title = $"Поиск по сайту Libri. Обработано {i + 1} из {isbnsLength}";
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
                    if (pageSource.Contains("Please enter your login information."))
                    {
                        MessageBox.Show("Не удалось авторизоваться. Поиск остановлен.");
                        break;
                    }
                    //==================================================================================================

                    // Блок обработки страницы с ответом
                    if (pageSource != string.Empty)
                    {
                        pageSource = AlphabetCheck.Check(pageSource);

                        // Блок проверки соответствия ISBN. Сравниваем тот ISBN который был в списке с тем что нашли.
                        // Если они не равны, тогда в столбце ISBN2 указываем что ISBN не совпадает.
                        temp = "Article no./EAN";
                        if (pageSource.Contains(temp))
                        {
                            string checkISBN = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            checkISBN = checkISBN.Substring(checkISBN.IndexOf("<td>") + 4);
                            checkISBN = checkISBN.Remove(checkISBN.IndexOf("<"));
                            checkISBN = AlphabetCheck.Check(checkISBN);
                            
                            if (isbn != checkISBN)
                            {
                                isbn2 = "ISBN не совпадает: " + checkISBN;
                            }
                        }

                        // Присваиваем наименование
                        temp = "<div class=\"col-md-6\"><h1>";
                        if (pageSource.Contains(temp))
                        {
                            title = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            title = title.Remove(title.IndexOf("<"));
                            title = AlphabetCheck.Check(title);
                        }

                        // Присваиваем автора
                        temp = "<td class=\"detail-label\"> Author </td>";
                        if (pageSource.Contains(temp))
                        {
                            author = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            author = author.Replace("<td>", "");
                            author = author.Remove(author.IndexOf("</td>"));
                            author = author.Replace("<br>", ";");
                            author = AlphabetCheck.Check(author);
                        }

                        // Присваиваем дату издания
                        temp = "<td class=\"detail-label\"> Release date </td> <td>";
                        if (pageSource.Contains(temp))
                        {
                            pubDate = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            pubDate = pubDate.Remove(pubDate.IndexOf("<"));
                            pubDate = AlphabetCheck.Check(pubDate);
                        }

                        // Присваиваем издательство
                        temp = "<td class=\"detail-label\"> Publisher </td> <td class=\"info-column\">";
                        if (pageSource.Contains(temp))
                        {
                            publisher = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            publisher = publisher.Remove(publisher.IndexOf("<"));
                            publisher = AlphabetCheck.Check(publisher);
                        }

                        // Присваиваем цену с валютой и без нее
                        temp = "<span class=\"price\">";
                        if (pageSource.Contains(temp))
                        {
                            priceWithCurrency = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            priceWithCurrency = priceWithCurrency.Remove(priceWithCurrency.IndexOf("<"));
                            priceWithCurrency = priceWithCurrency.Replace(".", ",");
                            price = priceWithCurrency.Remove(priceWithCurrency.IndexOf(" "));
                        }

                        // Присваиваем скидку
                        temp = "<td class=\"detail-label\"> Discount group </td>";
                        if (pageSource.Contains(temp))
                        {
                            discount = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            discount = discount.Substring(discount.IndexOf("content=\"") + 9);
                            discount = discount.Remove(discount.IndexOf("\""));
                            discount = discount.Replace("<br>", "; ");
                            try
                            {
                                discount = discount.Remove(discount.LastIndexOf(";"));
                            }
                            catch { }
                        }

                        // Присваиваем доступность
                        temp = "<td class=\"detail-label\"> Availability Status Code </td>";
                        if (pageSource.Contains(temp))
                        {
                            availability = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            availability = availability.Substring(availability.IndexOf("content=\"") + 9);
                            availability = availability.Remove(availability.IndexOf("\""));
                        }

                        // Присваиваем читательскую группу (Readership)
                        temp = "<td class=\"detail-label\"> Age group </td> <td>";
                        if (pageSource.Contains(temp))
                        {
                            readership = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            readership = readership.Remove(readership.IndexOf("<"));
                        }

                        // Присваиваем вес
                        temp = "<td class=\"detail-label\"> Weight </td> <td>";
                        if (pageSource.Contains(temp))
                        {
                            weight = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            weight = weight.Remove(weight.IndexOf("g"));
                            weight = ((float.Parse(weight)) / 1000).ToString();
                        }

                        // Присваиваем классификацию
                        temp = "<td class=\"detail-label\"> Product group </td> <td>";
                        if (pageSource.Contains(temp))
                        {
                            classification = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            classification = classification.Remove(classification.IndexOf("<"));
                        }

                        // Присваиваем обложку
                        temp = "<td class=\"detail-label\"> Book cover </td> <td>";
                        if (pageSource.Contains(temp))
                        {
                            bookCover = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            bookCover = bookCover.Remove(bookCover.IndexOf("<"));
                        }

                        // Присваиваем страницы
                        temp = "<td class=\"detail-label\"> Pages </td> <td>";
                        if (pageSource.Contains(temp))
                        {
                            pages = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            pages = pages.Remove(pages.IndexOf("<"));
                        }

                        // Присваиваем серию
                        temp = "<td class=\"detail-label\"> Series </td> <td>";
                        if (pageSource.Contains(temp))
                        {
                            series = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            series = series.Remove(series.IndexOf("<"));
                        }

                        // Присваиваем описание
                        temp = "<div class=\"detail-content detail-description\">";
                        if (pageSource.Contains(temp))
                        {
                            description = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            description = description.Remove(description.IndexOf("<"));
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
            // Сначала получаем токен:
            request = (HttpWebRequest)WebRequest.Create(loginPage);
            request.CookieContainer = cookieContainer;

            response = (HttpWebResponse)request.GetResponse();
            sr = new StreamReader(response.GetResponseStream());
            string pageSource = sr.ReadToEnd();

            string searchWord = "name=\"cmsauthenticitytoken\" value=\"";
            string token = pageSource.Substring(pageSource.IndexOf(searchWord) + searchWord.Length);
            token = token.Remove(token.IndexOf("\""));
            //==================================================================================================

            string reqString = 
                "module_fnc%5Bprimary%5D=Login&sSuccessURL=&sFailureURL=&sConsumer=loginBox" 
                + "&customerNumber=" + accountNumber 
                + "&slogin=" + username 
                + "&password=" + password 
                + "&cmsauthenticitytoken=" + token;
            
            byte[] data = Encoding.UTF8.GetBytes(reqString);
            try
            {
                request  = (HttpWebRequest)WebRequest.Create(loginPage);
                request.CookieContainer = cookieContainer;
                request.Method          = "POST";
                request.ContentType     = "application/x-www-form-urlencoded";
                request.ContentLength   = data.Length;

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
