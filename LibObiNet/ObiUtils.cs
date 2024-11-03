using System.Drawing;
using System.Collections.Generic;
using System;
using System.Drawing.Drawing2D;

namespace LibObiNet
{
    public static class ObiUtils
    {
        internal static byte[] GetDataFromBitmap(ushort width, ushort height, PixelFormat pixelFormat, ObiFlags flags, Bitmap bitmap)
        {
            byte[] data = null;

            if ((flags & ObiFlags.RLE) == ObiFlags.RLE)
            {
                List<byte> rleData = new List<byte>();

                for (int y = 0; y < height; y++)
                {
                    int runLength = 1;
                    byte lastValue = 0;
                    if (pixelFormat == PixelFormat.Format8Grayscale)
                    {
                        lastValue = bitmap.GetPixel(0, y).R;
                    }
                    else if (pixelFormat == PixelFormat.Format4Grayscale)
                    {
                        lastValue = ObiUtils.PackTwo4BitColors(bitmap, 0, y);
                    }
                    else if (pixelFormat == PixelFormat.Format2Grayscale)
                    {
                        lastValue = ObiUtils.PackFour2BitColors(bitmap, 0, y);
                    }
                    else if (pixelFormat == PixelFormat.Monochromatic)
                    {
                        lastValue = ObiUtils.PackEight1BitColors(bitmap, 0, y);
                    }

                    for (int x = 8 / (byte)pixelFormat; x < width;)
                    {
                        byte currentValue = 0;
                        if (pixelFormat == PixelFormat.Format8Grayscale)
                        {
                            currentValue = bitmap.GetPixel(x, y).R;
                            x += 1;
                        }
                        else if (pixelFormat == PixelFormat.Format4Grayscale)
                        {
                            currentValue = ObiUtils.PackTwo4BitColors(bitmap, x, y);
                            x += 2;
                        }
                        else if (pixelFormat == PixelFormat.Format2Grayscale)
                        {
                            currentValue = ObiUtils.PackFour2BitColors(bitmap, x, y);
                            x += 4;
                        }
                        else if (pixelFormat == PixelFormat.Monochromatic)
                        {
                            currentValue = ObiUtils.PackEight1BitColors(bitmap, x, y);
                            x += 8;
                        }

                        if (runLength < 255 && currentValue == lastValue)
                        {
                            runLength++;
                        }
                        else
                        {
                            rleData.Add(lastValue);
                            rleData.Add((byte)runLength);
                            lastValue = currentValue;
                            runLength = 1;
                        }
                    }

                    rleData.Add(lastValue);
                    rleData.Add((byte)runLength);
                }

                data = rleData.ToArray();
            }
            else
            {
                data = new byte[(width * height * (byte)pixelFormat) / 8];

                // Copy each pixel from the bitmap
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width;)
                    {
                        // Calculate the index in the Data array
                        int pixelIndex = y * width + x;

                        if (pixelFormat == PixelFormat.Format8Grayscale)
                        {
                            data[pixelIndex] = bitmap.GetPixel(x, y).R;
                            x += 1;
                        }
                        else if (pixelFormat == PixelFormat.Format4Grayscale)
                        {
                            pixelIndex /= 2;

                            data[pixelIndex] = ObiUtils.PackTwo4BitColors(bitmap, x, y);
                            x += 2;
                        }
                        else if (pixelFormat == PixelFormat.Format2Grayscale)
                        {
                            pixelIndex /= 4;

                            data[pixelIndex] = ObiUtils.PackFour2BitColors(bitmap, x, y);
                            x += 4;
                        }
                        else if (pixelFormat == PixelFormat.Monochromatic)
                        {
                            pixelIndex /= 8;

                            data[pixelIndex] = ObiUtils.PackEight1BitColors(bitmap, x, y);
                            x += 8;
                        }
                    }
                }
            }

            return data;
        }
        public static Bitmap CopyToBitmap(ObiFile obiFile)
        {
            Bitmap bitmap = new Bitmap(obiFile.Header.Width, obiFile.Header.Height);

            if (obiFile.Header.IsRLE)
            {
                int dataIndex = 0; // index to read from Data array
                for (int y = 0; y < obiFile.Header.Height; y++)
                {
                    for (int x = 0; x < obiFile.Header.Width;)
                    {
                        // Read the RLE count
                        byte pixelValue = obiFile.Data[dataIndex];
                        dataIndex++;
                        byte count = obiFile.Data[dataIndex];
                        dataIndex++;

                        // Set the pixel count number of times
                        for (int i = 0; i < count; i++)
                        {
                            if (obiFile.Header.PixelFormat == PixelFormat.Format8Grayscale)
                            {
                                bitmap.SetPixel(x, y, Color.FromArgb(pixelValue, pixelValue, pixelValue));
                                x++;
                            }
                            else if (obiFile.Header.PixelFormat == PixelFormat.Format4Grayscale)
                            {
                                Color color1;
                                Color color2;
                                ObiUtils.UnpackTwo4BitColors(pixelValue, out color1, out color2);

                                bitmap.SetPixel(x, y, color1);
                                bitmap.SetPixel(x + 1, y, color2);

                                x += 2;
                            }
                            else if (obiFile.Header.PixelFormat == PixelFormat.Format2Grayscale)
                            {
                                Color color1;
                                Color color2;
                                Color color3;
                                Color color4;
                                ObiUtils.UnpackFour2BitColors(pixelValue, out color1, out color2, out color3, out color4);

                                bitmap.SetPixel(x, y, color1);
                                bitmap.SetPixel(x + 1, y, color2);
                                bitmap.SetPixel(x + 2, y, color3);
                                bitmap.SetPixel(x + 3, y, color4);

                                x += 4;
                            }
                            else if (obiFile.Header.PixelFormat == PixelFormat.Monochromatic)
                            {
                                Color col1;
                                Color col2;
                                Color col3;
                                Color col4;
                                Color col5;
                                Color col6;
                                Color col7;
                                Color col8;
                                ObiUtils.UnpackEight1BitColors(pixelValue, out col1, out col2, out col3, out col4, out col5, out col6, out col7, out col8);

                                bitmap.SetPixel(x, y, col1);
                                bitmap.SetPixel(x + 1, y, col2);
                                bitmap.SetPixel(x + 2, y, col3);
                                bitmap.SetPixel(x + 3, y, col4);
                                bitmap.SetPixel(x + 4, y, col5);
                                bitmap.SetPixel(x + 5, y, col6);
                                bitmap.SetPixel(x + 6, y, col7);
                                bitmap.SetPixel(x + 7, y, col8);

                                x += 8;
                            }
                        }
                    }
                }
            }
            else
            {
                // Copy each pixel to the bitmap
                for (int y = 0; y < obiFile.Header.Height; y++)
                {
                    for (int x = 0; x < obiFile.Header.Width;)
                    {
                        // Calculate the index in the Data array
                        int pixelIndex = y * obiFile.Header.Width + x;

                        if (obiFile.Header.PixelFormat == PixelFormat.Format8Grayscale)
                        {
                            // Get the grayscale value from Data array
                            byte value = obiFile.Data[pixelIndex];

                            // Set the pixel in the bitmap
                            bitmap.SetPixel(x, y, Color.FromArgb(value, value, value));

                            x++;
                        }
                        else if (obiFile.Header.PixelFormat == PixelFormat.Format4Grayscale)
                        {
                            pixelIndex /= 2;

                            // Get the grayscale value from Data array
                            byte value = obiFile.Data[pixelIndex];

                            // Unpack two 4-bit grayscale colors
                            Color color1;
                            Color color2;
                            ObiUtils.UnpackTwo4BitColors(value, out color1, out color2);

                            // Set the first color
                            bitmap.SetPixel(x, y, color1);
                            // Set the second color
                            bitmap.SetPixel(x + 1, y, color2);

                            // Increment x to skip the next pixel since two have been set
                            x += 2; // We can safely increment since width is always even
                        }
                        else if (obiFile.Header.PixelFormat == PixelFormat.Format2Grayscale)
                        {
                            pixelIndex /= 4;

                            byte value = obiFile.Data[pixelIndex];

                            Color color1;
                            Color color2;
                            Color color3;
                            Color color4;
                            ObiUtils.UnpackFour2BitColors(value, out color1, out color2, out color3, out color4);

                            bitmap.SetPixel(x, y, color1);
                            bitmap.SetPixel(x + 1, y, color2);
                            bitmap.SetPixel(x + 2, y, color3);
                            bitmap.SetPixel(x + 3, y, color4);

                            x += 4;
                        }
                        else if (obiFile.Header.PixelFormat == PixelFormat.Monochromatic)
                        {
                            pixelIndex /= 8;

                            byte value = obiFile.Data[pixelIndex];

                            Color col1;
                            Color col2;
                            Color col3;
                            Color col4;
                            Color col5;
                            Color col6;
                            Color col7;
                            Color col8;
                            ObiUtils.UnpackEight1BitColors(value, out col1, out col2, out col3, out col4, out col5, out col6, out col7, out col8);

                            bitmap.SetPixel(x, y, col1);
                            bitmap.SetPixel(x + 1, y, col2);
                            bitmap.SetPixel(x + 2, y, col3);
                            bitmap.SetPixel(x + 3, y, col4);
                            bitmap.SetPixel(x + 4, y, col5);
                            bitmap.SetPixel(x + 5, y, col6);
                            bitmap.SetPixel(x + 6, y, col7);
                            bitmap.SetPixel(x + 7, y, col8);

                            x += 8; // We can safely increment since width goes like 8, 16, 24, 32, etc
                        }
                    }
                }
            }

            return bitmap;
        }

        public static Bitmap ConvertToGrayscale(Bitmap original, int bpp = 8)
        {
            Bitmap grayImage = new Bitmap(original.Width, original.Height);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    // Use the luminosity method for converting to grayscale
                    byte grayValue = (byte)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                    if (bpp == 1)
                    {
                        grayValue = (byte)(ObiUtils.To1Bit(grayValue) * ObiUtils.From1Bit);
                    }
                    else if (bpp == 2)
                    {
                        grayValue = (byte)(ObiUtils.To2Bits(grayValue) * ObiUtils.From2Bits);
                    }
                    else if (bpp == 4)
                    {
                        grayValue = (byte)(ObiUtils.To4Bits(grayValue) * ObiUtils.From4Bits);
                    }

                    grayImage.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }
            return grayImage;
        }

        public static byte To4Bits(byte grayscaleColor)
        {
            return (byte)(Math.Round((double)grayscaleColor * 15 / 255));
        }
        public const byte From4Bits = 17;

        public static byte To2Bits(byte grayscaleColor)
        {
            return (byte)(Math.Round((double)grayscaleColor * 3 / 255));
        }
        public const byte From2Bits = 85;

        public static byte To1Bit(byte grayscaleColor)
        {
            return (byte)(grayscaleColor > 128 ? 1 : 0);
        }
        public const byte From1Bit = 255;

        public static byte PackTwo4BitColors(Bitmap bitmap, int x, int y)
        {
            byte grayscale4BitColor1 = ObiUtils.To4Bits(bitmap.GetPixel(x, y).R);
            byte grayscale4BitColor2 = ObiUtils.To4Bits(bitmap.GetPixel(x + 1, y).R);
            return (byte)((grayscale4BitColor1 << 4) | grayscale4BitColor2);
        }

        public static void UnpackTwo4BitColors(byte packedColors, out Color color1, out Color color2)
        {
            // Extract the upper and lower nibbles (4 bits each)
            byte color1Value = (byte)((packedColors >> 4) & 0x0F); // Upper nibble
            byte color2Value = (byte)(packedColors & 0x0F);        // Lower nibble

            // Convert 4-bit grayscale values back to 8-bit (0-255 range)
            color1Value *= ObiUtils.From4Bits;
            color2Value *= ObiUtils.From4Bits;

            // Create grayscale Color objects
            color1 = Color.FromArgb(color1Value, color1Value, color1Value);
            color2 = Color.FromArgb(color2Value, color2Value, color2Value);
        }

        public static byte PackFour2BitColors(Bitmap bitmap, int x, int y)
        {
            byte color1 = ObiUtils.To2Bits(bitmap.GetPixel(x, y).R);         // First color
            byte color2 = ObiUtils.To2Bits(bitmap.GetPixel(x + 1, y).R);     // Second color
            byte color3 = ObiUtils.To2Bits(bitmap.GetPixel(x + 2, y).R);     // Third color
            byte color4 = ObiUtils.To2Bits(bitmap.GetPixel(x + 3, y).R);     // Fourth color

            return (byte)((color1 << 6) | (color2 << 4) | (color3 << 2) | color4);
        }

        public static void UnpackFour2BitColors(byte packedColors, out Color color1, out Color color2, out Color color3, out Color color4)
        {
            // Extract the individual 2-bit colors
            byte color1Value = (byte)((packedColors >> 6) & 0x03); // Bits 6-7
            byte color2Value = (byte)((packedColors >> 4) & 0x03); // Bits 4-5
            byte color3Value = (byte)((packedColors >> 2) & 0x03); // Bits 2-3
            byte color4Value = (byte)(packedColors & 0x03);        // Bits 0-1

            // Convert 2-bit grayscale values back to 8-bit (0-255 range)
            color1Value *= ObiUtils.From2Bits;
            color2Value *= ObiUtils.From2Bits;
            color3Value *= ObiUtils.From2Bits;
            color4Value *= ObiUtils.From2Bits;

            // Create grayscale Color objects
            color1 = Color.FromArgb(color1Value, color1Value, color1Value);
            color2 = Color.FromArgb(color2Value, color2Value, color2Value);
            color3 = Color.FromArgb(color3Value, color3Value, color3Value);
            color4 = Color.FromArgb(color4Value, color4Value, color4Value);
        }

        public static byte PackEight1BitColors(Bitmap bitmap, int x, int y)
        {
            byte color1 = ObiUtils.To1Bit(bitmap.GetPixel(x, y).R);
            byte color2 = ObiUtils.To1Bit(bitmap.GetPixel(x + 1, y).R);
            byte color3 = ObiUtils.To1Bit(bitmap.GetPixel(x + 2, y).R);
            byte color4 = ObiUtils.To1Bit(bitmap.GetPixel(x + 3, y).R);
            byte color5 = ObiUtils.To1Bit(bitmap.GetPixel(x + 4, y).R);
            byte color6 = ObiUtils.To1Bit(bitmap.GetPixel(x + 5, y).R);
            byte color7 = ObiUtils.To1Bit(bitmap.GetPixel(x + 6, y).R);
            byte color8 = ObiUtils.To1Bit(bitmap.GetPixel(x + 7, y).R);

            return (byte)((color1 << 7) | (color2 << 6) | (color3 << 5) | (color4 << 4) |
                          (color5 << 3) | (color6 << 2) | (color7 << 1) | color8);
        }

        public static void UnpackEight1BitColors(byte packedColors, out Color color1, out Color color2, out Color color3, out Color color4,
                                                                    out Color color5, out Color color6, out Color color7, out Color color8)
        {
            byte color1Value = (byte)((packedColors >> 7) & 0x1);
            byte color2Value = (byte)((packedColors >> 6) & 0x1);
            byte color3Value = (byte)((packedColors >> 5) & 0x1);
            byte color4Value = (byte)((packedColors >> 4) & 0x1);
            byte color5Value = (byte)((packedColors >> 3) & 0x1);
            byte color6Value = (byte)((packedColors >> 2) & 0x1);
            byte color7Value = (byte)((packedColors >> 1) & 0x1);
            byte color8Value = (byte)(packedColors & 0x1);

            color1Value *= ObiUtils.From1Bit;
            color2Value *= ObiUtils.From1Bit;
            color3Value *= ObiUtils.From1Bit;
            color4Value *= ObiUtils.From1Bit;
            color5Value *= ObiUtils.From1Bit;
            color6Value *= ObiUtils.From1Bit;
            color7Value *= ObiUtils.From1Bit;
            color8Value *= ObiUtils.From1Bit;

            color1 = Color.FromArgb(color1Value, color1Value, color1Value);
            color2 = Color.FromArgb(color2Value, color2Value, color2Value);
            color3 = Color.FromArgb(color3Value, color3Value, color3Value);
            color4 = Color.FromArgb(color4Value, color4Value, color4Value);
            color5 = Color.FromArgb(color5Value, color5Value, color5Value);
            color6 = Color.FromArgb(color6Value, color6Value, color6Value);
            color7 = Color.FromArgb(color7Value, color7Value, color7Value);
            color8 = Color.FromArgb(color8Value, color8Value, color8Value);
        }

        public static int GetMinWidth(int width, PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormat.Format4Grayscale)
            {
                return 2;
            }
            else if (pixelFormat == PixelFormat.Format2Grayscale)
            {
                return 4;
            }
            else if (pixelFormat == PixelFormat.Monochromatic)
            {
                return 8;
            }
            return 0;
        }

        public static int GetRequiredWidthForPixelFormat(int width, PixelFormat pixelFormat)
        {
            int minWidth = ObiUtils.GetMinWidth(width, pixelFormat);
            while (minWidth != 0 && width % minWidth != 0)
            {
                width++;
            }
            return width;
        }
        public static Bitmap StretchBitmapToMinWidthRequired(Bitmap bitmap, PixelFormat pixelFormat)
        {
            int requiredWidth = GetRequiredWidthForPixelFormat(bitmap.Width, pixelFormat);

            if(bitmap.Width == requiredWidth)
            {
                return bitmap;
            }

            Bitmap resizedBitmap = new Bitmap(requiredWidth, bitmap.Height);

            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(bitmap, 0, 0, requiredWidth, bitmap.Height);
            }

            return resizedBitmap;
        }

        public static Bitmap StretchBitmap(Bitmap bitmap, int width, int height, PixelFormat pixelFormat)
        {
            int requiredWidth = GetRequiredWidthForPixelFormat(width, pixelFormat);

            if (bitmap.Width == requiredWidth && bitmap.Height == height)
            {
                return bitmap;
            }

            Bitmap resizedBitmap = new Bitmap(requiredWidth, height);

            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(bitmap, 0, 0, requiredWidth, height);
            }

            return resizedBitmap;
        }

        public static Bitmap FillBitmap(Bitmap bitmap, int width, PixelFormat pixelFormat)
        {
            int requiredWidth = GetRequiredWidthForPixelFormat(width, pixelFormat);

            if (bitmap.Width == requiredWidth)
            {
                return bitmap;
            }

            int paddingTotal = requiredWidth - bitmap.Width;
            int paddingLeft = paddingTotal / 2;

            Bitmap resizedBitmap = new Bitmap(requiredWidth, bitmap.Height);

            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                //Fill background with black color
                graphics.Clear(Color.Black);

                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(bitmap, paddingLeft, 0);
            }

            return resizedBitmap;
        }

        public static Bitmap RescaleBitmapToMaxDimensions(Bitmap bitmap, int maxWidth, int maxHeight, PixelFormat pixelFormat)
        {
            if (bitmap.Width <= maxWidth && bitmap.Height <= maxHeight)
            {
                return StretchBitmapToMinWidthRequired(bitmap, pixelFormat);
            }

            float widthRatio = (float)maxWidth / bitmap.Width;
            float heightRatio = (float)maxHeight / bitmap.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            int newWidth = (int)(bitmap.Width * scale);
            int newHeight = (int)(bitmap.Height * scale);

            newWidth = GetRequiredWidthForPixelFormat(newWidth, pixelFormat);

            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(bitmap, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }
    }

}
