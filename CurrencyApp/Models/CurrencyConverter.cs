namespace CurrencyApp.Models
{
    public class CurrencyConverter
    {
        public decimal USD { get; set; }
        public decimal ConvertToUSD(decimal priceRUB) => priceRUB / USD;

        public decimal EUR { get; set; }
        public decimal ConvertToEUR(decimal priceRUB) => priceRUB / EUR;

        //10 гривен номинал (такие данные от ЦБ)
        public decimal UAH10 { get; set; }
        public decimal ConvertToUAH(decimal priceRUB) => priceRUB / (UAH10 / 10);
    }
}
