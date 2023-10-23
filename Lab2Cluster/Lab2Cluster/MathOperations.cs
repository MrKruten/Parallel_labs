namespace Lab2Cluster;

public static class MathOperations
{
    public static double Distance(double[] tuple, double[] mean)
    {
        // Euclidean distance between two vectors
        var sumSquaredDiffs = tuple.Select((t, j) => Math.Pow((t - mean[j]), 2)).Sum();
        return Math.Sqrt(sumSquaredDiffs);
    }
}