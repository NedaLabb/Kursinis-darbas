using ScannerSimulator;

using var sender = new EventSender();

var scenario = new TestScenario();
var automatic = new AutomaticSimulator();
var history = new HistoryScenario();

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Siųsti 30 įėjimų ir 5 išėjimus");
    Console.WriteLine("2. Automatinis generavimas");
    Console.WriteLine("3. Generuoti savaitės istoriją");
    Console.WriteLine("4. Pakartoti testinio scenarijaus įvykį");
    Console.WriteLine("0. Baigti");
    Console.Write("Pasirinkimas: ");

    var choice = Console.ReadLine();

    if (choice == "0")
    {
        break;
    }
    else if (choice == "1")
    {
        scenario.Run(sender);
    }
    else if (choice == "2")
    {
        Console.WriteLine("1. Centro klubas");
        Console.WriteLine("2. Parko klubas");
        Console.WriteLine("3. Šiaurės klubas");
        Console.WriteLine("0. Grįžti į meniu");
        Console.Write("Pasirinkite klubą: ");

        var clubChoice = Console.ReadLine();

        if (clubChoice == "0")
        {
            continue;
        }
        else if (clubChoice == "1")
        {
            automatic.Run(sender, "I001", "K001");
        }
        else if (clubChoice == "2")
        {
            automatic.Run(sender, "I001", "K002");
        }
        else if (clubChoice == "3")
        {
            automatic.Run(sender, "I002", "K003");
        }
        else
        {
            Console.WriteLine("Tokio klubo nėra.");
        }
    }
    else if (choice == "3")
    {
        history.Run(sender);
    }
    else if (choice == "4")
    {
        scenario.RepeatEvent(sender);
    }
    else
    {
        Console.WriteLine("Tokio pasirinkimo nėra.");
    }
}