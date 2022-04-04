namespace LuccaConsoleApp
{
    /// <summary>
    /// The requestProcessor class holds all the relevant information for every request:
    /// - The initial and targetted currencies
    /// - The initial amount in the initial currency
    /// - The conversions available and their rates
    /// </summary>
    public class RequestProcessor
    {
        #region Constants
        private const string wrongFirstLineText = "La première ligne doit avoir le format D1;M;D2 où D1 est le code à trois chiffres de la devise initiale, M est le montant de cette devise initiale sous la forme d'un nombre entier positif et D2 est le code à trois chiffres de la devise cible (ex.: EUR;50;JPY).";
        private const string wrongSecondLineText = "La deuxième ligne doit contenir le nombre de taux de change transmis.";
        private const string wrongConversionLine = "Une ligne de conversion doit avoir le format DD;DA;T où DD est le code à trois chiffres de la devise initiale DA est le code à trois chiffres de la devise d'arrivée, et T est le taux de change souls la form d'un nombre à 4 décimale qui utilise '.' comme séparateur (ex.: EUR;CHF;1.2053).";
        #endregion

        #region Properties
        public string InitialCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public uint InitialAmount { get; set; }

        internal List<CurrencyConversionRate> CurrencyConversionList { get; set; } = new List<CurrencyConversionRate>();

        List<ConversionChain> CurrencyConversionChainList { get; set; } = new List<ConversionChain>();

        #endregion

        #region Constructors
#pragma warning disable CS8618 // Warning disabled for non-nullable fields InitialCurrency and TargetCurrency, which are initialised in the Initialise method.
        public RequestProcessor(string[] splittedText)
#pragma warning restore CS8618
        {
            Initialise(splittedText);
        }
        #endregion

        #region Public Methods
        public int CalculateBestRate()
        {
            // Initialise the conversion chains by initiating a chain with all the conversions that contain the InitialCurrency
            var initialConversions = CurrencyConversionList.Where(cc => cc.Currency1 == InitialCurrency || cc.Currency2 == InitialCurrency);

            foreach (var currencyConversion in initialConversions)
            {
                var newConversionChain = new ConversionChain(InitialCurrency);
                newConversionChain.AddConversion(currencyConversion);
                CurrencyConversionChainList.Add(newConversionChain);
            }

            // Once a conversion has been used, it should be deactivated as only the shorttest chain should be used to calculate the final amount.
            foreach (var conversion in initialConversions)
                conversion.IsActive = false;

            // The chains of conversion are built through iterations while the target currency hasn't been reached and there are still available conversions.
            // Each iteration adds one conversion to all the chains
            // The chains that cannot reach the target currency (no further available conversions) are deleted at every iteration.
            // New chains may be added may an existing chain has several available conversion to grow it: at the end of evey iteration, there should be one chain per available conversion.

            // The addition and deletion of chains is done after every iteration. Those variables store the temporary chains that will be actionned at the end of the iteration.
            List<ConversionChain> chainsToBeDeletedAfterIteration;
            List<ConversionChain> chainsToBeAddedAfterIteration;

            while (CurrencyConversionList.Any(cc => cc.IsActive)
                    && !CurrencyConversionChainList.Any(ccc => ccc.LastCurrency == TargetCurrency))
            {
                chainsToBeAddedAfterIteration = new List<ConversionChain>();
                chainsToBeDeletedAfterIteration = new List<ConversionChain>();

                // The currency conversions used during an iteration should be deactivated at the end of the iteration to ensure they are not reused
                IEnumerable<CurrencyConversionRate> currencyConversionsUsedDuringIteration = new HashSet<CurrencyConversionRate>();

                foreach (var conversionChain in CurrencyConversionChainList)
                {
                    //Find all the conversions available for the current chain
                    var nextAvailableConversionsForChain = FindNextConversions(conversionChain);

                    if (nextAvailableConversionsForChain.Any())
                    {
                        // Add all the conversions used by the chain to the set of used conversions, so that they cannot be reused later: only the shortestpath should be considered as valid
                        currencyConversionsUsedDuringIteration = currencyConversionsUsedDuringIteration.Union(nextAvailableConversionsForChain);

                        for (int i = nextAvailableConversionsForChain.Count - 1; i >= 0; i--)
                        {
                            if (i == 0)
                                conversionChain.AddConversion(nextAvailableConversionsForChain.ElementAt(i));
                            else
                            {
                                var newChain = conversionChain.CopyConversionChain();
                                newChain.AddConversion(nextAvailableConversionsForChain.ElementAt(i));
                                chainsToBeAddedAfterIteration.Add(newChain);
                            }
                        }
                    }
                    // If there are no further possible conversions for the current chain (i.e. no possibility to reach the target currency), then remove the chain from the list of options
                    else
                    {
                        chainsToBeDeletedAfterIteration.Add(conversionChain);
                    }
                }

                foreach (var conversionChain in chainsToBeDeletedAfterIteration)
                    CurrencyConversionChainList.Remove(conversionChain);

                CurrencyConversionChainList.AddRange(chainsToBeAddedAfterIteration);

                if (!currencyConversionsUsedDuringIteration.Any())
                {
                    break;
                }
                else
                {
                    foreach (var currencyConversion in currencyConversionsUsedDuringIteration)
                    {
                        currencyConversion.IsActive = false;
                    }
                }
            }

            // Remove all the chains of conversions that don't lead to the target currency
            CurrencyConversionChainList.RemoveAll(ccc => ccc.LastCurrency != TargetCurrency);

            if (CurrencyConversionChainList.Count == 0)
                throw new ArgumentException("La devise cible ne peut etre atteinte à partir de la devise initiale avec les conversions disponibles.");

            decimal maxResult = 0m;

            // Looks for the optimal conversion chain
            foreach (var conversionChain in CurrencyConversionChainList)
            {
                var chainResult = conversionChain.CalculateNewAmount(InitialAmount);

                if (maxResult == 0m || maxResult < chainResult)
                {
                    maxResult = chainResult;
                }
            }

            return (int)Math.Round(maxResult, 0);

        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Initialise the Request processor by reading and validting the lines passed as argument
        /// </summary>
        /// <param name="splittedText"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private void Initialise(string[] splittedText)
        {
            // Even though the expected format when reading a file is that no space characters should be included, user errors or hidden characters after copy/paste operations could lead to leading/trailing characters.
            // As a consequence, every line and unique entry should be trimmed.
            // Also, the expected formating is that there should be no empty lines. To protect against user errors, empty lines are ignored instead of throwing an Exception.

            // First line
            // Expected format: "D1;M;D2"

            if (splittedText.Length < 2)
            {
                throw new ArgumentException("Le fichier doit contenir au moins deux lignes. " + wrongFirstLineText + " " + wrongSecondLineText);
            }

            string[] splittedFirstLine = splittedText[0].Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (splittedFirstLine.Length != 3)
            {
                throw new ArgumentException("Le format de la première ligne n'est pas valide. " + wrongFirstLineText);
            }

            InitialCurrency = ReadCurrencyCode(splittedFirstLine[0]);

            if (!uint.TryParse(splittedFirstLine[1], out var initialAmount))
            {
                throw new ArgumentException("Le montant indiqué dans la première ligne n'est pas valide. " + wrongFirstLineText);
            }
            InitialAmount = initialAmount;
            TargetCurrency = ReadCurrencyCode(splittedFirstLine[2]);

            // If the Initial currency and target currency are the same, the amount in the initial currency should be the same in the target currency
            if (InitialCurrency == TargetCurrency)
            {
                CurrencyConversionList.Add(new CurrencyConversionRate(InitialCurrency, TargetCurrency, 1m));
                return;
            }

            // Second Line
            // Expected content: number of following change rates

            uint numberChangeRates;
            uint.TryParse(splittedText[1], out numberChangeRates);

            if (numberChangeRates == 0
                || numberChangeRates + 2 != splittedText.Length)
            {
                throw new ArgumentException(wrongSecondLineText);
            }

            for (int i = 2; i < splittedText.Length; i++)
            {
                ReadConversionRateLine(splittedText[i]);
            }
        }

        /// <summary>
        /// Read a conversion rate line
        /// </summary>
        /// <param name="conversionRateLine"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ReadConversionRateLine(string conversionRateLine)
        {
            var splittedCconversionRateLine = conversionRateLine.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (splittedCconversionRateLine.Length != 3)
            {
                throw new ArgumentException(String.Format("The line '{0}' n'est pas valide. ", conversionRateLine) + wrongConversionLine);
            }

            string initialCurrency = ReadCurrencyCode(splittedCconversionRateLine[0]);
            string targetCurrency = ReadCurrencyCode(splittedCconversionRateLine[1]);

            // Conversions where the initial currency matches the target currency are ignored.
            if (initialCurrency == targetCurrency)
                return;

            decimal conversionRate;
            if (!decimal.TryParse(splittedCconversionRateLine[2], System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite,
                                    System.Globalization.CultureInfo.InvariantCulture, out conversionRate))
            {
                throw new ArgumentException(String.Format("Le taux de change indiqué dans la ligne '{0}' n'est pas valide. ", conversionRate) + wrongConversionLine);
            }

            // Check whether the conversion has already been defined. If it has, update the rates. This prevents duplicates and ensure the best conversion rates are always applied.
            // If no similar conversion (same currencies) has already been added to the CurrencyConversionList, add a new one with the current values.
            var duplicateCurrencyConversion = CurrencyConversionList.FirstOrDefault(cr => (cr.Currency1 == initialCurrency && cr.Currency2 == targetCurrency)
                                                    || (cr.Currency2 == targetCurrency && cr.Currency1 == initialCurrency));
            if (duplicateCurrencyConversion != null)
            {
                if (duplicateCurrencyConversion.Currency1 == initialCurrency)
                {
                    duplicateCurrencyConversion.Currency1ToCurrency2Rate = conversionRate;
                }
                else
                {
                    duplicateCurrencyConversion.Currency2ToCurrency1Rate = conversionRate;
                }
            }
            else
            {
                CurrencyConversionList.Add(new CurrencyConversionRate(initialCurrency, targetCurrency, conversionRate));
            }
        }

        /// <summary>
        /// Read a currency code and ensure it has the right format
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        string ReadCurrencyCode(string currencyCode)
        {
            currencyCode = currencyCode.ToUpperInvariant();

            if (currencyCode.Length != 3)
                throw new ArgumentException("Tous les codes des devises doivent être definis comme des codes de 3 lettres majuscules.");

            return currencyCode;

        }


        /// <summary>
        /// Find all the available conversions for a chain among active (i.e. unused) conversions
        /// </summary>
        /// <param name="conversionChain"></param>
        /// <returns></returns>
        private List<CurrencyConversionRate> FindNextConversions(ConversionChain conversionChain)
        {
            var nextConversionList = CurrencyConversionList.Where(c => c.IsActive && (c.Currency1.Equals(conversionChain.LastCurrency) || c.Currency2.Equals(conversionChain.LastCurrency)));

            if (!nextConversionList.Any())
                return new List<CurrencyConversionRate>();
            else
                return nextConversionList.ToList();
        }

        #endregion
    }
}
