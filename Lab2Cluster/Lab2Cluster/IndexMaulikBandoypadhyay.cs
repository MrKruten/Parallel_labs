namespace Lab2Cluster;

public static class IndexMaulikBandoypadhyay
{
    public static double GetIndex(double[][] data, int[] clustering, double[][] centers, int countThreads, int pow = 2)
    {
        // formula: MB_INDEX = (1/c * E1/Ec * D)^p
        var numClusters = centers.Length;
        var ec = CalculateSumOfIntraclusterDistances(data, clustering, centers, countThreads); // Ec
        var e = CalculateSumDistanceFromCenterToEachElement(data, countThreads); // E1
        var minDistance = MinDistance(centers); // D
        return Math.Pow(((e / ec) * (minDistance / numClusters)), pow);
    }

    private static double MinDistance(double[][] centers)
    {
        var minDistance = Double.MaxValue; // D = min i,j ||vi - vj|| 

        for (var i = 0; i < centers.Length - 1; i++)
        {
            for (var j = i + 1; j < centers.Length; j++)
            {
                var min = MathOperations.Distance(centers[i], centers[j]);
                if (min < minDistance)
                {
                    minDistance = min;
                }
            }
        }

        return minDistance;
    }

    // the sum of the distances from the cluster center to each cluster element
    private static double CalculateSumOfIntraclusterDistances(double[][] data, int[] clustering, double[][] centers, int countThreads)
    {
        if (countThreads > 1)
        {
            var threads = new List<Thread>(countThreads);
            var parametersList = new List<SumOfIntraclusterDistancesDto>(countThreads);
            var stepSize = data.Length / countThreads;

            for (var i = 0; i < countThreads; i++)
            {
                var start = stepSize * i;
                var end = start + stepSize;
                var parameters = new SumOfIntraclusterDistancesDto
                {
                    Centers = centers,
                    Clustering = clustering,
                    Data = data,
                    Start = start,
                    End = end,
                    Sum = 0
                };
                parametersList.Add(parameters);
                var myThread = new Thread(CalculateSumOfIntraclusterDistancesInternal);
                threads.Add(myThread);
                myThread.Start(parameters);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return parametersList.Select(p => p.Sum).Sum();
        }

        var parametersForOneThread = new SumOfIntraclusterDistancesDto
        {
            Centers = centers,
            Clustering = clustering,
            Data = data,
            Start = 0,
            End = data.Length,
            Sum = 0
        };
        CalculateSumOfIntraclusterDistancesInternal(parametersForOneThread);
        return parametersForOneThread.Sum;
    }

    private static void CalculateSumOfIntraclusterDistancesInternal(object? obj)
    {
        if (obj == null) return;
        var parameters = (SumOfIntraclusterDistancesDto)obj;
        for (var i = parameters.Start; i < parameters.End; ++i)
        {
            var clusterId = parameters.Clustering[i];
            parameters.Sum += MathOperations.Distance(parameters.Data[i], parameters.Centers[clusterId]);
        }
    }

    // the sum of the distances from the center of the set to each element
    private static double CalculateSumDistanceFromCenterToEachElement(double[][] data, int countThreads)
    {
        var center = CalculateCenter(data, countThreads);

        if (countThreads > 1)
        {
            var threads = new List<Thread>(countThreads);
            var parametersList = new List<SumDistanceFromCenterToEachElementDto>(countThreads);
            var stepSize = data.Length / countThreads;
            for (var i = 0; i < countThreads; i++)
            {
                var start = stepSize * i;
                var end = start + stepSize;
                var parameters = new SumDistanceFromCenterToEachElementDto
                {
                    Center = center,
                    Data = data,
                    Start = start,
                    End = end,
                    Sum = 0
                };
                parametersList.Add(parameters);
                var myThread = new Thread(CalculateSumDistanceFromCenterToEachElementInternal);
                threads.Add(myThread);
                myThread.Start(parameters);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return parametersList.Select(p => p.Sum).Sum();
        }

        var parametersForOneThread = new SumDistanceFromCenterToEachElementDto
        {
            Center = center,
            Data = data,
            Start = 0,
            End = data.Length,
            Sum = 0
        };
        CalculateSumDistanceFromCenterToEachElementInternal(parametersForOneThread);

        return parametersForOneThread.Sum;
    }

    private static void CalculateSumDistanceFromCenterToEachElementInternal(object? obj)
    {
        if (obj == null) return;
        var parameters = (SumDistanceFromCenterToEachElementDto)obj;
        for (var i = parameters.Start; i < parameters.End; ++i)
        {
            parameters.Sum += MathOperations.Distance(parameters.Data[i], parameters.Center);
        }
    }

    private static double[] CalculateCenter(double[][] data, int countThreads)
    {
        var center = new double[] { 0, 0 };

        if (countThreads > 1)
        {
            var threads = new List<Thread>(countThreads);
            var centersList = new List<double[]>(countThreads);
            var stepSize = data.Length / countThreads;
            for (var i = 0; i < countThreads; i++)
            {
                var start = stepSize * i;
                var end = start + stepSize;
                var threadCenter = new double[] { 0, 0 };
                centersList.Add(threadCenter);
                var parameters = new CalculateCenterDto
                {
                    Center = center,
                    Data = data,
                    Start = start,
                    End = end,
                };
                var myThread = new Thread(CalculateCenterInternal);
                threads.Add(myThread);
                myThread.Start(parameters);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            foreach (var threadCenter in centersList)
            {
                for (var i = 0; i < threadCenter.Length; i++)
                {
                    center[i] += threadCenter[i];
                }
            }
        }
        else
        {
            var parameters = new CalculateCenterDto
            {
                Center = center,
                Data = data,
                Start = 0,
                End = data.Length,
            };
            CalculateCenterInternal(parameters);
        }

        return center;
    }

    private static void CalculateCenterInternal(object? obj)
    {
        if(obj == null) return;
        var parameters = (CalculateCenterDto)obj;
        for (var i = parameters.Start; i < parameters.End; ++i)
        {
            var point = parameters.Data[i];
            for (var j = 0; j < point.Length; ++j)
            {
                parameters.Center[j] += point[j] / parameters.Data.Length;
            }
        }
    }


    private class SumDistanceFromCenterToEachElementDto : CalculateCenterDto
    {
        public double Sum { get; set; }
    }

    private class CalculateCenterDto : ThreadData
    {
        public double[] Center { get; set; }
    }

    private class SumOfIntraclusterDistancesDto: ThreadData
    {
        public double[][] Centers { get; set; }
        public int[] Clustering { get; set; }
        public double Sum { get; set; }
    }

    private class ThreadData
    {
        public int Start { get; set; }
        public int End { get; set; }
        public double[][] Data { get; set; }
    }
}