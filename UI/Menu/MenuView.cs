using Terminal.Gui;

public class MenuView
{
    public static void Initialise()
    {
        Application.Init();
    }

    public static void ShowMainScreen(Action onRunSelected, Action onExitSelected)
    {
        Window window = CreateMainWindow();
        Button runButton = CreateButton("_Run Job Shop Scheduler", onRunSelected, isDefault: false);
        runButton.X = Pos.Center();
        runButton.Y = 3;
        runButton.Width = 30;

        Button exitButton = CreateButton("_Exit", onExitSelected, isDefault: false);
        exitButton.X = Pos.Center();
        exitButton.Y = 5;
        exitButton.Width = 30;

        window.Add(CreateSubtitleLabel(), runButton, exitButton);
        Application.Top.Add(CreateMenuBar(onRunSelected, onExitSelected), window, CreateStatusBar(onRunSelected, onExitSelected));
    }

    public static void RunMainLoop()
    {
        Application.Run();
    }

    public static void Shutdown()
    {
        Application.Shutdown();
    }

    public MenuSelectionResult PromptSelection(
        string title,
        string[] options,
        string confirmButtonText,
        string cancelButtonText,
        MenuSelectionAction cancelAction,
        int width,
        int height,
        int defaultSelection = -1)
    {
        MenuSelectionResult result = new(MenuSelectionAction.Cancel, -1);

        ListView listView = new(options)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        if (defaultSelection >= 0 && defaultSelection < options.Length)
        {
            listView.SelectedItem = defaultSelection;
        }

        Dialog dialog = new(title, width, height);
        dialog.Add(listView);

        Button confirmButton = CreateButton(confirmButtonText, () =>
        {
            if (!TryGetSelectionIndex(listView.SelectedItem, options.Length, out int selectedIndex))
            {
                ShowError("Invalid Selection", "Please select an option.");
                return;
            }

            result = new(MenuSelectionAction.Confirmed, selectedIndex);
            Application.RequestStop(dialog);
        }, isDefault: true);

        Button cancelButton = CreateButton(cancelButtonText, () =>
        {
            result = new(cancelAction, -1);
            Application.RequestStop(dialog);
        }, isDefault: false);

        dialog.AddButton(confirmButton);
        dialog.AddButton(cancelButton);

        Application.Run(dialog);
        return result;
    }

    public MenuSelectionAction PromptConfirmation(
        string title,
        string message,
        string confirmButtonText,
        string backButtonText,
        int width,
        int height)
    {
        MenuSelectionAction action = MenuSelectionAction.Back;

        Dialog dialog = new(title, width, height);
        Label summary = new(message)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        dialog.Add(summary);

        Button confirmButton = CreateButton(confirmButtonText, () =>
        {
            action = MenuSelectionAction.Confirmed;
            Application.RequestStop(dialog);
        }, isDefault: true);

        Button backButton = CreateButton(backButtonText, () =>
        {
            action = MenuSelectionAction.Back;
            Application.RequestStop(dialog);
        }, isDefault: false);

        dialog.AddButton(confirmButton);
        dialog.AddButton(backButton);

        Application.Run(dialog);
        return action;
    }

    public static void ShowError(string title, string message)
    {
        MessageBox.ErrorQuery(50, 7, title, message, "OK");
    }

    public static void ShowInfo(string title, string message)
    {
        MessageBox.Query(70, 10, title, message, "OK");
    }

    public static void RequestStop()
    {
        Application.RequestStop();
    }

    private static Window CreateMainWindow()
    {
        return new Window("Job Shop Scheduler")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
    }

    private static Label CreateSubtitleLabel()
    {
        return new Label("Select an option to continue")
        {
            X = Pos.Center(),
            Y = 1
        };
    }

    private static MenuBar CreateMenuBar(Action onRunSelected, Action onExitSelected)
    {
        return new MenuBar([
            new MenuBarItem("_File", [
                new MenuItem("_Run", "Select schedule and algorithm", onRunSelected),
                new MenuItem("_Quit", "Exit application", onExitSelected)
            ])
        ]);
    }

    private static StatusBar CreateStatusBar(Action onRunSelected, Action onExitSelected)
    {
        return new StatusBar([
            new StatusItem(Key.F5, "~F5~ Run", onRunSelected),
            new StatusItem(Key.Q | Key.CtrlMask, "~Ctrl+Q~ Quit", onExitSelected)
        ]);
    }

    private static Button CreateButton(string text, Action onClick, bool isDefault)
    {
        Button button = new(text)
        {
            IsDefault = isDefault
        };
        button.Clicked += onClick;
        return button;
    }

    private static bool TryGetSelectionIndex(int selectedItem, int itemCount, out int selectedIndex)
    {
        if (selectedItem >= 0 && selectedItem < itemCount)
        {
            selectedIndex = selectedItem;
            return true;
        }

        selectedIndex = -1;
        return false;
    }
}
