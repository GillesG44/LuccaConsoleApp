using LuccaConsoleApp;

var textEntry = @"EUR; 550; JPY
6
AUD; CHF; 0.9661
JPY; KRW; 13.1151
EUR; CHF; 1.2053
AUD; JPY; 86.0305
EUR; USD; 1.2989
JPY; INR; 0.6571";

string[] splittedText = textEntry.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
try
{
    var requestProcessor = new RequestProcessor(splittedText);
    var calculatedBestRate = requestProcessor.CalculateBestRate();
    Console.WriteLine(calculatedBestRate);
}
catch (Exception exception)
{
    Console.WriteLine(exception.Message);
}

