using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autumn.Annotations;

using MiniBudget.Persistence.Facade;

namespace MiniBudget;

[Service]
public class Controller {

    [Inject]
    public IFacade Facade { get; }

    [EntryPoint]
    public void HandleActions() {

        while (true) {
            Console.Clear();
            Console.WriteLine("=======================");
            Console.WriteLine("Mini Budget 2023");
            Console.Write("Action: ");

            string? action = Console.ReadLine();
            if (string.IsNullOrEmpty(action)) {
                continue;
            }

            switch (action) {
                case "purchases":
                    ViewPurchases();
                    break;
                case "buy":
                    MakePurchase();
                    break;
                default:
                    break;
            }

        }

    }

    private void ViewPurchases() {

        Console.Clear();

        var purchases = this.Facade.GetAllPurchases();

    }

    public void MakePurchase() {

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
        string date = Console.ReadLine() ?? "";
        if (!DateOnly.TryParse(date, out DateOnly parsedDate)) {
            Console.WriteLine();
            Console.WriteLine("==============");
            Console.WriteLine("Registration failed: Unable to parse date");
            Thread.Sleep(3000);
            return;
        }

        Facade.RegisterPurchase(amount, desc, parsedDate);

    }

}
