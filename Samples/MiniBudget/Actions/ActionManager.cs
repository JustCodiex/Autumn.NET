using Autumn.Annotations;
using Autumn.Context;

namespace MiniBudget.Actions;

[Component]
public class ActionManager {

    private readonly Action[] actions;

    public ActionManager([Inject] AutumnAppContext appContext) {
        actions = appContext.GetComponents<Action>();
    }

    public void RunAction(string action) {

        for (int i = 0; i < actions.Length; i++) {
            if (actions[i].ShouldExecute(action)) {
                actions[i].Execute(action);
                return;
            }
        }

    }

}
