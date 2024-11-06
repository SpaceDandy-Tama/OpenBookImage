using System.Drawing;
using System;

using LibObiNet;
using LibObiNet.Dithering;

namespace OpenBookImageTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Not enough arguments.");
                return;
            }

            string imagePath = args[0];

            if (imagePath.EndsWith(".obi", StringComparison.OrdinalIgnoreCase))
            {
                LoadAndConvertObiToBmp(imagePath, null);
            }
            else
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Not enough arguments.");
                    return;
                }

                int bpp = 8;
                if (args[1] == "-4")
                {

                    bpp = 4;
                }
                else if (args[1] == "-2")
                {

                    bpp = 2;
                }
                else if (args[1] == "-1")
                {

                    bpp = 1;
                }

                //Resizing
                int resizingMode = 0;
                int maxWidth = 300;
                int maxHeight = 400;

                if (args[2] == "-noResize")
                {
                    resizingMode = 0;
                }
                else if (args[2].Equals("-min") || args[2].Equals("-minimum"))
                {
                    //stretch width to meet minWidth criteria
                    resizingMode = 1;
                }
                else if(args[2].Contains(":"))
                {
                    string[] split = args[2].Split(':');
                    string[] maxResolution = split[1].Split('x');

                    if (split[0].Equals("-max") || split[0].Equals("-maximum"))
                    {
                        //This is the current resizing mode
                        //It downscales the image if either width or height is above their respective max values
                        resizingMode = 2;
                    }
                    else if (split[0].Equals("-stretch"))
                    {
                        //stretch to specified dimensions ignoring aspect ratio
                        resizingMode = 3;
                    }
                    else if (split[0].Equals("-fill"))
                    {
                        //add black pixels to the sides of the image until desired width is reached
                        resizingMode = 4;
                    }

                    maxWidth = int.Parse(maxResolution[0]);
                    maxHeight = int.Parse(maxResolution[1]);
                }

                //Optional Arguments
                int applyDithering = 0;
                bool useRLE = false;
                string outputFileName = null;
                for (int i = 3; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg == "-blueNoise")
                        applyDithering = 3;
                    else if (arg == "-stucki")
                        applyDithering = 2;
                    else if (arg == "-floydSteinberg")
                        applyDithering = 1;
                    else if(arg == "-RLE")
                        useRLE = true;
                    else if (arg.StartsWith("-o"))
                    {
                        string[] split = arg.Split(':');
                        outputFileName = split[1];
                    }
                }

                string obiFile = LoadAndConvertImageToObi(imagePath, ref outputFileName, resizingMode, applyDithering, useRLE, maxWidth, maxHeight, bpp);
            }
        }

        private static string LoadAndConvertImageToObi(string sourceFile, ref string outputFilePath, int resizingMode, int applyDithering, bool useRLE,
                                                                                                                int maxWidth, int maxHeight, int bpp)
        {
            try
            {
                using (Bitmap originalImage = new Bitmap(sourceFile))
                {
                    Bitmap resizedImage = null;
                    LibObiNet.PixelFormat pixelFormat = (LibObiNet.PixelFormat)bpp;

                    if (resizingMode == 0)
                    {
                        resizedImage = originalImage;
                    }
                    else if(resizingMode == 1)
                    {
                        resizedImage = ObiUtils.StretchBitmapToMinWidthRequired(originalImage, pixelFormat);
                    }
                    else if(resizingMode == 2)
                    {
                        resizedImage = ObiUtils.RescaleBitmapToMaxDimensions(originalImage, maxWidth, maxHeight, pixelFormat);
                    }
                    else if(resizingMode == 3)
                    {
                        resizedImage = ObiUtils.StretchBitmap(originalImage, maxWidth, maxHeight, pixelFormat);
                    }
                    else if (resizingMode == 4)
                    {
                        resizedImage = ObiUtils.FillBitmap(originalImage, maxWidth, pixelFormat);
                    }


                    if (string.IsNullOrEmpty(outputFilePath))
                    {
#if DEBUG
                        // Construct the output file name
                        outputFilePath = $"{bpp}bit";
                        if (applyDithering == 3)
                            outputFilePath += "_blueNoise";
                        else if (applyDithering == 2)
                            outputFilePath += "_stucki";
                        else if (applyDithering == 1)
                            outputFilePath += "_floydSteinberg";
                        if (useRLE)
                            outputFilePath += "_RLE";
                        outputFilePath += $"_{resizedImage.Width}x{resizedImage.Height}.obi";
#else
                        outputFilePath = Path.GetFileNameWithoutExtension(sourceFile) + ".obi";
#endif
                    }
                    
                    Bitmap result = null;
                    if(bpp == 8 && applyDithering > 0)
                    {
                        Console.WriteLine("Can't have 8 bits per pixel and dithering at the same time");
                        Environment.Exit(0);
                    }
                    else if (applyDithering == 3)
                    {
                        if(bpp > 1)
                        {
                            Console.WriteLine("BlueNoiseDithering only works at 1 bpp");
                            Environment.Exit(0);
                        }

                        result = BlueNoise.Dither(resizedImage);
                    }
                    else if (applyDithering == 2)
                    {
                        result = Stucki.Dither(resizedImage, bpp);
                    }
                    else if (applyDithering == 1)
                    {
                        result = FloydSteinberg.Dither(resizedImage, bpp);
                    }
                    else
                    {
                        result = ObiUtils.ConvertToGrayscale(resizedImage, bpp);
                    }

                    //result.Save($"{outputFilePath}.debug.bmp");

                    // Convert to OBI format and save
                    ObiFile obiFile = new ObiFile(result, pixelFormat, useRLE);
                    obiFile.Save(outputFilePath);
                    Console.WriteLine($"Image saved in OBI format as {outputFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.ReadLine();
            }

            return outputFilePath;
        }

        private static void LoadAndConvertObiToBmp(string sourceFile, string outputFileName)
        {
            ObiFile obiFile = new ObiFile(sourceFile);
            Bitmap bitmap = ObiUtils.CopyToBitmap(obiFile);
            if (string.IsNullOrEmpty(outputFileName))
            {
                bitmap.Save($"{outputFileName}.bmp");
                Console.WriteLine($"{outputFileName}.bmp saved");
            }
            else
            {
                bitmap.Save($"{sourceFile}.bmp");
                Console.WriteLine($"{sourceFile}.bmp saved");
            }
        }
    }

}