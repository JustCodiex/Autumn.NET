using Autumn.Annotations;

using MiniBudget.Persistence.Facade;

namespace MiniBudget.Actions;

[Component]
public class ViewPurchases : Action {

    private readonly IFacade facade;

    public ViewPurchases([Inject] IFacade facade) {
        this.facade = facade;
    }

    public override void Execute(string commandline) {

        Console.Clear();

        var purchases = facade.GetAllPurchases();
        for (int i = 0; i < purchases.Count; i++) {
            if (i % 15 == 0 && i > 0) {
                Console.ReadLine();
                Console.Clear();
            }
            Console.WriteLine($"{purchases[i].Date} | {purchases[i].Amount,-10} | {(string.IsNullOrEmpty(purchases[i].Description) ? "No Description" : purchases[i].Description)}");
        }

        if (purchases.Count % 15 != 0)
            Console.ReadLine();

    }

    public override bool ShouldExecute(string testString) => testString.StartsWith("purchases");

}
