using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Model;
using CommunityToolkit.Maui.Storage;

namespace MyApp.Service;

public class CSVServices
{
    public async Task<List<Watch>> LoadData()
    {
        List<Watch> list = [];

        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a watch CSV file"
            });

            if (result != null)
            {
                // Vérifier si le fichier est accessible en lecture
                try
                {
                    using var stream = await result.OpenReadAsync();
                    // Si on peut ouvrir le stream, le fichier est accessible
                }
                catch (IOException ex)
                {
                    await Shell.Current.DisplayAlert("Access Error",
                        "The CSV file is currently used by another program. Please close it and try again.", "OK");
                    return list;
                }

                try
                {
                    var lines = await File.ReadAllLinesAsync(result.FullPath, Encoding.UTF8);

                    if (lines.Length < 1)
                    {
                        await Shell.Current.DisplayAlert("Empty File",
                            "The CSV file contains no data.", "OK");
                        return list;
                    }

                    var headers = lines[0].Split(';');
                    var properties = typeof(Watch).GetProperties();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                            continue;

                        Watch watch = new();
                        var values = lines[i].Split(';');

                        for (int j = 0; j < headers.Length; j++)
                        {
                            var property = properties.FirstOrDefault(p =>
                                p.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase));

                            if (property != null && j < values.Length)
                            {
                                try
                                {
                                    // Handle special type conversions
                                    object value = property.PropertyType switch
                                    {
                                        var t when t == typeof(int) => int.TryParse(values[j], out var intVal) ? intVal : 0,
                                        var t when t == typeof(decimal) => decimal.TryParse(values[j], out var decVal) ? decVal : 0m,
                                        var t when t == typeof(bool) => bool.TryParse(values[j], out var boolVal) ? boolVal : false,
                                        _ => values[j]
                                    };

                                    property.SetValue(watch, value);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error while converting value for {property.Name}: {ex.Message}");
                                    // Continue avec la prochaine propriété
                                }
                            }
                        }

                        list.Add(watch);
                    }
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Read Error",
                        $"Unable to read the CSV file: {ex.Message}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"An error occurred while loading the CSV: {ex.Message}", "OK");
        }

        return list;
    }

    public async Task PrintData(List<Watch> data)
    {
        try
        {
            var csv = new StringBuilder();
            var properties = typeof(Watch).GetProperties();

            // Header: property names
            csv.AppendLine(string.Join(";", properties.Select(p => p.Name)));

            foreach (var watch in data)
            {
                var values = properties.Select(p =>
                    p.GetValue(watch)?.ToString() ?? "");
                csv.AppendLine(string.Join(";", values));
            }

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
            var fileSaverResult = await FileSaver.Default.SaveAsync("WatchCollection.csv", stream);

            if (!fileSaverResult.IsSuccessful)
            {
                await Shell.Current.DisplayAlert("Export Error",
                    "Could not save the CSV file.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export Error",
                $"An error occurred during CSV export: {ex.Message}", "OK");
        }
    }
}
