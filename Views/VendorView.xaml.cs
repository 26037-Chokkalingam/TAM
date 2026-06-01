using System.Windows;
using System.Windows.Controls;
using TAM.Dialogs;
using TAM.Helpers;
using TAM.Models;
using TAM.ViewModels;

namespace TAM.Views;

public partial class VendorView : UserControl
{
    public VendorView() => InitializeComponent();

    private void ViewDetail_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is Vendor v)
            new DetailDialog(DetailInfoBuilder.ForVendor(v)) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
