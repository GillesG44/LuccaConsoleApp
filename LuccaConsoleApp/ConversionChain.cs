namespace LuccaConsoleApp
{
    /// <summary>
    /// The ConversionChain objects hold information about a chain of conversions (ordered list of conversions, initial currency and final currency)
    /// They are used to calculate the amount obtained by converting from the inital currency to the final one while going throuhg the entire chain of conversions.
    /// </summary>
    internal class ConversionChain
    {
        #region Properties
        public string InitialCurrency { get; private set; }

        /// <summary>
        /// Last currency reached with the current chain. The Last Currency is updated every time a conversion gets added to the chain.
        /// </summary>
        public string LastCurrency { get; private set; }

        /// <summary>
        /// Ordered list of conversions that form the chain.
        /// </summary>
        public LinkedList<CurrencyConversionRate> ConversionChainList { get; set; } = new LinkedList<CurrencyConversionRate>();

        #endregion

        #region Constructors
        public ConversionChain(string initialCurrency)
        {
            InitialCurrency = initialCurrency;
            LastCurrency = initialCurrency;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a conversion at the end of the chain
        /// </summary>
        /// <param name="currencyConversionRate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddConversion(CurrencyConversionRate currencyConversionRate)
        {
            ConversionChainList.AddLast(currencyConversionRate);

            if (currencyConversionRate.Currency1 == LastCurrency)
            {
                LastCurrency = currencyConversionRate.Currency2;
            }
            else if (currencyConversionRate.Currency2 == LastCurrency)
            {
                LastCurrency = currencyConversionRate.Currency1;
            }
            else
                throw new InvalidOperationException(String.Format("La devise {0} n'est pas présente parmis les devises du taux d'échange {1}/{2}", LastCurrency, currencyConversionRate.Currency1, currencyConversionRate.Currency2));
        }

        /// <summary>
        /// Calculates the amount in the last currency after applying all the conversion in the chain, based on the amount in the initial currency
        /// </summary>
        /// <param name="initialAmount"></param>
        /// <param name="initialCurrency"></param>
        /// <returns></returns>
        public decimal CalculateNewAmount(decimal initialAmount)
        {
            decimal amount = initialAmount;
            string currentCurrency = InitialCurrency;

            foreach (var conversion in ConversionChainList)
            {
                amount = conversion.CalculateNewAmount(amount, currentCurrency);
                if (currentCurrency == conversion.Currency1)
                {
                    currentCurrency = conversion.Currency2;
                }
                else
                {
                    currentCurrency = conversion.Currency1;
                }
            }
            return amount;
        }

        /// <summary>
        /// Creates a new chain with the same values as the current one.
        /// </summary>
        /// <returns></returns>
        public ConversionChain CopyConversionChain()
        {
            ConversionChain newConversionChain = new ConversionChain(InitialCurrency);
            foreach (var conversion in ConversionChainList)
            {
                newConversionChain.AddConversion(conversion);
            }

            return newConversionChain;
        }
        #endregion
    }
}
