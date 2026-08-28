using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
class program
{
    static string filePath = Path.Combine(AppContext.BaseDirectory, "data", "Countries_area.txt");

    class Country
    {
        public string Name { get; set; }
        public double GDP { get; set; }
        public long Population { get; set; }
        public string Currency { get; set; }
        public double Area { get; set; }

        public double GDPPerCapita
        {
            get
            {
                return (GDP * 1_000_000_000) / Population;
            }
        }

        public double PopulationDensity
        {
            get
            {
                return Population / Area;
            }
        }
    }

    class EconomicResult
    {
        public string Country { get; set; }
        public double GDPPerCapita { get; set; }
        public double PopulationDensity { get; set; }
        public double GDP { get; set; }
        public double EconomicIndex { get; set; }
    }

    static void Main(string[] args)
    {

        Jesper();

        Gustaf();

        Fredrik();

    }

    //Jesper
    public static void Jesper()
    {
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

    //Jesper
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

    //Jesper
    public static List<(string CountryName, double Density)> FiltreraPerAntal(string filePath, double threshold)
    {
        return File.ReadLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            //.Skip(1)
            .Select(line => line.Split(';'))
            .Select(columns =>
            {
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

    //Gustaf
    public static void Gustaf()
    {

        try
        {
            List<Country> countries = ReadCountries(filePath);

            if (countries.Count == 0)
            {
                Console.WriteLine("Inga giltiga länder hittades i filen.");
                return;
            }

            Console.WriteLine(
                $"Antal inlästa länder: {countries.Count}");

            Console.WriteLine();

            // 1. Visa alla befolkningstätheter
            ShowPopulationDensities(countries);

            // 2. Visa länder med befolkningstäthet över 100
            ShowDenseCountries(countries, 100.0);

            // 3. Analys av valuta
            ShowCurrencyAnalysis(countries);

            // 4. Ekonomisk ranking
            ShowEconomicRanking(countries);

            // 5. Total GDP
            ShowTotalGDP(countries);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine(
                $"Filen '{filePath}' kunde inte hittas.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Ett fel uppstod: {ex.Message}");
        }
    }

    //Gustaf
    // Läser in länder från filen
    static List<Country> ReadCountries(string fileName)
    {
        var countries = new List<Country>();

        foreach (string line in File.ReadLines(fileName))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(';');

            if (parts.Length < 5)
                continue;

            string name = parts[0].Trim();
            string currency = parts[3].Trim();

            bool validGDP =
                double.TryParse(
                    parts[1].Trim(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double gdp);

            bool validPopulation =
                long.TryParse(
                    parts[2].Trim(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out long population);

            bool validArea =
                double.TryParse(
                    parts[4].Trim(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double area);

            if (!validGDP ||
                !validPopulation ||
                !validArea ||
                population <= 0 ||
                area <= 0)
            {
                continue;
            }

            countries.Add(new Country
            {
                Name = name,
                GDP = gdp,
                Population = population,
                Currency = currency,
                Area = area
            });
        }

        return countries;
    }

    //Gustaf
    // Visar alla beräknade befolkningstätheter
    static void ShowPopulationDensities(List<Country> countries)
    {
        Console.WriteLine(
            "--- Alla beräknade befolkningstätheter ---");

        foreach (Country country in countries)
        {
            Console.WriteLine(
                $"{country.Name}: " +
                $"{country.PopulationDensity:N1} inv/km²");
        }
    }

    //Gustaf
    // Filtrerar fram länder med hög befolkningstäthet
    static void ShowDenseCountries(
        List<Country> countries,
        double threshold)
    {
        Console.WriteLine();

        Console.WriteLine(
            $"--- Länder med en befolkningstäthet över " +
            $"{threshold} invånare/km² ---");

        var denseCountries = countries
            .Where(c => c.PopulationDensity > threshold)
            .OrderByDescending(c => c.PopulationDensity)
            .ToList();

        foreach (Country country in denseCountries)
        {
            Console.WriteLine(
                $"{country.Name}: " +
                $"{country.PopulationDensity:N1} inv/km²");
        }
    }

    //Gustaf
    // Hittar den valuta som används av flest länder
    static string GetMostUsedCurrency(
        List<Country> countries)
    {
        return countries
            .GroupBy(c => c.Currency)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .FirstOrDefault();
    }

    //Gustaf
    // Räknar ut genomsnittlig GDP per capita
    static double CalculateAverageGDPPerCapita(
        List<Country> countries,
        string currency)
    {
        return countries
            .Where(c => c.Currency == currency)
            .Select(c => c.GDPPerCapita)
            .Average();
    }

    //Gustaf
    // Räknar ut medianen för GDP per capita
    static double CalculateMedianGDPPerCapita(
        List<Country> countries,
        string currency)
    {
        var values = countries
            .Where(c => c.Currency == currency)
            .Select(c => c.GDPPerCapita)
            .OrderBy(value => value)
            .ToList();

        if (values.Count == 0)
            return 0;

        int middle = values.Count / 2;

        if (values.Count % 2 != 0)
        {
            return values[middle];
        }

        return (values[middle - 1] + values[middle]) / 2;
    }

    //Gustaf
    // Visar valutaanalysen
    static void ShowCurrencyAnalysis(
        List<Country> countries)
    {
        string mostUsedCurrency =
            GetMostUsedCurrency(countries);

        if (mostUsedCurrency == null)
        {
            Console.WriteLine(
                "Ingen valuta kunde hittas.");

            return;
        }

        double averageGDPPerCapita =
            CalculateAverageGDPPerCapita(
                countries,
                mostUsedCurrency);

        double medianGDPPerCapita =
            CalculateMedianGDPPerCapita(
                countries,
                mostUsedCurrency);

        Console.WriteLine();

        Console.WriteLine(
            $"Mest använda valuta: {mostUsedCurrency}");

        Console.WriteLine(
            $"Genomsnittlig GDP per capita: " +
            $"{averageGDPPerCapita:N2}");

        Console.WriteLine(
            $"Median GDP per capita: " +
            $"{medianGDPPerCapita:N2}");
    }

    //Gustaf
    // Beräknar Economic Index för alla länder
    static List<EconomicResult> CalculateEconomicIndex(
        List<Country> countries)
    {
        return countries
            .Select(c => new EconomicResult
            {
                Country = c.Name,

                GDPPerCapita = c.GDPPerCapita,

                PopulationDensity = c.PopulationDensity,

                GDP = c.GDP,

                EconomicIndex =
                    (c.GDPPerCapita * 0.5) +
                    (c.PopulationDensity * 0.3) +
                    (c.GDP * 0.2)
            })
            .OrderByDescending(x => x.EconomicIndex)
            .ToList();
    }

    //Gustaf
    // Visar ranking
    static void ShowEconomicRanking(
        List<Country> countries)
    {
        List<EconomicResult> results =
            CalculateEconomicIndex(countries);

        Console.WriteLine();

        Console.WriteLine(
            "--------------------------------------------");

        Console.WriteLine(
            "--- Ekonomisk ranking ---");

        Console.WriteLine(
            "--------------------------------------------");

        int rank = 1;

        foreach (EconomicResult result in results)
        {
            Console.WriteLine(
                $"{rank}. {result.Country}");

            Console.WriteLine(
                $"   GDP per capita: " +
                $"{result.GDPPerCapita:N2}");

            Console.WriteLine(
                $"   Befolkningstäthet: " +
                $"{result.PopulationDensity:N2} inv/km²");

            Console.WriteLine(
                $"   Total GDP: " +
                $"{result.GDP:N2} miljarder");

            Console.WriteLine(
                $"   Economic Index: " +
                $"{result.EconomicIndex:N2}");

            Console.WriteLine();

            rank++;
        }
    }

    //Gustaf
    // Räknar ut total GDP
    static double CalculateTotalGDP(
        List<Country> countries)
    {
        return countries
            .Select(c => c.GDP)
            .Aggregate(
                0.0,
                (total, gdp) => total + gdp
            );
    }

    //Gustaf
    // Visar total GDP
    static void ShowTotalGDP(
        List<Country> countries)
    {
        double totalGDP =
            CalculateTotalGDP(countries);

        Console.WriteLine();

        Console.WriteLine(
            $"Total GDP för alla länder: " +
            $"{totalGDP:N2} miljarder");
    }

    //Fredrik
    public static void Fredrik()
    {
        Console.WriteLine("----------------------------------------------------------------------------------");
        Console.WriteLine("Tjohej! Fredrik här och jag har lagt till en ny funktion här som knappt gör något.");
        Console.WriteLine("----------------------------------------------------------------------------------");
    }

}