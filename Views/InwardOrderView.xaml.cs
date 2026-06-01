using System.Windows;
using System.Windows.Controls;
using TAM.Dialogs;
using TAM.Helpers;
using TAM.Models;

namespace TAM.Views;

public partial class InwardOrderView : UserControl
{
    public InwardOrderView() => InitializeComponent();

    private void ViewDetail_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is InwardOrder inw)
            new DetailDialog(DetailInfoBuilder.ForInwardOrder(inw)) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
