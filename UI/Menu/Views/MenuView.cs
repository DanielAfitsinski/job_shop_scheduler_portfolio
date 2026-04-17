namespace Job_Shop_Scheduler_Portfolio.UI.Menu.Views;

using Terminal.Gui;
using Job_Shop_Scheduler_Portfolio.UI.Menu.Models;

// TerminalGui wrapper for all menu dialogs and screens
public class MenuView
{
    // Initialises the TerminalGui application lifecycle
    public static void Initialise()
    {
        Application.Init();
    }

    // Builds the main landing screen with run and exit actions
    public static void ShowMainScreen(Action onRunSelected, Action onExitSelected)
    {
        // Create the primary window for the application
        Window window = CreateMainWindow();
        // Create the run button that opens the workflow
        Button runButton = CreateButton("_Run Job Shop Scheduler", onRunSelected, isDefault: false);
        runButton.X = Pos.Center();
        runButton.Y = 3;
        runButton.Width = 30;

        // Create the exit button that closes the application
        Button exitButton = CreateButton("_Exit", onExitSelected, isDefault: false);
        exitButton.X = Pos.Center();
        exitButton.Y = 5;
        exitButton.Width = 30;

        // Add the visible controls to the window
        window.Add(CreateSubtitleLabel(), runButton, exitButton);
        Application.Top.Add(window);
    }

    // Starts the main UI loop
    public static void RunMainLoop()
    {
        Application.Run();
    }

    // Shuts down TerminalGui cleanly
    public static void Shutdown()
    {
        Application.Shutdown();
    }

    // Shows a selection dialog with confirm and cancel actions
    public MenuSelectionResult PromptSelection(
        string title,
        string[] options,
        string confirmButtonText,
        string cancelButtonText,
        MenuSelectionAction cancelAction,
        int width,
        int height,
        int defaultSelection = -1,
        string? detailsText = null)
    {
        // Default to a cancelled dialog result until the user confirms
        MenuSelectionResult result = new(MenuSelectionAction.Cancel, -1);

        // Reserve space for optional details above the option list
        int listTop = 0;
        Label? detailsLabel = null;
        if (!string.IsNullOrWhiteSpace(detailsText))
        {
            // Render extra context text when provided
            detailsLabel = new Label(detailsText)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 4
            };
            listTop = 4;
        }

        // Present the available choices in a list
        ListView listView = new(options)
        {
            X = 0,
            Y = listTop,
            Width = Dim.Fill(),
            Height = Dim.Fill(listTop + 1)
        };

        // Preselect the requested default option if it is valid
        if (defaultSelection >= 0 && defaultSelection < options.Length)
        {
            listView.SelectedItem = defaultSelection;
        }

        // Create the dialog shell that hosts the list
        Dialog dialog = new(title, width, height);
        if (detailsLabel is not null)
        {
            dialog.Add(detailsLabel);
        }

        dialog.Add(listView);

        // Confirm the current selection if the item index is valid
        Button confirmButton = CreateButton(confirmButtonText, () =>
        {
            if (!TryGetSelectionIndex(listView.SelectedItem, options.Length, out int selectedIndex))
            {
                // Keep the dialog open until a valid selection is made
                ShowError("Invalid Selection", "Please select an option.");
                return;
            }

            result = new(MenuSelectionAction.Confirmed, selectedIndex);
            Application.RequestStop(dialog);
        }, isDefault: true);

        // Cancel returns to the caller with the requested cancel action
        Button cancelButton = CreateButton(cancelButtonText, () =>
        {
            result = new(cancelAction, -1);
            Application.RequestStop(dialog);
        }, isDefault: false);

        dialog.AddButton(confirmButton);
        dialog.AddButton(cancelButton);

        // Run the modal dialog and return the chosen result
        Application.Run(dialog);
        return result;
    }

    // Shows a simple confirmation dialog with confirm and back actions
    public MenuSelectionAction PromptConfirmation(
        string title,
        string message,
        string confirmButtonText,
        string backButtonText,
        int width,
        int height)
    {
        // Back is the default result unless the user confirms
        MenuSelectionAction action = MenuSelectionAction.Back;

        // Build the dialog with the supplied message
        Dialog dialog = new(title, width, height);
        Label summary = new(message)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        dialog.Add(summary);

        // Confirm and continue with the selected configuration
        Button confirmButton = CreateButton(confirmButtonText, () =>
        {
            action = MenuSelectionAction.Confirmed;
            Application.RequestStop(dialog);
        }, isDefault: true);

        // Back returns to the previous screen
        Button backButton = CreateButton(backButtonText, () =>
        {
            action = MenuSelectionAction.Back;
            Application.RequestStop(dialog);
        }, isDefault: false);

        dialog.AddButton(confirmButton);
        dialog.AddButton(backButton);

        // Run the modal confirmation dialog and return the result
        Application.Run(dialog);
        return action;
    }

    // Shows a modal error dialog
    public static void ShowError(string title, string message)
    {
        MessageBox.ErrorQuery(50, 7, title, message, "OK");
    }

    // Shows a modal informational dialog
    public static void ShowInfo(string title, string message, int width = 70, int height = 10)
    {
        MessageBox.Query(width, height, title, message, "OK");
    }

    // Requests that the UI stop running
    public static void RequestStop()
    {
        Application.RequestStop();
    }

    // Creates the main application window
    private static Window CreateMainWindow()
    {
        return new Window("Job Shop Scheduler")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
    }

    // Creates the subtitle displayed on the landing screen
    private static Label CreateSubtitleLabel()
    {
        return new Label("Select an option to continue")
        {
            X = Pos.Center(),
            Y = 1
        };
    }

    // Creates a reusable button with the requested click handler
    private static Button CreateButton(string text, Action onClick, bool isDefault)
    {
        Button button = new(text)
        {
            IsDefault = isDefault
        };
        button.Clicked += onClick;
        return button;
    }

    // Validates the selected item index before confirming a choice
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
