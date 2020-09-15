using RELOD_Tools.Logic;
using RELOD_Tools.WebSearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RELOD_Tools.WebParsing.WebSearch.Site
{
    class Brill : SiteSearchModel
    {
        HttpWebRequest request;
        HttpWebResponse response;
        StreamReader sr;
        CookieContainer cookieContainer = new CookieContainer();
        public Brill(string[] isbns)
        {
            string isbnUrl = "https://brill.com/search?q1="; // 9789042036666
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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

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
                    PB.Title = $"Поиск по сайту Brill. Обработано {i + 1} из {isbnsLength}";
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

                    // Блок обработки страницы с ответом
                    if (pageSource != string.Empty && isbn != "")
                    {
                        pageSource = AlphabetCheck.Check(pageSource);

                        // Присваиваем наименование
                        temp = "<div class=\"typography-body text-headline color-primary\">";
                        if (pageSource.Contains(temp))
                        {
                            title = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            title = title.Substring(title.IndexOf(">") + 1);
                            title = title.Remove(title.IndexOf("<"));
                            //title = AlphabetCheck.Check(title);
                        }

                        // Присваиваем автора. Авторов может быть несколько ищем все строки
                        temp = "<div class=\"contributor-line text-subheading\">";
                        if (pageSource.Contains(temp))
                        {

                            temp = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            temp = temp.Remove(temp.IndexOf("</div>"));

                            string[] rows = Regex.Split(temp, "</span>");

                            for (int j = 0; j < rows.Length; j++)
                            {
                                if (rows[j].Contains("</a>"))
                                {
                                    temp = rows[j].Substring(rows[j].IndexOf("\">") + 2);
                                    temp = temp.Remove(temp.IndexOf("<"));
                                    author += temp + ", ";
                                }
                            }
                            try
                            {
                                author = author.Remove(author.LastIndexOf(","));
                                //author = AlphabetCheck.Check(author);
                            }
                            catch
                            {
                                author = string.Empty;
                            }
                        }

                        // Присваиваем описание
                        temp = "<div id=\"ABSTRACT_OR_EXCERPT";
                        if (pageSource.Contains(temp))
                        {
                            try
                            {
                                description = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                description = description.Substring(description.IndexOf(">") + 1);
                                description = description.Remove(description.IndexOf("</div>"));

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
                                        //description = AlphabetCheck.Check(description);
                                        done = true;
                                    }
                                }
                            }
                            catch
                            {
                                description = string.Empty;
                            }

                        }

                        // Присваиваем ссылку на картинку
                        temp = "<div class=\"cover cover-image configurable-index-card-cover-image\">";
                        if (pageSource.Contains(temp))
                        {
                            imageUrl = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            imageUrl = imageUrl.Substring(imageUrl.IndexOf(">") + 1);
                            imageUrl = imageUrl.Substring(imageUrl.IndexOf("src=\"") + 5);
                            imageUrl = "https://brill.com" + imageUrl.Remove(imageUrl.IndexOf(".") + 4);
                        }

                        temp = "<div class=\"content-box-body null\">";
                        if (pageSource.Contains(temp))
                        {
                            string[] body = Regex.Split(pageSource, temp);
                            string[] meta = null;
                            string mainContent = null;
                            for (int j = 0; j < body.Length; j++)
                            {
                                if (body[j].Contains("<div class=\"text-metadata-format\">"))
                                {
                                    body[j] = body[j].Replace("-", "");
                                    meta = Regex.Split(body[j], "<div class=\"textmetadataformat\">");
                                    break;
                                }
                            }

                            for (int j = 0; j < meta.Length; j++)
                            {
                                if (meta[j].Contains(isbn))
                                {
                                    mainContent = meta[j];
                                }
                            }

                            try
                            {
                                bookCover = mainContent.Remove(mainContent.IndexOf("<"));
                            }
                            catch { }
                            try
                            {
                                temp = "<dd class=\"availability inline cList__item cList__item  secondary textmetadatavalue\">";
                                availability = mainContent.Substring(mainContent.IndexOf(temp) + temp.Length);
                                availability = availability.Remove(availability.IndexOf("<"));
                            }
                            catch { }
                            try
                            {
                                temp = "<dd class=\"pubdate inline cList__item cList__item  secondary textmetadatavalue\">";
                                pubDate = mainContent.Substring(mainContent.IndexOf(temp) + temp.Length);
                                pubDate = pubDate.Remove(pubDate.IndexOf("<"));
                            }
                            catch { }
                            try
                            {
                                temp = "</abbr>";
                                price = mainContent.Substring(mainContent.IndexOf(temp) + temp.Length);
                                price = price.Remove(price.IndexOf("<"));
                                price = price.Replace("€", "");
                                price = price.Replace(".", ",");
                            }
                            catch { }
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
    }
}
