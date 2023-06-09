using System.Globalization;

using Autumn.Annotations;

using MiniBudget.Persistence.Facade;

namespace MiniBudget.Actions;

[Component]
public class BuyAction : Action {
    
    private readonly IFacade facade;

    public BuyAction([Inject] IFacade facade) {
        this.facade = facade;
    }

    public override void Execute(string commandline) {

        Console.WriteLine();
        Console.Write("Amount:                       ");
        string amountStr = Console.ReadLine() ?? "0";
        if (!decimal.TryParse(amountStr, CultureInfo.InvariantCulture, out decimal amount)) {
            Console.WriteLine();
            Console.WriteLine("==============");
            Console.WriteLine("Registration failed: Unable to parse amount");
            Thread.Sleep(3000);
            return;
        }

        Console.Write("Description:                  ");
        string desc = Console.ReadLine() ?? "";

        Console.Write("Date (Leave empty for today): ");
        string dateStr = Console.ReadLine() ?? "";
        DateOnly date;
        if (string.IsNullOrEmpty(dateStr)) {
            date = DateOnly.FromDateTime(DateTime.Now);
        } else if (!DateOnly.TryParse(dateStr, CultureInfo.InvariantCulture, out date)) {
            Console.WriteLine();
            Console.WriteLine("==============");
            Console.WriteLine("Registration failed: Unable to parse date");
            Thread.Sleep(3000);
            return;
        }

        facade.RegisterPurchase(amount, desc, date);

    }

    public override bool ShouldExecute(string testString) => testString.StartsWith("buy");

}
