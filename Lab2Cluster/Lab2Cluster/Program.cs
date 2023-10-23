using System.Diagnostics;

namespace Lab2Cluster;

public class Program
{
    private static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            for (var numClusters = 3; numClusters <= 5; numClusters++)
            {
                Console.WriteLine($"Number clusters: {numClusters}");
                for (var countRows = 1000; countRows <= 5000; countRows += 2000)
                {
                    for (var countThreads = 2; countThreads <= 16; countThreads += 2)
                    {
                        var (index, timeMs) = RunProgram(countRows, countThreads, numClusters);
                        Console.WriteLine($"Rows: {countRows}\tThreads: {countThreads}\tIndex: {index}\tTime: {timeMs}");
                    }
                    Console.WriteLine();
                }
            }

            Console.ReadLine();
            return;
        }

        // Test run with chart rendering
        var testNumClusters = 5;
        var testCountRows = 5000;
        var testCountThreads = 2;

        if (args.Length == 3)
        {
            if (int.TryParse(args[0], out var argNumClusters))
            {
                testNumClusters = argNumClusters;
            }

            if (int.TryParse(args[1], out var argCountRows))
            {
                testCountRows = argCountRows;
            }

            if (int.TryParse(args[2], out var argCountThreads))
            {
                testCountThreads = argCountThreads;
            }
        }
        Console.WriteLine("\nNt: " + testCountThreads);
        var stopwatch = new Stopwatch();
        var rawData = BDPatientsReader.ReadFile(@".\BD-Patients.csv", testCountRows);

        var (clustering, normalizedData, means) = KMeans.Cluster(rawData, testNumClusters);

        stopwatch.Start();
        var testIndex = IndexMaulikBandoypadhyay.GetIndex(normalizedData, clustering, means, testCountThreads);
        stopwatch.Stop();

        Chart.Create(clustering, normalizedData, testNumClusters);
        Console.WriteLine("\nNumber clusters: " + testNumClusters);
        Console.WriteLine($"\nIndex: {testIndex}\n");
        Console.WriteLine($"\n{stopwatch.ElapsedMilliseconds} ms");
        Console.ReadLine();
    }

    private static (double, long) RunProgram(int countRows, int countThreads, int numClusters)
    {
        var stopwatch = new Stopwatch();
        var rawData = BDPatientsReader.ReadFile(@".\BD-Patients.csv", countRows);
        var (clustering, normalizedData, means) = KMeans.Cluster(rawData, numClusters);
        stopwatch.Start();
        var index = IndexMaulikBandoypadhyay.GetIndex(normalizedData, clustering, means, countThreads);
        stopwatch.Stop();

        return (index, stopwatch.ElapsedMilliseconds);
    }
}