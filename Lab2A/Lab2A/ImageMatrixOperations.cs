using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Lab2A;

public class DilateMatrixDto
{
    public int KernelSize { get; set; }
    public byte[] ImageBuffer { get; set; }
    public byte[] Result { get; set; }
    public int StartX { get; set; }
    public int EndX { get; set; }
    public int DataHeight { get; set; }
    public int Stride { get; set; }
}

public class ConvertImageDto
{
    public byte[] ImageBuffer { get; set; }
    public int Threshold { get; set; }
    public int StartY { get; set; }
    public int EndY { get; set; }
    public int Width { get; set; }
    public int Stride { get; set; }
    public int BytesPerPixel { get; set; }
}

public static class ImageMatrixOperations
{
    public static Bitmap ConvertImageByIntensity(Bitmap image, int threshold, int countThreads = 1)
    {
        var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);

        var bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
        var byteCount = bitmapData.Stride * image.Height;
        var pixels = new byte[byteCount];
        var ptrFirstPixel = bitmapData.Scan0;
        Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
        var heightInPixels = bitmapData.Height;
        var widthInBytes = bitmapData.Width * bytesPerPixel;


        if (countThreads > 1)
        {
            var height = heightInPixels / countThreads;
            var threads = new List<Thread>(countThreads);
            for (var i = 0; i < countThreads; i++)
            {
                var startY = height * i;
                var endY = startY + height;

                var parameters = new ConvertImageDto()
                {
                    ImageBuffer = pixels,
                    Threshold = threshold,
                    BytesPerPixel = bytesPerPixel,
                    StartY = startY,
                    EndY = endY,
                    Stride = bitmapData.Stride,
                    Width = widthInBytes,
                };
                var myThread = new Thread(ConvertImageByIntensityInternal);
                threads.Add(myThread);
                myThread.Start(parameters);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
        else
        {
            var parameters = new ConvertImageDto()
            {
                ImageBuffer = pixels,
                Threshold = threshold,
                BytesPerPixel = bytesPerPixel,
                StartY = 0,
                EndY = heightInPixels,
                Stride = bitmapData.Stride,
                Width = widthInBytes,
            };
            ConvertImageByIntensityInternal(parameters);
        }

        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
        image.UnlockBits(bitmapData);

        return image;
    }

    private static void ConvertImageByIntensityInternal(object? obj)
    {
        if (obj == null) return;
        var parameters = (ConvertImageDto)obj;

        for (var y = parameters.StartY; y < parameters.EndY; y++)
        {
            var currentLine = y * parameters.Stride;
            for (var x = 0; x < parameters.Width; x = x + parameters.BytesPerPixel)
            {
                int oldBlue = parameters.ImageBuffer[currentLine + x];
                int oldGreen = parameters.ImageBuffer[currentLine + x + 1];
                int oldRed = parameters.ImageBuffer[currentLine + x + 2];

                var intensity = (oldBlue + oldGreen + oldRed) / 3;
                var newPixel =(byte)( intensity > parameters.Threshold ? 255 : 0);

                parameters.ImageBuffer[currentLine + x] = newPixel;
                parameters.ImageBuffer[currentLine + x + 1] = newPixel;
                parameters.ImageBuffer[currentLine + x + 2] = newPixel;
            }
        }
    }

    private static void Dilate(object? obj)
    {
        if (obj == null) return;
        var parameters = (DilateMatrixDto)obj;
        var halfKernelSize = (parameters.KernelSize - 1) / 2;
        for (var i = halfKernelSize + parameters.StartX; i < parameters.EndX - halfKernelSize; i++)
        {
            for (var j = halfKernelSize; j < parameters.DataHeight - halfKernelSize; j++)
            {
                var position = i * 3 + j * parameters.Stride;
                for (var k = -halfKernelSize; k <= halfKernelSize; k++)
                {
                    for (var l = -halfKernelSize; l <= halfKernelSize; l++)
                    {
                        var sePos = position + k * 3 + l * parameters.Stride;
                        for (var c = 0; c < 3; c++)
                        {
                            parameters.Result[sePos + c] = Math.Max(parameters.Result[sePos + c], parameters.ImageBuffer[position]);
                        }
                    }
                }
            }
        }
    }

    public static Bitmap Dilate(Bitmap image, int kernelSize, int countThreads = 1)
    {
        var bitmapData = image.LockBits(
            new Rectangle(0, 0, image.Width, image.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        var byteCount = bitmapData.Stride * bitmapData.Height;
        var buffer = new byte[byteCount];
        var result = new byte[byteCount];

        Marshal.Copy(bitmapData.Scan0, buffer, 0, byteCount);

        if (countThreads > 1)
        {
            var width = bitmapData.Width / countThreads;
            var threads = new List<Thread>(countThreads);
            for (var i = 0; i < countThreads; i++)
            {
                var startX = width * i;
                var endX = startX + width;

                var parameters = new DilateMatrixDto()
                {
                    ImageBuffer = buffer,
                    KernelSize = kernelSize,
                    StartX = startX,
                    EndX = endX,
                    Stride = bitmapData.Stride,
                    DataHeight = bitmapData.Height,
                    Result = result
                };
                var myThread = new Thread(Dilate);
                threads.Add(myThread);
                myThread.Start(parameters);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
        else
        {
            var parameters = new DilateMatrixDto()
            {
                ImageBuffer = buffer,
                KernelSize = kernelSize,
                StartX = 0,
                EndX = bitmapData.Width,
                Stride = bitmapData.Stride,
                DataHeight = bitmapData.Height,
                Result = result
            };
            Dilate(parameters);
        }

        Marshal.Copy(result, 0, bitmapData.Scan0, byteCount);
        image.UnlockBits(bitmapData);

        return image;
    }
}