public enum MenuSelectionAction
{
    Confirmed,
    Back,
    Cancel
}

public readonly record struct MenuSelectionResult(MenuSelectionAction Action, int SelectedIndex);
