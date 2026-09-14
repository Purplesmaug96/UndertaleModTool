using System;
using Avalonia.Controls;

namespace UndertaleModToolAvalonia;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => UpdateIndentTextBoxVisibility();
        IndentComboBox.SelectionChanged += (_, _) => UpdateIndentTextBoxVisibility();

        Closing += async (_, _) =>
        {
            if (DataContext is SettingsViewModel vm)
            {
                if (vm.MainVM.Settings.Save() is Exception ex)
                {
                    await vm.MainVM.View!.MessageDialog($"Error when saving settings file:\n{ex.Message}");
                }
            }
        };
    }
    void UpdateIndentTextBoxVisibility()
    {
        if (IndentComboBox.SelectedIndex == IndentComboBox.ItemCount - 1)
            IndentTextBox.IsVisible = true;
        else
            IndentTextBox.IsVisible = false;
    }
}
