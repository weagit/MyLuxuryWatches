using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using MyApp.Model;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.ViewModel;

public partial class StatsViewModel : BaseViewModel
{
    [ObservableProperty]
    private Chart brandChart;

    [ObservableProperty]
    private decimal totalCollectionValue;

    [ObservableProperty]
    private int totalWatchCount;

    [ObservableProperty]
    private decimal averageWatchValue;

    [ObservableProperty]
    private string mostValuableBrand;

    public ObservableCollection<BrandStatistic> BrandStatistics { get; private set; } = new();

    public StatsViewModel()
    {
        LoadBrandStatistics();
        CreateLuxuryChart();
        CalculateCollectionMetrics();
    }

    private void LoadBrandStatistics()
    {
        // Clear existing statistics
        BrandStatistics.Clear();

        // Calculate brand statistics
        var statistics = Globals.MyWatches
            .GroupBy(w => w.Brand)
            .Select(g => new BrandStatistic
            {
                BrandName = g.Key,
                WatchCount = g.Count(),
                TotalValue = g.Sum(w => w.Price),
                AverageValue = g.Average(w => w.Price),
                HighestValue = g.Max(w => w.Price)
            })
            .OrderByDescending(bs => bs.TotalValue)
            .ToList();

        // Add statistics to observable collection
        foreach (var stat in statistics)
        {
            BrandStatistics.Add(stat);
        }
    }

    private void CalculateCollectionMetrics()
    {
        TotalCollectionValue = BrandStatistics.Sum(bs => bs.TotalValue);
        TotalWatchCount = BrandStatistics.Sum(bs => bs.WatchCount);
        AverageWatchValue = TotalWatchCount > 0 ? TotalCollectionValue / TotalWatchCount : 0;
        MostValuableBrand = BrandStatistics.OrderByDescending(bs => bs.TotalValue).FirstOrDefault()?.BrandName ?? "N/A";
    }

    private void CreateLuxuryChart()
    {
        // Palette élégante avec des couleurs luxueuses et plus contrastées
        string[] luxuryColors = new[]
        {
            "#FFD700",  // Or
            "#C0C0C0",  // Argent
            "#B87333",  // Cuivre
            "#E6BE8A",  // Champagne
            "#4682B4",  // Bleu acier
            "#800020",  // Bordeaux
            "#2F4F4F",  // Gris ardoise foncé
            "#006400",  // Vert foncé
            "#4B0082",  // Indigo
            "#8B4513",  // Brun
            "#191970",  // Bleu minuit
            "#4A4A4A"   // Gris charbon
        };

        var entries = new List<ChartEntry>();

        // Create chart entries for each brand
        for (int i = 0; i < BrandStatistics.Count; i++)
        {
            var stat = BrandStatistics[i];
            // Utiliser la longueur du tableau pour éviter l'erreur d'index
            string color = luxuryColors[i % luxuryColors.Length];

            entries.Add(new ChartEntry((float)stat.TotalValue)
            {
                Label = stat.BrandName,
                ValueLabel = $"£{stat.TotalValue:N0}",
                Color = SKColor.Parse(color),
                TextColor = SKColors.White,
                ValueLabelColor = SKColor.Parse("#FFD700")
            });
        }

        // Conserver le RadialGaugeChart comme demandé mais avec des ajustements pour une meilleure visibilité
        BrandChart = new RadialGaugeChart
        {
            Entries = entries,
            BackgroundColor = SKColors.Transparent,
            AnimationDuration = TimeSpan.FromMilliseconds(1200),
            IsAnimated = true,
            MaxValue = (float)(entries.Sum(x => x.Value) * 1.05), // Légère marge pour éviter la saturation
            LabelTextSize = 40, // Taille de texte réduite pour éviter les chevauchements
            LabelColor = SKColors.White,
            StartAngle = 0,
            LineAreaAlpha = 200, // Plus opaque pour une meilleure visibilité
            LineSize = 50 // Épaisseur de ligne plus importante
        };
    }

    [RelayCommand]
    internal async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}

// Classe représentant les statistiques par marque
public class BrandStatistic
{
    public string BrandName { get; set; }
    public int WatchCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AverageValue { get; set; }
    public decimal HighestValue { get; set; }
}