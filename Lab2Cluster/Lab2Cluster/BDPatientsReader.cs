using System.Globalization;

namespace Lab2Cluster;

public static class BDPatientsReader
{
    public static double[][] ReadFile(string path, int countRows)
    {
        if(string.IsNullOrEmpty(path) || countRows < 1) return Array.Empty<double[]>();
        var parsed = new List<PatientRow>();

        using (var reader = new StreamReader(path))
        {
            var currentRowIndex = 1;
            const string hctMeanName = "HCT_mean";
            const string urineMeanMeanName = "Urine_mean";

            var row = reader.ReadLine();
            if (row == null)
            {
                return Array.Empty <double[]>();
            }

            var headers = row.Split(',');
            var htcMeanIndex = Array.IndexOf(headers, hctMeanName);
            var urineMeanIndex = Array.IndexOf(headers, urineMeanMeanName);

            while ((row = reader.ReadLine()) != null)
            {
                var columns = row.Split(",");
                parsed.Add(new PatientRow(columns[htcMeanIndex], columns[urineMeanIndex]));
                currentRowIndex++;
                if (currentRowIndex >= countRows) break;
            }
        }

        var rawData = new double[parsed.Count][];
        for (var i = 0; i < parsed.Count; i++)
        {
            var el = parsed[i];
            rawData[i] = new[] { el.HctMean, el.UrineMean };
        }

        return rawData;
    }
}

public class PatientRow
{
    public double HctMean { get; set; }
    public double UrineMean { get; set; }

    public PatientRow(string hctMean, string urineMean)
    {
        HctMean = string.IsNullOrEmpty(hctMean) ? 0 : double.Parse(hctMean, CultureInfo.InvariantCulture.NumberFormat);
        UrineMean = string.IsNullOrEmpty(urineMean) ? 0 : double.Parse(urineMean, CultureInfo.InvariantCulture.NumberFormat);
    }
}