using System.Windows;
using System.Windows.Controls;
using TAM.Dialogs;
using TAM.Helpers;
using TAM.Models;

namespace TAM.Views;

public partial class AccessoryView : UserControl
{
    public AccessoryView() => InitializeComponent();

    private void ViewDetail_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is Accessory a)
            new DetailDialog(DetailInfoBuilder.ForAccessory(a)) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
