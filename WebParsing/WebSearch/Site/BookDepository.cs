using RELOD_Tools.WebSearch;
using RELOD_Tools.Logic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System;

namespace RELOD_Tools.WebParsing.WebSearch.Site
{
    class BookDepository : SiteSearchModel
    {
        HttpWebRequest  request;
        HttpWebResponse response;
        StreamReader    sr;
        CookieContainer cookieContainer = new CookieContainer();
        public BookDepository(string[] isbns)
        {
            string isbnUrl      = "https://www.bookdepository.com/search?searchTerm=";
            string pageSource   = string.Empty;
            string temp         = string.Empty;

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
                    PB.Title = $"Поиск по сайту BookDepository. Обработано {i + 1} из {isbnsLength}";
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
                        //pageSource = AlphabetCheck.Check(pageSource);

                        // Блок проверки соответствия ISBN. Сравниваем тот ISBN который был в списке с тем что нашли.
                        // Если они не равны, тогда в столбце ISBN2 указываем что ISBN не совпадает.
                        temp = "<span itemprop=\"isbn\">";
                        string checkISBN = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                        checkISBN = checkISBN.Remove(checkISBN.IndexOf("<"));

                        if (isbn != checkISBN)
                        {
                            isbn2 = "ISBN не совпадает: " + checkISBN;
                        }

                        // Присваиваем наименование
                        temp = "<h1 itemprop=\"name\">";
                        if (pageSource.Contains(temp))
                        {
                            title = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            title = title.Remove(title.IndexOf("<"));
                            title = AlphabetCheck.Check(title);
                        }

                        // Присваиваем автора. Авторов может быть несколько ищем все строки
                        temp = "<div class=\"author-info";
                        if (pageSource.Contains(temp))
                        {

                            temp = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            temp = temp.Remove(temp.IndexOf("</div>"));

                            string[] rows = temp.Split('\n');

                            for (int j = 0; j < rows.Length; j++)
                            {
                                if (rows[j].Contains("itemscope=\""))
                                {
                                    temp = rows[j].Substring(rows[j].IndexOf("itemscope=\"") + 11);
                                    temp = temp.Remove(temp.IndexOf("\""));
                                    author += temp + ", ";
                                }
                            }
                            try
                            {
                                author = author.Remove(author.LastIndexOf(","));
                                author = AlphabetCheck.Check(author);
                            }
                            catch 
                            { 
                                author = string.Empty; 
                            }
                        }

                        // Присваиваем дату издания
                        temp = "<span itemprop=\"datePublished\">";
                        if (pageSource.Contains(temp))
                        {
                            pubDate = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            pubDate = pubDate.Remove(pubDate.IndexOf("<"));
                            pubDate = AlphabetCheck.Check(pubDate);
                        }

                        // Присваиваем издательство
                        temp = "<span itemprop=\"publisher\"";
                        if (pageSource.Contains(temp))
                        {
                            publisher = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            publisher = publisher.Substring(publisher.IndexOf("itemscope=\"") + 11);
                            publisher = publisher.Remove(publisher.IndexOf("\""));
                            publisher = AlphabetCheck.Check(publisher);
                        }

                        // Присваиваем импринт
                        temp = "<label>Imprint</label>";
                        if (pageSource.Contains(temp))
                        {
                            imprint = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            imprint = imprint.Substring(imprint.IndexOf(">") + 1);
                            imprint = imprint.Remove(imprint.IndexOf("<"));
                            imprint = AlphabetCheck.Check(imprint);
                        }

                        // Присваиваем цену с валютой и без нее
                        temp = "<span class=\"sale-price\">";
                        if (pageSource.Contains(temp))
                        {
                            priceWithCurrency = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            priceWithCurrency = priceWithCurrency.Remove(priceWithCurrency.IndexOf("<"));
                            priceWithCurrency = priceWithCurrency.Replace("US$", "") + " USD";
                            priceWithCurrency = priceWithCurrency.Replace(".", ",");
                            price = priceWithCurrency.Remove(priceWithCurrency.IndexOf(" "));
                        }

                        // Проверяем, есть ли строка, говорящая о недоступности
                        temp = "<p class=\"list-price\">";
                        if (pageSource.Contains(temp))
                        {
                            availability        = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            priceWithCurrency   = availability.Substring(availability.IndexOf("$") + 1);
                            priceWithCurrency   = priceWithCurrency.Remove(priceWithCurrency.IndexOf("<"));
                            priceWithCurrency   += " USD";
                            priceWithCurrency   = priceWithCurrency.Replace(".", ",");
                            price               = priceWithCurrency.Remove(priceWithCurrency.IndexOf(" "));

                            availability        = availability.Substring(availability.IndexOf(">") + 1);

                            if (availability.Contains("<p class=\"red-text bold\">"))
                            {
                                availability = availability.Substring(availability.IndexOf(">") + 1);
                                availability = availability.Remove(availability.IndexOf("<"));
                            }
                        }

                        // Присваиваем доступность
                        temp = "<div class=\"availability-text\">";
                        if (pageSource.Contains(temp))
                        {
                            availability = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            availability = availability.Substring(availability.IndexOf("<p>") + 3);
                            availability = availability.Remove(availability.IndexOf("<"));
                            availability = AlphabetCheck.Check(availability);
                        }

                        // Присваиваем вес
                        temp = "<label>Dimensions</label>";
                        if (pageSource.Contains(temp))
                        {
                            try
                            {
                                weight = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                weight = weight.Remove(weight.IndexOf("</span>"));
                                weight = weight.Substring(weight.IndexOf("| ") + 1);
                                weight = weight.Remove(weight.IndexOf("g"));

                                weight = AlphabetCheck.Check(weight);

                                weight = ((float.Parse(weight)) / 1000).ToString();
                            }
                            catch 
                            {
                                weight = string.Empty;
                            }
                        }

                        // Присваиваем размеры
                        temp = "<label>Dimensions</label>";
                        if (pageSource.Contains(temp))
                        {
                            dimensions = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            dimensions = dimensions.Replace("<span>", "");
                            dimensions = dimensions.Remove(dimensions.IndexOf("mm"));
                            dimensions = AlphabetCheck.Check(dimensions);

                            string[] dim = dimensions.Split('x');
                            try
                            {
                                length = dim[1];
                                width = dim[0];
                                height = dim[2];
                            }
                            catch { }
                        }

                        // Присваиваем страну происхождения
                        temp = "<label>Publication City/Country</label>";
                        if (pageSource.Contains(temp))
                        {
                            pubCountry = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            pubCountry = pubCountry.Replace("<span>", "");
                            pubCountry = pubCountry.Remove(pubCountry.IndexOf("<"));
                            pubCountry = AlphabetCheck.Check(pubCountry);
                        }

                        // Присваиваем обложку
                        temp = "<label>Format</label>";
                        if (pageSource.Contains(temp))
                        {
                            bookCover = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            bookCover = bookCover.Substring(bookCover.IndexOf(">") + 1);
                            bookCover = bookCover.Remove(bookCover.IndexOf("<"));

                            // Здесь может быть ошибка. Т.к. не всегда присутствует элемент " | "
                            try
                            {
                                bookCover = bookCover.Remove(bookCover.IndexOf("|"));
                            }
                            catch { }
                            bookCover = AlphabetCheck.Check(bookCover);
                        }

                        // Присваиваем страницы
                        temp = "<span itemprop=\"numberOfPages\">";
                        if (pageSource.Contains(temp))
                        {
                            pages = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                            pages = pages.Remove(pages.IndexOf(" "));
                        }

                        // Присваиваем описание
                        temp = "<div class=\"item-excerpt trunc\" itemprop=\"description\" data-height=\"230\">";
                        if (pageSource.Contains(temp))
                        {
                            try
                            {
                                description = pageSource.Substring(pageSource.IndexOf(temp) + temp.Length);
                                description = description.Remove(description.IndexOf("</div>"));
                                description = description.Remove(description.LastIndexOf("<br />"));
                                description = description.Replace("<br />", " ");

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
                            }
                            catch
                            {
                                description = string.Empty;
                            }
                            //==================================================================================================
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
