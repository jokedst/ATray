using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ATray.Tools
{
    public static  class BitmapTools
    {
        public static void ApplyPerPixel(this Bitmap bmp, Action<IntPtr> modifier)
        {
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
                throw new Exception("Ahhh my icon has changed format");
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            var bitsPerPixel = Image.GetPixelFormatSize(bmpData.PixelFormat);
            var bytesPerPixel = (bitsPerPixel + 7) / 8;
            var unusedBytes = bmpData.Stride - (bmpData.Width * bytesPerPixel);

            unsafe
            {
                var scan0 = (byte*)bmpData.Scan0.ToPointer();
                var pixelPointer = scan0;
                for (var i = 0; i < bmpData.Height; ++i)
                {
                    for (var j = 0; j < bmpData.Width; ++j)
                    {
                        modifier((IntPtr) pixelPointer);
                        pixelPointer += bytesPerPixel;
                    }

                    pixelPointer += unusedBytes;
                }
            }
            bmp.UnlockBits(bmpData);
        }
    }
}
