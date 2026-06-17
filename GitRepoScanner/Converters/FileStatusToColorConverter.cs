using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitRepoScanner.Converters;

public class FileStatusToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Amber  = new(Color.FromRgb(0xD9, 0x77, 0x06));
    private static readonly SolidColorBrush Green  = new(Color.FromRgb(0x16, 0xA3, 0x4A));
    private static readonly SolidColorBrush Red    = new(Color.FromRgb(0xDC, 0x26, 0x26));
    private static readonly SolidColorBrush Purple = new(Color.FromRgb(0x7C, 0x3A, 0xED));
    private static readonly SolidColorBrush Gray   = new(Color.FromRgb(0x6B, 0x72, 0x80));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = (value as string ?? "").Trim();
        if (s.Contains('M')) return Amber;
        if (s.Contains('A')) return Green;
        if (s.Contains('D')) return Red;
        if (s == "??")       return Purple;
        return Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
