using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MaomaoDesktopPet.Core;

public static class PlaceholderFactory
{
    public static ImageSource Create(PetState state)
    {
        const int size = 200;
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, size, size));

            var body = new RadialGradientBrush(
                Color.FromRgb(255, 255, 255),
                Color.FromRgb(230, 236, 245));
            dc.DrawEllipse(body, null, new Point(100, 115), 55, 48);

            dc.DrawEllipse(Brushes.White, null, new Point(70, 70), 18, 28);
            dc.DrawEllipse(Brushes.White, null, new Point(130, 70), 18, 28);
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(255, 182, 193)), null, new Point(70, 58), 8, 10);
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(255, 182, 193)), null, new Point(130, 58), 8, 10);

            var eyeColor = state switch
            {
                PetState.Sleep => Color.FromRgb(120, 140, 160),
                PetState.Click => Color.FromRgb(80, 140, 255),
                PetState.Drag => Color.FromRgb(100, 160, 255),
                _ => Color.FromRgb(70, 130, 230)
            };
            var eyeBrush = new SolidColorBrush(eyeColor);

            if (state == PetState.Sleep)
            {
                var pen = new Pen(eyeBrush, 3);
                dc.DrawLine(pen, new Point(78, 105), new Point(92, 105));
                dc.DrawLine(pen, new Point(108, 105), new Point(122, 105));
            }
            else
            {
                dc.DrawEllipse(eyeBrush, null, new Point(85, 105), 8, 10);
                dc.DrawEllipse(eyeBrush, null, new Point(115, 105), 8, 10);
                dc.DrawEllipse(Brushes.White, null, new Point(82, 101), 3, 3);
                dc.DrawEllipse(Brushes.White, null, new Point(112, 101), 3, 3);
            }

            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(255, 160, 180)), null, new Point(100, 118), 4, 3);

            var scarf = new SolidColorBrush(Color.FromRgb(100, 160, 230));
            dc.DrawRoundedRectangle(scarf, null, new Rect(70, 135, 60, 14), 6, 6);

            var label = state switch
            {
                PetState.Walk => "走走",
                PetState.Sleep => "Zzz",
                PetState.Drag => "抓住!",
                PetState.Click => "嗯?",
                _ => "毛毛"
            };

            var text = new FormattedText(
                label,
                CultureInfo.GetCultureInfo("zh-CN"),
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                14,
                new SolidColorBrush(Color.FromRgb(90, 110, 140)),
                1.25);
            dc.DrawText(text, new Point((size - text.Width) / 2, 165));
        }

        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
