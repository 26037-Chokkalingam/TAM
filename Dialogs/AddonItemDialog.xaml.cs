using System.Windows;
using System.Windows.Input;

namespace TAM.Dialogs;

public partial class AddonItemDialog : Window
{
    public string ItemName { get; private set; } = string.Empty;

    public AddonItemDialog(string title, string label, string existing = "")
    {
        InitializeComponent();
        TitleText.Text = title;
        Title = title;
        LabelText.Text = label;
        NameBox.Text = existing;
        Loaded += (_, _) => { NameBox.Focus(); NameBox.SelectAll(); };
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ItemName = NameBox.Text.Trim();
        DialogResult = true;
    }

    private void NameBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) SaveBtn_Click(sender, new RoutedEventArgs());
    }
}
