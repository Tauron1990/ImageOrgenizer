using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Syncfusion.Licensing;
using Syncfusion.UI.Xaml.Grid;

namespace TestWpfApp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            SyncfusionLicenseProvider.RegisterLicense("MjcwMTFAMzEzNjJlMzMyZTMwUy9NTnVGSU9scHNKNW0rSU51VG1ON3ZBQ2ozWGpqZXUwKzJ2RitJM1pzTT0=");
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string target = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (Directory.Exists(target))
                Directory.Delete(target, true);
            Directory.CreateDirectory(target);

            Task.Run(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    Dispatcher.Invoke(() => myText.Text = (i + 1).ToString());
                    Task.Delay(1000).Wait();
                    Dispatcher.Invoke(() =>
                    {
                        using (FileStream outStream = new FileStream(Path.Combine(target, i + ".png"), FileMode.Create))
                        {
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(GetRender(myCanvas, 96)));
                            encoder.Save(outStream);
                        }
                    });
                }
            });
        }

        public static BitmapSource GetRender(UIElement source, double dpi)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(source);

            var scale = dpi / 96.0;
            var width = (bounds.Width + bounds.X) * scale;
            var height = (bounds.Height + bounds.Y) * scale;

            RenderTargetBitmap rtb =
                new RenderTargetBitmap((int)Math.Round(width, MidpointRounding.AwayFromZero),
                    (int)Math.Round(height, MidpointRounding.AwayFromZero),
                    dpi, dpi, PixelFormats.Pbgra32);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(source);
                ctx.DrawRectangle(vb, null,
                    new Rect(new Point(bounds.X, bounds.Y), new Point(width, height)));
            }

            rtb.Render(dv);
            return (BitmapSource)rtb.GetAsFrozen();
        }
    }
}
