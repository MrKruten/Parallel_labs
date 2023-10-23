using ScottPlot;

namespace Lab2Cluster;

public static class Chart
{
    public static void Create(int[] clustering, double[][] data, int numClusters)
    {
        var plt = new Plot();

        for (var k = 0; k < numClusters; ++k)
        {
            var x = new List<double>();
            var y = new List<double>();
            for (var i = 0; i < data.Length; ++i)
            {
                var clusterId = clustering[i];
                if (clusterId != k) continue;

                x.Add(data[i][0]);
                y.Add(data[i][1]);
            }
            plt.AddScatter(x.ToArray(), y.ToArray(), lineWidth: 0, label: $"Cluster: {k+1}");
        }

        plt.XLabel("Hct mean");
        plt.YLabel("Urine mean");
        plt.Legend();

        plt.SaveFig(@".\chart.png");
    }
}