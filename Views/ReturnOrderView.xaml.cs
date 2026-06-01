using System.Windows;
using System.Windows.Controls;
using TAM.Dialogs;
using TAM.Helpers;
using TAM.Models;

namespace TAM.Views;

public partial class ReturnOrderView : UserControl
{
    public ReturnOrderView() => InitializeComponent();

    private void ViewDetail_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is ReturnOrder ret)
            new DetailDialog(DetailInfoBuilder.ForReturnOrder(ret)) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
