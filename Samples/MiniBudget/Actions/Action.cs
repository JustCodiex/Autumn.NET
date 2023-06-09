namespace MiniBudget.Actions;

public abstract class Action {

    public abstract bool ShouldExecute(string testString);

    public abstract void Execute(string commandline);

}
