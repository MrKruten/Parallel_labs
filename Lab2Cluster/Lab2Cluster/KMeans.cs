namespace Lab2Cluster;

// source: https://visualstudiomagazine.com/Articles/2013/12/01/K-Means-Data-Clustering-Using-C.aspx?Page=2
public static class KMeans
{
    public static (int[], double[][], double[][]) Cluster(double[][] rawData, int numClusters)
    {
        // index of return is tuple ID, cell is cluster ID
        // ex: [2 1 0 0 2 2] means tuple 0 is cluster 2, tuple 1 is cluster 1, tuple 2 is cluster 0, tuple 3 is cluster 0, etc.
        var data = Normalized(rawData); // so large values don't dominate

        var isChanged = true; // was there a change in at least one cluster assignment?
        var isSuccess = true; // were all means able to be computed? (no zero-count clusters)

        // init clustering[] to get things started
        // an alternative is to initialize means to randomly selected tuples
        // then the processing loop is
        // loop
        //    update clustering
        //    update means
        // end loop
        var clustering = InitClustering(data.Length, numClusters, 0); // semi-random initialization
        var means = Allocate(numClusters, data[0].Length); // small convenience

        var maxCount = data.Length * 10; // sanity check
        var ct = 0;
        while (isChanged && isSuccess && ct < maxCount)
        {
            ++ct; // k-means typically converges very quickly
            isSuccess = UpdateMeans(data, clustering, means); // compute new cluster means if possible. no effect if fail
            isChanged = UpdateClustering(data, clustering, means); // (re)assign tuples to clusters. no effect if fail
        }
        // consider adding means[][] as an out parameter - the final means could be computed
        // the final means are useful in some scenarios (e.g., discretization and RBF centroids)
        // and even though you can compute final means from final clustering, in some cases it
        // makes sense to return the means (at the expense of some method signature uglinesss)
        //
        // another alternative is to return, as an out parameter, some measure of cluster goodness
        // such as the average distance between cluster means, or the average distance between tuples in 
        // a cluster, or a weighted combination of both
        return (clustering, data, means);
    }

    private static double[][] Normalized(double[][] rawData)
    {
        // normalize raw data by computing (x - mean) / stddev
        // primary alternative is min-max:
        // v' = (v - min) / (max - min)

        // make a copy of input data
        var result = new double[rawData.Length][];
        for (var i = 0; i < rawData.Length; ++i)
        {
            result[i] = new double[rawData[i].Length];
            Array.Copy(rawData[i], result[i], rawData[i].Length);
        }

        for (var j = 0; j < result[0].Length; ++j) // each col
        {
            var colSum = result.Sum(t => t[j]);
            var mean = colSum / result.Length;
            var sum = result.Sum(t => (t[j] - mean) * (t[j] - mean));
            var sd = sum / result.Length;
            foreach (var t in result)
            {
                t[j] = (t[j] - mean) / sd;
            }
        }
        return result;
    }

    private static int[] InitClustering(int numTuples, int numClusters, int randomSeed)
    {
        // init clustering semi-randomly (at least one tuple in each cluster)
        var random = new Random(randomSeed);
        var clustering = new int[numTuples];
        for (var i = 0; i < numClusters; ++i) // make sure each cluster has at least one tuple
        {
            clustering[i] = i;
        }

        for (var i = numClusters; i < clustering.Length; ++i)
        {
            clustering[i] = random.Next(0, numClusters); // other assignments random
        }
                
        return clustering;
    }

    private static double[][] Allocate(int numClusters, int numColumns)
    {
        // convenience matrix allocator for Cluster()
        var result = new double[numClusters][];
        for (var i = 0; i < numClusters; ++i)
        {
            result[i] = new double[numColumns];
        }
        return result;
    }

    private static bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
    {
        // returns false if there is a cluster that has no tuples assigned to it
        // parameter means[][] is really a ref parameter

        // check existing cluster counts
        // can omit this check if InitClustering and UpdateClustering
        // both guarantee at least one tuple in each cluster (usually true)
        var numClusters = means.Length;
        var clusterCounts = new int[numClusters];
        for (var i = 0; i < data.Length; ++i)
        {
            var cluster = clustering[i];
            ++clusterCounts[cluster];
        }

        for (var i = 0; i < numClusters; ++i)
        {
            if (clusterCounts[i] == 0)
                return false; // bad clustering. no change to means[][]
        }


        // update, zero-out means so it can be used as scratch matrix 
        foreach (var t in means)
        {
            for (var j = 0; j < t.Length; ++j)
            {
                t[j] = 0.0;
            }
        }
            
        for (var i = 0; i < data.Length; ++i)
        {
            var cluster = clustering[i];
            for (var j = 0; j < data[i].Length; ++j)
            {
                means[cluster][j] += data[i][j]; // accumulate sum
            }
        }

        for (var k = 0; k < means.Length; ++k)
        {
            for (var j = 0; j < means[k].Length; ++j)
            {
                means[k][j] /= clusterCounts[k]; // danger of div by 0
            }
        }
                
        return true;
    }

    private static bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
    {
        // (re)assign each tuple to a cluster (closest mean)
        // returns false if no tuple assignments change OR
        // if the reassignment would result in a clustering where
        // one or more clusters have no tuples.

        var numClusters = means.Length;
        var isChanged = false;

        var newClustering = new int[clustering.Length]; // proposed result
        Array.Copy(clustering, newClustering, clustering.Length);

        var distances = new double[numClusters]; // distances from curr tuple to each mean

        for (var i = 0; i < data.Length; ++i) // walk thru each tuple
        {
            for (var k = 0; k < numClusters; ++k)
            {
                distances[k] = MathOperations.Distance(data[i], means[k]); // compute distances from curr tuple to all k means
            }

            var newClusterId = MinIndex(distances); // find closest mean ID
            if (newClusterId == newClustering[i]) continue;
            isChanged = true;
            newClustering[i] = newClusterId; // update
        }

        if (isChanged == false)
            return false; // no change so bail and don't update clustering[][]

        // check proposed clustering[] cluster counts
        var clusterCounts = new int[numClusters];
        for (var i = 0; i < data.Length; ++i)
        {
            var cluster = newClustering[i];
            ++clusterCounts[cluster];
        }

        for (var k = 0; k < numClusters; ++k)
        {
            if (clusterCounts[k] == 0)
                return false; // bad clustering. no change to clustering[][]
        }

        Array.Copy(newClustering, clustering, newClustering.Length); // update
        return true; // good clustering and at least one change
    }
        
    private static int MinIndex(double[] distances)
    {
        // index of smallest value in array
        // helper for UpdateClustering()
        var indexOfMin = 0;
        var smallDist = distances[0];
        for (var k = 0; k < distances.Length; ++k)
        {
            if (distances[k] < smallDist)
            {
                smallDist = distances[k];
                indexOfMin = k;
            }
        }
        return indexOfMin;
    }
}