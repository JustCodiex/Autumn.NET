using Autumn.Annotations;

using MiniBudget.Actions;

namespace MiniBudget;

[Service]
public class Controller {

    [Inject] ActionManager ActionManager { get; }

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

            ActionManager.RunAction(action);

        }

    }

}
