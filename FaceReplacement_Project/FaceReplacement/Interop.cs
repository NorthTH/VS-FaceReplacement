using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace FaceReplacement
{
    class Interop
    {
        [System.Runtime.InteropServices.DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap source)
        {
            IntPtr Hbitmap = source.GetHbitmap(); //obtain the Hbitmap

            BitmapSource output = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Hbitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(Hbitmap); //release the HBitmap
            return output;
        }

        public static BitmapSource IImageToBitmapSource(Emgu.CV.IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr Hbitmap = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource output = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    Hbitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(Hbitmap); //release the HBitmap
                return output;
            }
        }

        public static System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                PngBitmapEncoder enc = new PngBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(source));

                enc.Save(outStream);

                return new System.Drawing.Bitmap(outStream);
            }
        }

        public static System.Drawing.PointF[] WPFPointCollectionToPointFArray(PointCollection points)
        {
            System.Drawing.PointF[] result = new System.Drawing.PointF[points.Count];
            for (int i = 0; i < points.Count; i++)
			{
                result[i] = WPFPointToPointF(points[i]);
            }
            return result;
        }

        public static System.Drawing.PointF WPFPointToPointF(Point point)
        {
            return new System.Drawing.PointF((float)point.X, (float)point.Y);
        }

        public static System.Drawing.Rectangle RectToRectangle(Rect value)
        {
            System.Drawing.Rectangle result = new System.Drawing.Rectangle();
            result.X = (int)value.X;
            result.Y = (int)value.Y;
            result.Width = (int)value.Width;
            result.Height = (int)value.Height;

            return result;
        }

        public static Rect RectangleToRect(System.Drawing.Rectangle value)
        {
            Rect result = new Rect();
            result.X = value.X;
            result.Y = value.Y;
            result.Width = value.Width;
            result.Height = value.Height;

            return result;
        }

        public static Int32Rect RectangleToInt32Rect(System.Drawing.Rectangle value)
        {
            Int32Rect result = new Int32Rect();
            result.X = value.X;
            result.Y = value.Y;
            result.Width = value.Width;
            result.Height = value.Height;

            return result;
        }

        public static Int32Rect RectToInt32Rect(Rect value)
        {
            Int32Rect result = new Int32Rect();
            result.X = (int)value.X;
            result.Y = (int)value.Y;
            result.Width = (int)value.Width;
            result.Height = (int)value.Height;

            return result;
        }
    }
}
