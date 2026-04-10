public class Menu
{
    public static void Run()
    {
        IScenarioProvider scenarioProvider = new FileManagerScenarioProvider();
        MenuView view = new();
        MenuController controller = new(view, scenarioProvider);
        controller.Run();
    }
}