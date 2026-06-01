using System.Windows;
using System.Windows.Controls;
using TAM.Dialogs;
using TAM.Helpers;
using TAM.Models;

namespace TAM.Views;

public partial class OutwardOrderView : UserControl
{
    public OutwardOrderView() => InitializeComponent();

    private void ViewDetail_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is OutwardOrder out1)
            new DetailDialog(DetailInfoBuilder.ForOutwardOrder(out1)) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
