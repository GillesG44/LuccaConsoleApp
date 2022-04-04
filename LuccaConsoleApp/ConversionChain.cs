namespace LuccaConsoleApp
{
    internal class ConversionChain
    {
        public string InitialCurrency { get; private set; }

        /// <summary>
        /// Last currency reached with the current chain. The Last Currency is updated every time a conversion gets added to the chain.
        /// </summary>
        public string LastCurrency { get; private set; }

        public LinkedList<CurrencyConversionRate> ConversionChainList { get; set; } = new LinkedList<CurrencyConversionRate>();
        public ConversionChain(string initialCurrency)
        {
            InitialCurrency = initialCurrency;
            LastCurrency = initialCurrency;
        }

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

        public ConversionChain CopyConversionChain()
        {
            ConversionChain newConversionChain = new ConversionChain(InitialCurrency);
            foreach (var conversion in ConversionChainList)
            {
                newConversionChain.AddConversion(conversion);
            }

            return newConversionChain;
        }
    }
}
