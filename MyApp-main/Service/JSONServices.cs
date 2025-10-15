using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyApp.Service;

public class JSONServices
{
    internal async Task<List<Watch>> GetWatches()
    {
        // URL du serveur distant où se trouve le fichier JSON
        var url = "https://185.157.245.38:5000/json?FileName=MyWatches.json";

        List<Watch> myList = new();

        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using HttpClient _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // Timeout raisonnable

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync();

                myList = JsonSerializer.Deserialize<List<Watch>>(content) ?? new List<Watch>();
            }
            else
            {
                await Shell.Current.DisplayAlert("Server Error",
                    $"Unable to fetch data. Code: {response.StatusCode}", "OK");
            }
        }
        catch (HttpRequestException ex)
        {
            await Shell.Current.DisplayAlert("Network Error",
                "Could not connect to the server. Please check your internet connection.", "OK");
            Console.WriteLine($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            await Shell.Current.DisplayAlert("Timeout",
                "Connection to the server took too long. Please try again later.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"An error occurred while loading data: {ex.Message}", "OK");
            Console.WriteLine($"Exception: {ex}");
        }

        return myList;
    }

    internal async Task SetWatches(List<Watch> myList)
    {
        // URL du serveur distant pour l'envoi du fichier
        var url = "https://185.157.245.38:5000/json";

        try
        {
            MemoryStream mystream = new();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using HttpClient _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(15); // Timeout plus long pour l'upload

            JsonSerializer.Serialize(mystream, myList);
            mystream.Position = 0;

            var fileContent = new ByteArrayContent(mystream.ToArray());

            var content = new MultipartFormDataContent
            {
                { fileContent, "file", "MyWatches.json" }
            };

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Success", "Watches saved to server successfully", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Server Error", $"Status code: {response.StatusCode}", "OK");
            }
        }
        catch (HttpRequestException ex)
        {
            await Shell.Current.DisplayAlert("Network Error",
                "Could not connect to the server to save data. Please check your internet connection.", "OK");
            Console.WriteLine($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            await Shell.Current.DisplayAlert("Timeout",
                "Connection to the server took too long. Please try again later.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"An error occurred while saving data: {ex.Message}", "OK");
            Console.WriteLine($"Exception: {ex}");
        }
    }
}
