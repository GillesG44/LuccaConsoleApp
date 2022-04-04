namespace LuccaConsoleApp
{
    internal class CurrencyConversionRate
    {
        #region private Variables
        // The currency rates are stored separately from currency 1 to 2 and from currency 2 to 1. This allows for different rates to be passed in each direction.
        private decimal currency1ToCurrency2Rate;
        private decimal currency2ToCurrency1Rate;
        #endregion


        #region Public Properties
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
        #endregion

        #region Constructors
        public CurrencyConversionRate(string initialCurrency, string targetCurrency, decimal rate)
        {
            Currency1 = initialCurrency;
            Currency2 = targetCurrency;
            Currency1ToCurrency2Rate = rate;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the conversion rate based on the initial currency
        /// </summary>
        /// <param name="fromCurrency"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public decimal GetConversionRate(string fromCurrency)
        {
            if (Currency1 == fromCurrency)
                return Currency1ToCurrency2Rate;
            else if (Currency2 == fromCurrency)
                return Currency2ToCurrency1Rate;
            else
                throw new InvalidOperationException(string.Format("The currency does not match the available currencies for the conversion {0}/{1}", Currency1, Currency2));
        }

        /// <summary>
        /// Calculate the amount obtained in the target currency after the conversion from the initial currency
        /// </summary>
        /// <param name="initialAmount"></param>
        /// <param name="initialCurrency"></param>
        /// <returns></returns>
        public decimal CalculateNewAmount(decimal initialAmount, string initialCurrency)
        {
            return Math.Round(initialAmount * GetConversionRate(initialCurrency), 4);
        }
        #endregion
    }
}
