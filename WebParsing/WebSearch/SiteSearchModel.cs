using System;
using System.Collections.Generic;

namespace RELOD_Tools.WebSearch
{
    abstract class SiteSearchModel
    {
        protected ProgressBar PB = new ProgressBar();

        protected delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        protected SiteSearchModel()
        {
            // Сразу добавляем книгу (для шапки)
            InitBook();
        }
        protected List<BookModel> book = new List<BookModel>();
        protected string number             = "";
        protected string isbn               = "";
        protected string isbn2              = "";
        protected string title              = "";
        protected string author             = "";
        protected string pubDate            = "";
        protected string publisher          = "";
        protected string imprint            = "";
        protected string supplier           = "";
        protected string priceWithCurrency  = "";
        protected string price              = "";
        protected string priceComparision   = "";
        protected string discount           = "";
        protected string availability       = "";
        protected string availability2      = "";
        protected string marketRestrictions = "";
        protected string readership         = "";
        protected string edition            = "";
        protected string weight             = "";
        protected string dimensions         = "";
        protected string pubCountry         = "";
        protected string classification     = "";
        protected string bookCover          = "";
        protected string pages              = "";
        protected string series             = "";
        protected string description        = "";
        protected string language           = "";
        protected string contents           = "";
        protected string length             = "";
        protected string height             = "";
        protected string width              = "";
        private void InitBook()
        {
            book.Add(new BookModel()
            {
                Number                      = "Номер",
                Isbn                        = "ISBN",
                Isbn2                       = "ISBN_2",
                Title                       = "Наименование",
                Author                      = "Автор",
                PubDate                     = "Дата публикации",
                Publisher                   = "Издательство",
                Imprint                     = "Импринт (Divison для ABE-IPS)",
                Supplier                    = "Поставщик",
                PriceWithCurrency           = "Цена с валютой",
                Price                       = "Цена",
                PriceComparision            = "Сравнение цены (для PubEasy)",
                Discount                    = "Скидка (для ABE-IPS сразу указывается цена со скидкой)",
                Availability                = "Доступность",
                Availability2               = "Доступность_2 (для PubEasy)",
                MarketRestrictions          = "Market Restrictions",
                Readership                  = "Читательская группа",
                Edition                     = "Издание",
                Weight                      = "Вес",
                Dimensions                  = "Размеры",
                PubCountry                  = "Страна издания",
                Classification              = "Классификация",
                BookCover                   = "Обложка",
                Pages                       = "Страницы",
                Series                      = "Серия",
                Description                 = "Описание",
                Language                    = "Язык",
                Contents                    = "Содержание",
                Length                      = "Длинна",
                Width                       = "Ширина",
                Height                      = "Высота"
            });
        }

        // Функция добавляет элементы в List<BookModel> для его последующего сохранения в файл
        protected void AddBookToList()
        {
            book.Add(new BookModel()
            {
                Number                      = number,
                Isbn                        = isbn,
                Isbn2                       = isbn2,
                Title                       = title,
                Author                      = author,
                PubDate                     = pubDate,
                Publisher                   = publisher,
                Imprint                     = imprint,
                Supplier                    = supplier,
                PriceWithCurrency           = priceWithCurrency,
                Price                       = price,
                PriceComparision            = priceComparision,
                Discount                    = discount,
                Availability                = availability,
                Availability2               = availability2,
                MarketRestrictions          = marketRestrictions,
                Readership                  = readership,
                Edition                     = edition,
                Weight                      = weight,
                Dimensions                  = dimensions,
                PubCountry                  = pubCountry,
                Classification              = classification,
                BookCover                   = bookCover,
                Pages                       = pages,
                Series                      = series,
                Description                 = description,
                Language                    = language,
                Contents                    = contents,
                Length                      = length,
                Height                      = height,
                Width                       = width
            });
        }
        // Функция для очистки переменных. Это нужно для того чтобы после каждой итерации поиска позиций обнулялись переменные,
        // т.к. в них остаются данные которые могут быть не перезаписаны в следующей итерации (если например по первой книге был автор,
        // а в следующей книге его нет)
        protected void ClearBookList()
        {
            isbn2                           = "";
            title                           = "";
            author                          = "";
            pubDate                         = "";
            publisher                       = "";
            imprint                         = "";
            supplier                        = "";
            priceWithCurrency               = "";
            price                           = "";
            priceComparision                = "";
            discount                        = "";
            availability                    = "";
            availability2                   = "";
            marketRestrictions              = "";
            readership                      = "";
            edition                         = "";
            weight                          = "";
            dimensions                      = "";
            pubCountry                      = "";
            classification                  = "";
            bookCover                       = "";
            pages                           = "";
            series                          = "";
            description                     = "";
            language                        = "";
            contents                        = "";
            length                          = "";
            height                          = "";
            width                           = "";
    }
    }
}
