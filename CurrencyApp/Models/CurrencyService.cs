using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;

namespace CurrencyApp.Models
{
    public class CurrencyService : BackgroundService
    {
        private readonly IMemoryCache memoryCache;

        public CurrencyService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //так как эта задача выполняется в другом потоке, то велика вероятность, что
                    //культура по умолчанию может отличаться от той, которая установлена в нашем приложении,
                    //поэтому явно укажем нужную нам, чтобы не было проблем с разделителями, названиями и т.д.
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU"); // <== нужная вам культура

                    //кодировка файла xml с сайта ЦБ == windows-1251
                    //по умолчанию она недоступна в .NET Core, поэтому регистрируем нужный провайдер 
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    //т.к. мы знаем что данные к нам приходят именно в файле, именно в формате XML,
                    //поэтому нет необходимости использовать WebRequest,
                    //используем в работе класс XDocument и сразу забираем файл с удаленного сервера
                    XDocument xml = XDocument.Load("http://www.cbr.ru/scripts/XML_daily.asp");

                    //далее парсим файл и находим нужные нам валюты по их коду ID, и заполняем класс-модель
                    CurrencyConverter currencyConverter = new CurrencyConverter();
                    currencyConverter.USD = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute").FirstOrDefault(x => x.Element("NumCode").Value == "840").Elements("Value").FirstOrDefault().Value);
                    currencyConverter.EUR = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute").FirstOrDefault(x => x.Element("NumCode").Value == "978").Elements("Value").FirstOrDefault().Value);
                    currencyConverter.UAH10 = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute").FirstOrDefault(x => x.Element("NumCode").Value == "980").Elements("Value").FirstOrDefault().Value);

                    memoryCache.Set("key_currency", currencyConverter, TimeSpan.FromMinutes(1440));
                }
                catch (Exception e)
                {
                    //logger.LogError(e.InnerException.Message);
                }

                //если указаний о завершении данной задачи не поступало,
                //то запрашиваем обновление данных каждый час
                await Task.Delay(3600000, stoppingToken);
            }
        }
    }
}
