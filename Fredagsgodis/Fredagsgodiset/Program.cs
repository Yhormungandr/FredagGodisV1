using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
class program
{

    static void Main(string[] args)
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "data", "Countries_area.txt");

        try
        {
            
            Console.WriteLine("--- Alla beräknade befolkningstätheter ---");
            List<double> areasInSqm = BefolkningPerkvadratkilometer(filePath);
            foreach (double area in areasInSqm)
            {
                Console.WriteLine($"{area:N1} inv/km²");
            }

            Console.WriteLine(); // Tom rad för struktur

            
            double threshold = 100.0;
            Console.WriteLine($"--- Länder med en befolkningstäthet över {threshold} invånare/km² ---");
            List<(string CountryName, double Density)> denseCountries = FiltreraPerAntal(filePath, threshold);
            foreach (var country in denseCountries)
            {
                Console.WriteLine($"{country.CountryName}: {country.Density:N1} inv/km²");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ett fel uppstod: {ex.Message}");
        }
    }

    public static List<double> BefolkningPerkvadratkilometer(string filePath)
    {
        return File.ReadLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            //.Skip(1)
            .Select(line => line.Split(';'))
            .Select(columns =>
            {
                // Kontrollera att båda kolumnerna finns innan bearbetning
                if (columns.Length > 4)
                {
                    string val1Str = columns[1].Trim();
                    string val4Str = columns[4].Trim();

                    // Parsar båda värdena separat
                    bool p1 = double.TryParse(val1Str, NumberStyles.Any, CultureInfo.InvariantCulture, out double val1) ||
                              double.TryParse(val1Str, NumberStyles.Any, CultureInfo.CurrentCulture, out val1);

                    bool p2 = double.TryParse(val4Str, NumberStyles.Any, CultureInfo.InvariantCulture, out double val2) ||
                              double.TryParse(val4Str, NumberStyles.Any, CultureInfo.CurrentCulture, out val2);

                    // Kontrollera att båda lyckades och att vi inte dividerar med noll
                    if (p1 && p2 && val2 != 0)
                    {
                        double result = val1 / val2;
                        return result * 1; // Behålls om du fortfarande vill göra om till kvadratmeter
                    }
                }
                return (double?)null;
            })
            //// Sortera bort rader där konverteringen misslyckades
            .Where(area => area.HasValue)
            .Select(area => area.Value)
            .ToList();
    }

    public static List<(string CountryName, double Density)> FiltreraPerAntal(string filePath, double threshold)
    {
        return File.ReadLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            //.Skip(1)
            .Select(line => line.Split(';'))
            .Select(columns => {
                // Kontrollera att kolumn 0 (namn), 1 och 4 existerar
                if (columns.Length > 4)
                {
                    string countryName = columns[0].Trim();
                    string val1Str = columns[1].Trim();
                    string val4Str = columns[4].Trim();

                    bool p1 = double.TryParse(val1Str, NumberStyles.Any, CultureInfo.InvariantCulture, out double val1) ||
                              double.TryParse(val1Str, NumberStyles.Any, CultureInfo.CurrentCulture, out val1);

                    bool p2 = double.TryParse(val4Str, NumberStyles.Any, CultureInfo.InvariantCulture, out double val2) ||
                              double.TryParse(val4Str, NumberStyles.Any, CultureInfo.CurrentCulture, out val2);

                    if (p1 && p2 && val2 != 0)
                    {
                        double density = val1 / val2;
                        return (CountryName: countryName, Density: density);
                    }
                }
                return ((string CountryName, double Density)?)null;
            })
            .Where(item => item.HasValue)
            .Select(item => item.Value)
            // Filtrerar fram länder över tröskelvärdet
            .Where(item => item.Density > threshold)
            .ToList();
    }
}
