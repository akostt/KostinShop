using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace KostinShop.Helpers;

public static class PhoneInputHelper
{
    public static void Attach(TextBox box)
    {
        box.PreviewKeyDown += OnPreviewKeyDown;
        box.TextChanged    += OnTextChanged;
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Back or Key.Delete or Key.Left or Key.Right or Key.Tab) return;

        var c = e.Key >= Key.D0 && e.Key <= Key.D9     ? (char)('0' + (e.Key - Key.D0))
              : e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ? (char)('0' + (e.Key - Key.NumPad0))
              : '\0';

        if (c == '\0') { e.Handled = true; return; }

        var box    = (TextBox)sender;
        var digits = StripDigits(box.Text);
        if (digits.Length >= 10) { e.Handled = true; return; }

        box.Text       = Format(digits + c);
        box.CaretIndex = box.Text.Length;
        e.Handled      = true;
    }

    private static bool _formatting;

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_formatting) return;
        _formatting = true;

        var box       = (TextBox)sender;
        var digits    = StripDigits(box.Text)[..Math.Min(10, StripDigits(box.Text).Length)];
        var formatted = Format(digits);

        if (box.Text != formatted)
        {
            box.Text       = formatted;
            box.CaretIndex = formatted.Length;
        }

        _formatting = false;
    }

    private static string StripDigits(string input)
    {
        var d = Regex.Replace(input, @"\D", "");
        return d.StartsWith("7") ? d[1..] : d;
    }

    public static string Format(string digits10)
    {
        var r = "+7";
        var l = digits10.Length;
        if (l >= 1) r += $" ({digits10[..Math.Min(3, l)]}";
        if (l >= 3) r += ")";
        if (l >= 4) r += $" {digits10[3..Math.Min(6, l)]}";
        if (l >= 7) r += $"-{digits10[6..Math.Min(8, l)]}";
        if (l >= 9) r += $"-{digits10[8..Math.Min(10, l)]}";
        return r;
    }
}
