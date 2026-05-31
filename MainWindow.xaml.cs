using System.Windows;
using System.Windows.Controls;
using TAM.ViewModels;

namespace TAM;

public partial class MainWindow : Window
{
    private MainViewModel VM => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
            VM.NavigateTo(tag);
    }
}
