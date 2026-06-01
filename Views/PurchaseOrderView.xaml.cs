using System.Windows;
using System.Windows.Controls;
using TAM.Dialogs;
using TAM.Helpers;
using TAM.Models;

namespace TAM.Views;

public partial class PurchaseOrderView : UserControl
{
    public PurchaseOrderView() => InitializeComponent();

    private void ViewDetail_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is PurchaseOrder po)
            new DetailDialog(DetailInfoBuilder.ForPurchaseOrder(po)) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
