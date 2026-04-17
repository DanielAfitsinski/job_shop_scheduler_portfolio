namespace Job_Shop_Scheduler_Portfolio.UI.Menu.Models;

// Possible outcomes for a menu selection dialog
public enum MenuSelectionAction
{
    // The user confirmed the current selection
    Confirmed,
    // The user moved back to the previous screen
    Back,
    // The user cancelled the dialog entirely
    Cancel
}

// Captures both the dialog outcome and the selected index
public readonly record struct MenuSelectionResult(MenuSelectionAction Action, int SelectedIndex);