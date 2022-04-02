namespace LuccaConsoleApp
{
    internal class CurrencyConversionRate
    {
        private decimal currency1ToCurrency2Rate;
        private decimal currency2ToCurrency1Rate;

        public string Currency1 { get; set; }
        public string Currency2 { get; set; }
        public decimal Currency1ToCurrency2Rate
        {
            get
            {
                return currency1ToCurrency2Rate;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Tous les taux d'échange doivent etre positifs.");

                // Ensure the converstion rate has a maximum of 4 significant digits after the decimal point
                var newRate = Math.Round(value, 4);

                if (newRate > currency1ToCurrency2Rate)
                {
                    currency1ToCurrency2Rate = newRate;
                }

                decimal newCurrency2ToCurrency1Rate = Math.Round(1m / currency1ToCurrency2Rate, 4);

                if (newCurrency2ToCurrency1Rate > Currency2ToCurrency1Rate)
                    currency2ToCurrency1Rate = newCurrency2ToCurrency1Rate;
            }
        }

        public decimal Currency2ToCurrency1Rate
        {
            get
            {
                return currency2ToCurrency1Rate;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Tous les taux d'échange doivent etre positifs.");

                // Ensure the converstion rate has a maximum of 4 significant digits after the decimal point
                var newRate = Math.Round(value, 4);

                if (newRate > currency2ToCurrency1Rate)
                {
                    currency2ToCurrency1Rate = newRate;
                }

                decimal newCurrency1ToCurrency2Rate = Math.Round(1m / currency2ToCurrency1Rate, 4);

                if (newCurrency1ToCurrency2Rate > Currency2ToCurrency1Rate)
                    currency1ToCurrency2Rate = newCurrency1ToCurrency2Rate;
            }
        }

        public bool IsActive { get; set; } = true;

        public CurrencyConversionRate(string initialCurrency, string targetCurrency, decimal rate)
        {
            Currency1 = initialCurrency;
            Currency2 = targetCurrency;
            Currency1ToCurrency2Rate = rate;
        }

        public decimal GetConversionRate(string fromCurrency)
        {
            if (Currency1 == fromCurrency)
                return Currency1ToCurrency2Rate;
            else if (Currency2 == fromCurrency)
                return Currency2ToCurrency1Rate;
            else
                throw new InvalidOperationException(string.Format("The currency does not match the available currencies for the conversion {0}/{1}", Currency1, Currency2));
        }

        public decimal CalculateNewAmount(decimal oldAmount, string initialCurrency)
        {
            return Math.Round(oldAmount * GetConversionRate(initialCurrency), 4);
        }
    }
}
