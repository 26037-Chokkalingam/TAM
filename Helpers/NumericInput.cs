using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TAM.Helpers;

public static class NumericInput
{
    private static readonly Regex _valid = new(@"^\d*\.?\d*$");

    public static readonly DependencyProperty IsDecimalProperty =
        DependencyProperty.RegisterAttached("IsDecimal", typeof(bool), typeof(NumericInput),
            new PropertyMetadata(false, OnChanged));

    public static bool GetIsDecimal(DependencyObject obj) => (bool)obj.GetValue(IsDecimalProperty);
    public static void SetIsDecimal(DependencyObject obj, bool value) => obj.SetValue(IsDecimalProperty, value);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;
        if ((bool)e.NewValue)
        {
            tb.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(tb, OnPaste);
        }
        else
        {
            tb.PreviewTextInput -= OnPreviewTextInput;
            DataObject.RemovePastingHandler(tb, OnPaste);
        }
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var proposed = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                              .Insert(tb.SelectionStart, e.Text);
        e.Handled = !_valid.IsMatch(proposed);
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(typeof(string))) { e.CancelCommand(); return; }
        var pasted = (string)e.DataObject.GetData(typeof(string))!;
        if (sender is TextBox tb)
        {
            var proposed = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                                  .Insert(tb.SelectionStart, pasted);
            if (!_valid.IsMatch(proposed)) e.CancelCommand();
        }
    }
}
