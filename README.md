# LuccaConsoleApp
Lucca console app


Le programme peut être exécuté à partir de la commande **LuccaDevises <chemin vers le fichier>**

Le contenu du fichier est lu et enregistré dans un string[]. Chaque ligne du fichier correspond à un élément du tableau. Les lignes vides sont ignorées.
  
Le programme commence par initialisé un object de la classe **RequestProcessor** avec la devise initiale, son montant initial, la devise d'arrivée et l'ensemble des taux de conversions entre devises (classe **CurrencyConversionRate**).
Cette initialisation est faite en analysant le string[] fournit en argument du constructeur.

Une fois l'initialisation effectuée, le programme appelle la méthode **CalculateBestRate()** de l'object **RequestProcessor**.
Cette méthode recherche le plus cours chemin de conversions avec le meilleur montant final dans la devise d'arrivée. Les chemins sont définis comme **LinkedList<ConcurrencyConversionRate>** dans la classe **ConversionChain**.

Les taux de change et montants obtenus après chaque conversion dans une nouvelle devise sont tous arrondis à 4 décimales.
Le montant retourné est un entier.
