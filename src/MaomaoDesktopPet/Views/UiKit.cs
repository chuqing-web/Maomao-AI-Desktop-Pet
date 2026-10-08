using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MaomaoDesktopPet.Views;

public static class UiKit
{
    public static Window CreateShell(string title, double width, double height)
    {
        return new Window
        {
            Title = title,
            Width = width,
            Height = height,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontFamily = new FontFamily("Segoe UI, Microsoft YaHei UI")
        };
    }

    public static TextBlock H1(string text) => new()
    {
        Text = text,
        FontSize = 20,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 0, 0, 12),
        Foreground = new SolidColorBrush(Color.FromRgb(40, 60, 80))
    };

    public static TextBlock P(string text) => new()
    {
        Text = text,
        FontSize = 13,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 8),
        Foreground = new SolidColorBrush(Color.FromRgb(70, 90, 110))
    };

    public static Button Btn(string text, RoutedEventHandler click)
    {
        var b = new Button
        {
            Content = text,
            Margin = new Thickness(0, 4, 8, 4),
            Padding = new Thickness(12, 6, 12, 6),
            Background = new SolidColorBrush(Color.FromRgb(120, 170, 230)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        b.Click += click;
        return b;
    }

    public static ProgressBar Bar(double value) => new()
    {
        Height = 10,
        Minimum = 0,
        Maximum = 100,
        Value = value,
        Margin = new Thickness(0, 0, 0, 8)
    };

    public static ScrollViewer Scroll(UIElement child) => new()
    {
        Content = child,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
    };
}
