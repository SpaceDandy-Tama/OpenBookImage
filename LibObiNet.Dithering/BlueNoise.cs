using System.Drawing;

namespace LibObiNet.Dithering
{
    //Suitable for 1 bit per pixel
    public static class BlueNoise
    {
        private static readonly double[,] ditherMatrix = {
            { 0.0, 0.5, 0.25, 0.75 },
            { 0.875, 0.375, 0.625, 0.125 },
            { 0.1875, 0.6875, 0.9375, 0.4375 },
            { 0.5625, 0.3125, 0.8125, 0.3125 }
        };

        private static readonly int matrixSize = 4; // Size of the dither matrix

        public static Bitmap Dither(Bitmap colorBitmap)
        {
            Bitmap grayImage = ObiUtils.ConvertToGrayscale(colorBitmap);
            int width = grayImage.Width;
            int height = grayImage.Height;
            Bitmap ditheredImage = new Bitmap(width, height);

            // Calculate the threshold matrix for blue noise dithering
            double[,] thresholdMatrix = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    thresholdMatrix[y, x] = ditherMatrix[x % matrixSize, y % matrixSize];
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color oldColor = grayImage.GetPixel(x, y);
                    int grayValue = oldColor.R; // Assuming the image is in grayscale
                    double threshold = thresholdMatrix[y, x] * 255; // Scale to 0-255

                    // Determine the new color value based on threshold
                    int newColorValue = (grayValue < threshold) ? 0 : 255;
                    Color newColor = Color.FromArgb(newColorValue, newColorValue, newColorValue);
                    ditheredImage.SetPixel(x, y, newColor);
                }
            }

            grayImage.Dispose();
            return ditheredImage;
        }
    }
}
