using System.Drawing;

namespace LibObiNet.Dithering
{

    //Suitable for 4, 2 and 1 bits per pixel
    public static class FloydSteinberg
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
                    Color oldColor = grayImage.GetPixel(x, y);
                    int newColorValue = oldColor.R;
                    if(bpp == 4)
                    {
                        newColorValue = ObiUtils.To4Bits(oldColor.R) * ObiUtils.From4Bits;
                    }
                    else if (bpp == 2)
                    {
                        newColorValue = ObiUtils.To2Bits(oldColor.R) * ObiUtils.From2Bits;
                    }
                    else if(bpp == 1)
                    {
                        newColorValue = ObiUtils.To1Bit(oldColor.R) * ObiUtils.From1Bit;
                    }
                    Color newColor = Color.FromArgb(newColorValue, newColorValue, newColorValue);
                    ditheredImage.SetPixel(x, y, newColor);

                    int error = oldColor.R - newColorValue;

                    // Distribute the error to neighboring pixels
                    if (x + 1 < width)
                        DistributeError(grayImage, x + 1, y, error * 7 / 16);
                    if (x - 1 >= 0 && y + 1 < height)
                        DistributeError(grayImage, x - 1, y + 1, error * 3 / 16);
                    if (y + 1 < height)
                        DistributeError(grayImage, x, y + 1, error * 5 / 16);
                    if (x + 1 < width && y + 1 < height)
                        DistributeError(grayImage, x + 1, y + 1, error * 1 / 16);
                }
            }

            grayImage.Dispose();

            return ditheredImage;
        }

        public static void DistributeError(Bitmap image, int x, int y, int error)
        {
            Color pixelColor = image.GetPixel(x, y);
            int newGrayValue = Clamp(pixelColor.R + error, 0, 255);
            image.SetPixel(x, y, Color.FromArgb(newGrayValue, newGrayValue, newGrayValue));
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

    }
}