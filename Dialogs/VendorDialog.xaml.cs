using System.Windows;
using TAM.Models;
using TAM.Services;

namespace TAM.Dialogs;

public partial class VendorDialog : Window
{
    private Vendor? _existing;

    public VendorDialog(Vendor? vendor = null)
    {
        InitializeComponent();
        _existing = vendor;
        if (vendor != null)
        {
            TitleText.Text = "Edit Vendor";
            NameBox.Text = vendor.Name;
            ContactBox.Text = vendor.ContactPerson;
            PhoneBox.Text = vendor.Phone;
            EmailBox.Text = vendor.Email;
            AddressBox.Text = vendor.Address;
            ActiveCheck.IsChecked = vendor.IsActive;
        }
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Vendor name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_existing == null)
        {
            var v = new Vendor
            {
                Name = NameBox.Text.Trim(),
                ContactPerson = ContactBox.Text.Trim(),
                Phone = PhoneBox.Text.Trim(),
                Email = EmailBox.Text.Trim(),
                Address = AddressBox.Text.Trim(),
                IsActive = ActiveCheck.IsChecked == true
            };
            DataService.Instance.AddVendor(v);
        }
        else
        {
            var prev = _existing.Name;
            _existing.Name = NameBox.Text.Trim();
            _existing.ContactPerson = ContactBox.Text.Trim();
            _existing.Phone = PhoneBox.Text.Trim();
            _existing.Email = EmailBox.Text.Trim();
            _existing.Address = AddressBox.Text.Trim();
            _existing.IsActive = ActiveCheck.IsChecked == true;
            DataService.Instance.UpdateVendor(_existing, $"Updated vendor details");
        }

        DialogResult = true;
    }
}
