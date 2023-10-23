using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Lab2B;

public class InvertImageDto
{
    public byte[] ImageBuffer { get; set; }
    public int StartY { get; set; }
    public int EndY { get; set; }
    public int Width { get; set; }
    public int Stride { get; set; }
    public int BytesPerPixel { get; set; }
}

public class ConvolutionImageDto
{
    public byte[] ImageBuffer { get; set; }
    public byte[] Result { get; set; }
    public double[,] KernelMatrix { get; set; }
    public int StartX { get; set; }
    public int EndX { get; set; }
    public int ImageHeight { get; set; }
    public int ImageWidth { get; set; }
}

public static class ImageMatrixOperations
{
    public static Bitmap Invert(Bitmap image, int countThreads = 1)
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

                var parameters = new InvertImageDto()
                {
                    ImageBuffer = pixels,
                    BytesPerPixel = bytesPerPixel,
                    StartY = startY,
                    EndY = endY,
                    Stride = bitmapData.Stride,
                    Width = widthInBytes,
                };
                var myThread = new Thread(InvertInternal);
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
            var parameters = new InvertImageDto()
            {
                ImageBuffer = pixels,
                BytesPerPixel = bytesPerPixel,
                StartY = 0,
                EndY = heightInPixels,
                Stride = bitmapData.Stride,
                Width = widthInBytes,
            };
            InvertInternal(parameters);
        }

        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
        image.UnlockBits(bitmapData);

        return image;
    }

    private static void InvertInternal(object? obj)
    {
        if (obj == null) return;
        var parameters = (InvertImageDto)obj;

        for (var y = parameters.StartY; y < parameters.EndY; y++)
        {
            var currentLine = y * parameters.Stride;
            for (var x = 0; x < parameters.Width; x = x + parameters.BytesPerPixel)
            {
                parameters.ImageBuffer[currentLine + x] = (byte)(255 - parameters.ImageBuffer[currentLine + x]);
                parameters.ImageBuffer[currentLine + x + 1] = (byte)(255 - parameters.ImageBuffer[currentLine + x + 1]); ;
                parameters.ImageBuffer[currentLine + x + 2] = (byte)(255 - parameters.ImageBuffer[currentLine + x + 2]); ;
            }
        }
    }

    private static void ConvolutionInternal(object? obj)
    {
        if (obj == null) return;
        var parameters = (ConvolutionImageDto)obj;

        var kernelWidth = parameters.KernelMatrix.GetLength(0);
        var kernelHeight = parameters.KernelMatrix.GetLength(1);

        for (var x = parameters.StartX; x < parameters.EndX; x++)
        {
            for (var y = 0; y < parameters.ImageHeight; y++)
            {
                double redSum = 0, greenSum = 0, blueSum = 0, kernelSum = 0;

                for (var i = 0; i < kernelWidth; i++)
                {
                    for (var j = 0; j < kernelHeight; j++)
                    {
                        var pixelPosX = x + (i - (kernelWidth / 2));
                        var pixelPosY = y + (j - (kernelHeight / 2));
                        if ((pixelPosX < 0) ||
                          (pixelPosX >= parameters.ImageWidth) ||
                        (pixelPosY < 0) ||
                          (pixelPosY >= parameters.ImageHeight)) continue;

                        var r = parameters.ImageBuffer[3 * (parameters.ImageWidth * pixelPosY + pixelPosX) + 0];
                        var g = parameters.ImageBuffer[3 * (parameters.ImageWidth * pixelPosY + pixelPosX) + 1];
                        var b = parameters.ImageBuffer[3 * (parameters.ImageWidth * pixelPosY + pixelPosX) + 2];

                        var kernelVal = parameters.KernelMatrix[i, j];

                        redSum += r * kernelVal;
                        greenSum += g * kernelVal;
                        blueSum += b * kernelVal;

                        kernelSum += kernelVal;
                    }
                }

                if (kernelSum <= 0) kernelSum = 1;

                redSum /= kernelSum;
                if (redSum < 0) redSum = 0;
                if (redSum > 255) redSum = 255;

                greenSum /= kernelSum;
                if (greenSum < 0) greenSum = 0;
                if (greenSum > 255) greenSum = 255;

                blueSum /= kernelSum;
                if (blueSum < 0) blueSum = 0;
                if (blueSum > 255) blueSum = 255;

                parameters.Result[3 * (parameters.ImageWidth * y + x) + 0] = (byte)redSum;
                parameters.Result[3 * (parameters.ImageWidth * y + x) + 1] = (byte)greenSum;
                parameters.Result[3 * (parameters.ImageWidth * y + x) + 2] = (byte)blueSum;
            }
        }
    }

    public static Bitmap Convolution(Bitmap image, double[,] kernelMatrix, int countThreads = 1)
    {
        var imageWidth = image.Width;
        var imageHeight = image.Height;

        var imageData = image.LockBits(
            new Rectangle(0, 0, imageWidth, imageHeight),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        var bytes = imageData.Stride * imageData.Height;
        var buffer = new byte[bytes];
        var result = new byte[bytes];

        Marshal.Copy(imageData.Scan0, buffer, 0, bytes);

        if (countThreads > 1)
        {
            var width = imageWidth / countThreads;
            var threads = new List<Thread>(countThreads);
            for (var i = 0; i < countThreads; i++)
            {
                var startX = width * i;
                var endX = startX + width;

                var parameters = new ConvolutionImageDto()
                {
                    ImageBuffer = buffer,
                    Result = result,
                    KernelMatrix = kernelMatrix,
                    ImageWidth = imageWidth,
                    ImageHeight = imageHeight,
                    StartX = startX,
                    EndX = endX
                };
                var myThread = new Thread(ConvolutionInternal);
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

            var parameters = new ConvolutionImageDto()
            {
                ImageBuffer = buffer,
                Result = result,
                KernelMatrix = kernelMatrix,
                ImageWidth = imageWidth,
                ImageHeight = imageHeight,
                StartX = 0,
                EndX = imageWidth
            };
            ConvolutionInternal(parameters);
        }

        Marshal.Copy(result, 0, imageData.Scan0, bytes);
        image.UnlockBits(imageData);

        return image;
    }
}