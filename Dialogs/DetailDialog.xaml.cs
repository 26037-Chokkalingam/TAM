using System.Windows;
using System.Windows.Controls;
using TAM.Models;

namespace TAM.Dialogs;

public partial class DetailDialog : Window
{
    public DetailDialog(DetailInfo info)
    {
        InitializeComponent();
        TitleText.Text = info.Title;
        SubtitleText.Text = info.Subtitle;
        FieldsList.ItemsSource = info.Fields;
        HistoryGrid.ItemsSource = info.EditHistory;

        if (info.Items.Any())
        {
            ItemsHeaderText.Text = info.ItemsHeader;
            var cols = info.ItemColumns.Count > 0 ? info.ItemColumns : new List<string> { "Item", "Qty", "Extra", "" };
            var props = new[] { nameof(DetailItemRow.Col1), nameof(DetailItemRow.Col2), nameof(DetailItemRow.Col3), nameof(DetailItemRow.Col4) };
            for (int i = 0; i < cols.Count && i < 4; i++)
            {
                if (string.IsNullOrEmpty(cols[i])) continue;
                ItemsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = cols[i],
                    Binding = new System.Windows.Data.Binding(props[i]),
                    Width = i == 0 ? new DataGridLength(1, DataGridLengthUnitType.Star) : new DataGridLength(100)
                });
            }
            ItemsGrid.ItemsSource = info.Items;
        }
        else
        {
            ItemsPanel.Visibility = Visibility.Collapsed;
        }

        if (!info.EditHistory.Any())
            HistoryGrid.MinHeight = 40;
    }
}
