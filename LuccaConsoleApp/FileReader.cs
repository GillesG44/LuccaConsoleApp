namespace LuccaConsoleApp
{
    public class RequestProcessor
    {
        private const string wrongFirstLineText = "La première ligne doit avoir le format D1;M;D2 où D1 est le code à trois chiffres de la devise initiale, M est le montant de cette devise initiale sous la forme d'un nombre entier positif et D2 est le code à trois chiffres de la devise cible (ex.: EUR;50;JPY).";
        private const string wrongSecondLineText = "La deuxième ligne doit contenir le nombre de taux de change transmis.";

        public string InitialCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public uint InitialAmount { get; set; }

        internal List<CurrencyConversionRate> CurrencyConversionList { get; set; } = new List<CurrencyConversionRate>();

        List<ConversionChain> CurrencyConversionChainList { get; set; } = new List<ConversionChain>();
        public RequestProcessor(string[] splittedText)
        {
            Initialise(splittedText);
        }

        public bool Initialise(string[] splittedText)
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
                throw new InvalidOperationException("Le format de la première ligne n'est pas valide. " + wrongFirstLineText);
            }

            InitialCurrency = ReadCurrencyCode(splittedFirstLine[0]);

            if (!uint.TryParse(splittedFirstLine[1], out var initialAmount))
            {
                throw new InvalidOperationException("Le montant indiqué dans la première ligne n'est pas valide. " + wrongFirstLineText);
            }
            InitialAmount = initialAmount;
            TargetCurrency = ReadCurrencyCode(splittedFirstLine[2]);

            // If the Initial currency and target currency are the same, the amount in the initial currency should be the same in the target currency
            if (InitialCurrency == TargetCurrency)
            {
                CurrencyConversionList.Add(new CurrencyConversionRate(InitialCurrency, TargetCurrency, 1m));
                return true;
            }

            // Second Line
            // Expected content: number of following change rates

            uint numberChangeRates;
            uint.TryParse(splittedText[1], out numberChangeRates);

            if (numberChangeRates == 0)
            {
                throw new InvalidOperationException(wrongSecondLineText);
            }

            if (numberChangeRates + 2 != splittedText.Length)
            {
                throw new InvalidOperationException(wrongSecondLineText);
            }

            for (int i = 2; i < splittedText.Length; i++)
            {
                ReadConversionRateLine(splittedText[i]);
            }

            return true;

        }

        void ReadConversionRateLine(string conversionRateLine)
        {
            var splittedCconversionRateLine = conversionRateLine.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (splittedCconversionRateLine.Length != 3)
            {
                throw new InvalidOperationException();
            }

            string initialCurrency = ReadCurrencyCode(splittedCconversionRateLine[0]);
            string targetCurrency = ReadCurrencyCode(splittedCconversionRateLine[1]);
            decimal conversionRate;
            if (!decimal.TryParse(splittedCconversionRateLine[2], out conversionRate))
            {
                throw new InvalidOperationException();
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

        string ReadCurrencyCode(string currencyCode)
        {
            currencyCode = currencyCode.ToUpperInvariant();

            if (currencyCode.Length != 3)
                throw new InvalidOperationException("Tous les codes des devises doivent être definis comme des codes de 3 lettres majuscules.");

            return currencyCode;

        }

        public int CalculateBestRate()
        {
            // Initialise the conversion chains
            var initialConversions = CurrencyConversionList.Where(cc => cc.Currency1 == InitialCurrency || cc.Currency2 == InitialCurrency);

            foreach (var currencyConversion in initialConversions)
            {
                var newConversionChain = new ConversionChain(InitialCurrency);
                newConversionChain.AddConversion(currencyConversion);
                CurrencyConversionChainList.Add(newConversionChain);
            }

            foreach (var conversion in initialConversions)
                conversion.IsActive = false;

            List<ConversionChain> chainsToBeDeleted;
            List<ConversionChain> chainsToBeAdded;

            while (CurrencyConversionList.Any(cc => cc.IsActive))
            {
                chainsToBeAdded = new List<ConversionChain>();
                chainsToBeDeleted = new List<ConversionChain>();

                IEnumerable<CurrencyConversionRate> usedCurrencyConversions = new List<CurrencyConversionRate>();
                foreach (var conversionChain in CurrencyConversionChainList)
                {
                    if (conversionChain.LastCurrency == TargetCurrency)
                        continue;

                    var usedCurrencyConversionsForChain = FindNextConversions(conversionChain).ToList();

                    // Add all the conversions used by the chain to the used conversions, so that they can be reused later: only the shortestpath should be considered as valid
                    if (usedCurrencyConversionsForChain.Any())
                    {
                        usedCurrencyConversions = usedCurrencyConversions.Union(usedCurrencyConversionsForChain);

                        for (int i = usedCurrencyConversions.Count() - 1; i >= 0; i--)
                        {
                            if (i == 0)
                                conversionChain.AddConversion(usedCurrencyConversions.ElementAt(i));
                            else
                            {
                                var newChain = conversionChain.CopyConversionChain();
                                newChain.AddConversion(usedCurrencyConversions.ElementAt(i));
                                chainsToBeAdded.Add(newChain);
                            }
                        }
                    }
                    // If there are no further possible conversions for the current chain (i.e. no possibility to reach the target currency), then remove the chain from the list of options
                    else
                    {
                        chainsToBeDeleted.Add(conversionChain);
                    }
                }

                foreach (var conversionChain in chainsToBeDeleted)
                    CurrencyConversionChainList.Remove(conversionChain);

                CurrencyConversionChainList.AddRange(chainsToBeAdded);

                if (!usedCurrencyConversions.Any())
                {
                    break;
                }
                else
                {
                    foreach (var currencyConversion in usedCurrencyConversions)
                    {
                        currencyConversion.IsActive = false;
                    }
                }
            }

            decimal maxResult = 0m;

            // The following line should be included if there is a willingness to do something with the process that leads to the returned amount.
            // ex.: display all the currency conversions
            //ConversionChain selectedConversionChain = null;

            foreach (var conversionChain in CurrencyConversionChainList)
            {
                var chainResult = conversionChain.CalculateNewAmount(InitialAmount, InitialCurrency);

                if (maxResult == 0m || maxResult < chainResult)
                {
                    maxResult = chainResult;
                    
                    // The following line should be included if there is a willingness to do something with the process that leads to the returned amount.
                    // ex.: display all the currency conversions
                    //selectedConversionChain = conversionChain;
                }
            }

            return (int)Math.Round(maxResult, 0);

        }

        private List<CurrencyConversionRate> FindNextConversions(ConversionChain conversionChain)
        {
            // Find all the active Currency Conversions that have the chain last currency as one of their currencies
            var nextConversionList = CurrencyConversionList.Where(c => c.IsActive && (c.Currency1.Equals(conversionChain.LastCurrency) || c.Currency2.Equals(conversionChain.LastCurrency)));

            if (!nextConversionList.Any())
                return new List<CurrencyConversionRate>();
            else
                return nextConversionList.ToList();
        }

    }
}
