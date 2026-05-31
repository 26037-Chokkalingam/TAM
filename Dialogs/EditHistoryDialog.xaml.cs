using System.Collections.Generic;
using System.Windows;
using TAM.Models;

namespace TAM.Dialogs;

public partial class EditHistoryDialog : Window
{
    public EditHistoryDialog(string title, List<EditHistoryEntry> history)
    {
        InitializeComponent();
        TitleText.Text = title;
        HistoryGrid.ItemsSource = history;
        if (!history.Any())
        {
            // Show empty state
            Title = title + " (No history)";
        }
    }
}
