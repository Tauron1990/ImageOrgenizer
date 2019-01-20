using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TastImageMaker
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
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
                for (int i = 0; i < 150; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MyText.Text = (i + 1).ToString();
                        MyProgressbar.Maximum = 150;
                        MyProgressbar.Value = i;
                    });
                    Task.Delay(500).Wait();
                    Dispatcher.Invoke(() =>
                    {
                        using (FileStream outStream = new FileStream(Path.Combine(target, i + ".png"), FileMode.Create))
                        {
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(GetRender(MyDrawinfPlace, 96)));
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
