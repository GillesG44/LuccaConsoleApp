using LuccaConsoleApp;

string? filePath = null;

#region Initialse the path to the file if it has been passed as an argument

if (args.Length == 1)
{
    filePath = args[0];
}
else
{
    Console.WriteLine("Le programme doit être exécuté avec un seul argument: le chemin vers le fichier contenant les conversions.");
}
#endregion

bool isPathValid = File.Exists(filePath);

do
{
    #region Get the path to the file
    while (string.IsNullOrWhiteSpace(filePath) || !isPathValid)
    {
        string message = "";
        if (!isPathValid)
        {
            message ="Le chemin du fichier n'est pas valide. ";
        }

        Console.WriteLine(message + "Veuillez indiquer le chemin du fichier contenant les règles de conversions.");
        filePath = Console.ReadLine();
        
        isPathValid = string.IsNullOrWhiteSpace(filePath) || File.Exists(filePath);
    }
    #endregion

    var fileContent = File.ReadAllText(filePath);

    // The file content is splitted into a table 
    string[] splittedText = fileContent.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    try
    {
        // This is where the file content is parsed and processed.
        // The parsing happens in the RequestProcessor constuctor. By the end of the process, all the relevant possible conversions have been listed
        // and process is initialised with the initial and target currencies.
        var requestProcessor = new RequestProcessor(splittedText);
        
        // The call to the CalculateBestRate method is used to calculate the best amount that can be achieved in the target currency when starting with the initial one.
        var calculatedBestRate = requestProcessor.CalculateBestRate();
 
        // CalculateBestRate returns an integer which is displayed in the Console.
        Console.WriteLine(calculatedBestRate);
    }
    catch (Exception exception)
    {
        Console.WriteLine(exception.Message);
    }

    #region Give the opportunity to the user to continue using the programme
    Console.WriteLine("Voulez-vous quitter le programme? Pressez Y pour quitter, ou n'importe quelle autre touche pour continuer.");

    char pressedKey = Console.ReadKey().KeyChar;
    if (pressedKey == 'Y' || pressedKey == 'y')
        break;

    #endregion

    // Reinitialise the variable values for the next iteration of the programme
    filePath = null;
    isPathValid = true;

}
while (true);


