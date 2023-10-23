using System.Diagnostics;
using System.Drawing;

namespace Lab2A;

public class Program
{
    private static void Main(string[] args)
    {
        string? path = null;
        Bitmap? image = null;
        
        while(image == null)
        {
            Console.Write("Enter the path to the file: ");
            path = Console.ReadLine();

            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            path = path.Trim();

            if (!File.Exists(path))
            {
                Console.Write("\nThis file does not exist \n");
                continue;
            };
            try
            {
                image = new Bitmap(path);
            }
            catch
            {
                image  = null;
                Console.Write("\nThis file is not a picture\n");
            }
        }

        string? savePath = null;
        while (savePath == null)
        {
            Console.Write("\nEnter the save path: ");
            savePath = Console.ReadLine();

            if (string.IsNullOrEmpty(savePath))
            {
                savePath = null;
                continue;
            }

            savePath = savePath.Trim();

            if (Path.IsPathFullyQualified(savePath))
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                if (savePath[^1] != '\\')
                {
                    savePath += "\\";
                }
            }
            else
            {
                Console.Write("\nThis path is not valid\n");
                savePath = null;
            }
        }

        int? countThreads = null;
        while (countThreads == null)
        {
            Console.Write("\nEnter count threads: ");
            try
            {
                countThreads = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Console.Write("\nEnter number\n");
                countThreads = null;
            }
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var newImage = ImageMatrixOperations.ConvertImageByIntensity(image, 125, (int)countThreads);
        newImage = ImageMatrixOperations.Dilate(newImage, 6, (int)countThreads);

        var pathSave = savePath + Path.GetFileName(path);
        newImage.Save(pathSave);

        stopwatch.Stop();
        Console.Write($"\n\n{newImage.Height}x{newImage.Width} - {stopwatch.ElapsedMilliseconds} ms");
        Console.Write($"\nNew file location: {pathSave}");
        Console.ReadLine();
    }
}