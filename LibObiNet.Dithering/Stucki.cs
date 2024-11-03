using System;
using System.Drawing;

namespace LibObiNet.Dithering
{

    //Suitable for 4, 2, 1 bits per pixel
    public static class Stucki
    {
        public static Bitmap Dither(Bitmap colorBitmap, int bpp)
        {
            Bitmap grayImage = ObiUtils.ConvertToGrayscale(colorBitmap);
            int width = grayImage.Width;
            int height = grayImage.Height;
            Bitmap ditheredImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get the old pixel value
                    Color oldPixel = grayImage.GetPixel(x, y);
                    // Calculate the new pixel value (nearest 4-bit grayscale)
                    int newPixelValue = oldPixel.R;
                    if (bpp == 4)
                    {
                        newPixelValue = ObiUtils.To4Bits(oldPixel.R) * ObiUtils.From4Bits;
                    }
                    else if (bpp == 2)
                    {
                        newPixelValue = ObiUtils.To2Bits(oldPixel.R) * ObiUtils.From2Bits;
                    }
                    else if (bpp == 1)
                    {
                        newPixelValue = ObiUtils.To1Bit(oldPixel.R) * ObiUtils.From1Bit;
                    }
                    Color newPixel = Color.FromArgb(newPixelValue, newPixelValue, newPixelValue);
                    ditheredImage.SetPixel(x, y, newPixel);

                    // Calculate quantization error
                    int quantError = oldPixel.R - newPixelValue;

                    // Distribute the error to the neighboring pixels
                    if (x + 1 < width)
                        grayImage.SetPixel(x + 1, y, ClampColor(grayImage.GetPixel(x + 1, y).R + (quantError * 7 / 42)));
                    if (x - 1 >= 0 && y + 1 < height)
                        grayImage.SetPixel(x - 1, y + 1, ClampColor(grayImage.GetPixel(x - 1, y + 1).R + (quantError * 5 / 42)));
                    if (y + 1 < height)
                        grayImage.SetPixel(x, y + 1, ClampColor(grayImage.GetPixel(x, y + 1).R + (quantError * 7 / 42)));
                    if (x + 1 < width && y + 1 < height)
                        grayImage.SetPixel(x + 1, y + 1, ClampColor(grayImage.GetPixel(x + 1, y + 1).R + (quantError * 5 / 42)));
                    if (x + 2 < width && y + 1 < height)
                        grayImage.SetPixel(x + 2, y + 1, ClampColor(grayImage.GetPixel(x + 2, y + 1).R + (quantError * 3 / 42)));
                    if (x - 1 >= 0 && y + 2 < height)
                        grayImage.SetPixel(x - 1, y + 2, ClampColor(grayImage.GetPixel(x - 1, y + 2).R + (quantError * 3 / 42)));
                    if (x < width && y + 2 < height)
                        grayImage.SetPixel(x, y + 2, ClampColor(grayImage.GetPixel(x, y + 2).R + (quantError * 5 / 42)));
                    if (x + 1 < width && y + 2 < height)
                        grayImage.SetPixel(x + 1, y + 2, ClampColor(grayImage.GetPixel(x + 1, y + 2).R + (quantError * 3 / 42)));
                }
            }

            grayImage.Dispose();

            return ditheredImage;
        }

        private static Color ClampColor(int value)
        {
            // Ensure the color value is within the valid range
            value = Math.Max(0, Math.Min(255, value));
            return Color.FromArgb(value, value, value);
        }
    }
}