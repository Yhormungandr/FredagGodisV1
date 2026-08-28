using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
// för att få sökvägen till filen att funka behöver man skapa en mapp som heter data i bin/debug/net10.0 och lägga in filen Countries_area.txt där så funkar det utan att ha en local sökväg

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

class Program
{
    static void Main(string[] args)
    {
        string filePath = Path.Combine(
            AppContext.BaseDirectory,
            "data",
            "Countries_area.txt"
        );

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
}
