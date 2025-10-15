using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MyApp.ViewModel;

public partial class MainViewModel(JSONServices MyJSONService, DeviceOrientationService deviceService, MongoUserService mongoService) : BaseViewModel
{
    private readonly DeviceOrientationService _deviceService = deviceService;
    private readonly MongoUserService _mongoUserService = mongoService;
    private readonly JSONServices _jsonServices = MyJSONService;
    private CSVServices _csvServices = new CSVServices();

    [ObservableProperty]
    private bool isUserLoggedIn;

    [ObservableProperty]
    private bool isUserAdmin;

    // Collection observable affichant les montres dans l'interface
    public ObservableCollection<Watch> MyObservableList { get; } = new ObservableCollection<Watch>();

    public Dictionary<string, int> UserRatings { get; set; } = new();

    public async Task LoadUserRatings()
    {
        if (CurrentUserManager.CurrentUser != null)
        {
            var user = await _mongoUserService.GetUserById(CurrentUserManager.CurrentUser.Id);
            if (user != null && user.WatchRatings != null)
            {
                UserRatings = user.WatchRatings;
            }
        }
    }

    // Cette méthode sera appelée à chaque chargement de page ou changement d'état d'authentification
    public void UpdatePermissions()
    {
        IsUserLoggedIn = CurrentUserManager.IsAuthenticated;
        IsUserAdmin = CurrentUserManager.IsAdmin;
    }

    // Commande de navigation vers la vue de détails pour une montre donnée (en passant son ID)
    [RelayCommand]
    internal async Task GoToDetails(string id)
    {
        if (!IsUserAdmin)
        {
            await Shell.Current.DisplayAlert("Access Denied", "You must be an administrator to view details.", "OK");
            return;
        }

        IsBusy = true;
        await Shell.Current.GoToAsync("DetailsView", true, new Dictionary<string, object>
        {
            { "selectedWatch", id }
        });
        IsBusy = false;
    }


    // Commande pour charger la collection de montres depuis un fichier JSON
   [RelayCommand]
    internal async Task LoadJSON()
    {
        IsBusy = true;
        try
        {
            Globals.MyWatches = await _jsonServices.GetWatches();

            if (Globals.MyWatches == null || !Globals.MyWatches.Any())
            {
                await Shell.Current.DisplayAlert("Error", "No watches were loaded from the JSON file.", "OK");
                return;
            }

            Console.WriteLine($"Loaded {Globals.MyWatches.Count} watches from JSON");

            MyObservableList.Clear();
            foreach (var item in Globals.MyWatches)
            {
                MyObservableList.Add(item);
            }

            Console.WriteLine($"Observable collection now contains {MyObservableList.Count} watches");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading watches: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Unable to load watches: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    // Commande pour naviguer vers la page d'ajout d'une nouvelle montre
    [RelayCommand]
    internal async Task GoToAddWatch()
    {
        IsBusy = true;
        await Shell.Current.GoToAsync("AddWatchView");
        IsBusy = false;
    }

    // Nouvelle commande pour scanner un objet via l'interface hardware (ou émulateur)
    [RelayCommand]
    internal async Task ScanObject()
    {
        IsBusy = true;

        try
        {
            Console.WriteLine("Attempting to open scanner port...");
            _deviceService.OpenPort();

            Console.WriteLine("Waiting for scan...");
            string scannedId = await WaitForScanAsync();

            Console.WriteLine($"Scanned ID: {scannedId}");

            if (string.IsNullOrEmpty(scannedId))
            {
                await Shell.Current.DisplayAlert("Timeout", "No code scanned within the time limit.", "OK");
                return;
            }

            string cleanedId = CleanScannedId(scannedId);
            Console.WriteLine($"Cleaned ID: {cleanedId}");

            var matchingWatch = Globals.MyWatches.FirstOrDefault(w =>
                CleanScannedId(w.Id) == cleanedId);

            if (matchingWatch != null)
            {
                Console.WriteLine($"Matching watch found: {matchingWatch.Brand} {matchingWatch.Model}");
                await Shell.Current.GoToAsync("DetailsView", true, new Dictionary<string, object>
                {
                    { "selectedWatch", matchingWatch.Id }
                });
            }
            else
            {
                Console.WriteLine($"No watch found for ID: {cleanedId}");
                await Shell.Current.DisplayAlert("Not Found", "No watch matches the scanned code.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ScanObject: {ex}");
            await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    // Méthode pour nettoyer l'ID scanné
    private string CleanScannedId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return string.Empty;

        // Supprimer les espaces, les caractères de contrôle, etc.
        return new string(id.Where(c => !char.IsWhiteSpace(c) && char.IsLetterOrDigit(c)).ToArray());
    }

    // Méthode qui attend qu'une donnée soit disponible dans le SerialBuffer
    private async Task<string> WaitForScanAsync()
    {
        var tcs = new TaskCompletionSource<string>();

        // Gestionnaire pour l'événement Changed du SerialBuffer
        EventHandler handler = null;
        handler = (s, e) =>
        {
            // Vérifie si une donnée est disponible dans le buffer
            if (_deviceService.SerialBuffer.Count > 0)
            {
                // On récupère la première donnée scannée
                string? scanned = _deviceService.SerialBuffer.Dequeue()?.ToString();
                if (!string.IsNullOrWhiteSpace(scanned))
                {
                    tcs.TrySetResult(scanned.Trim());
                    // On se désabonne pour éviter d'autres déclenchements
                    _deviceService.SerialBuffer.Changed -= handler;
                }
            }
        };

        // On s'abonne à l'événement
        _deviceService.SerialBuffer.Changed += handler;

        // On attend soit le scan, soit le timeout de 10 secondes
        var timer = Task.Delay(TimeSpan.FromSeconds(10));
        var completedTask = await Task.WhenAny(tcs.Task, timer);

        if (completedTask == timer)
        {
            // Timeout atteint, on renvoie une chaîne vide
            tcs.TrySetResult(string.Empty);
            // On se désabonne pour éviter des déclenchements ultérieurs
            _deviceService.SerialBuffer.Changed -= handler;
        }

        return await tcs.Task;
    }

    [RelayCommand]
    internal async Task ExportToCSV()
    {
        IsBusy = true;
        try
        {
            await _csvServices.PrintData(Globals.MyWatches);
            await Shell.Current.DisplayAlert("Export", "Watch collection exported successfully to desktop!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    internal async Task ImportFromCSV()
    {
        IsBusy = true;
        try
        {
            var importedWatches = await _csvServices.LoadData();
            if (importedWatches.Any())
            {
                int initialCount = Globals.MyWatches.Count;
                SynchronizeWatchCollection(importedWatches);
                int addedCount = Globals.MyWatches.Count - initialCount;
                int updatedCount = importedWatches.Count - addedCount;

                await _jsonServices.SetWatches(Globals.MyWatches);
                RefreshPage();

                await Shell.Current.DisplayAlert("Import",
                    $"Collection updated: {addedCount} watches added, {updatedCount} watches updated.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Import", "No watches were imported.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Import Error", $"An error occurred during import: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Méthode pour synchroniser la collection
    private void SynchronizeWatchCollection(List<Watch> importedWatches)
    {
        // Supprimer les doublons de la liste importée
        importedWatches = importedWatches.DistinctBy(w => w.Id).ToList();

        // Ajouter ou mettre à jour seulement les montres importées
        foreach (var importedWatch in importedWatches)
        {
            var existingWatch = Globals.MyWatches.FirstOrDefault(w => w.Id == importedWatch.Id);

            if (existingWatch != null)
            {
                // Mettre à jour l'élément existant
                existingWatch.Brand = importedWatch.Brand;
                existingWatch.Model = importedWatch.Model;
                existingWatch.Type = importedWatch.Type;
                existingWatch.Material = importedWatch.Material;
                existingWatch.Year = importedWatch.Year;
                existingWatch.Price = importedWatch.Price;
                existingWatch.IsLimitedEdition = importedWatch.IsLimitedEdition;
                existingWatch.Picture = importedWatch.Picture;
            }
            else
            {
                // Ajouter un nouvel élément
                Globals.MyWatches.Add(importedWatch);
            }
        }
    }

    [RelayCommand]
    internal async Task GoToStats()
    {
        IsBusy = true;
        await Shell.Current.GoToAsync("StatsView");
        IsBusy = false;
    }

    [RelayCommand]
    internal async Task GoToUsers()
    {
        IsBusy = true;

        if (!CurrentUserManager.IsAuthenticated)
        {
            await Shell.Current.DisplayAlert("Access Denied", "You must be logged in to access this page.", "OK");
            await Shell.Current.GoToAsync("LoginView");
        }
        else if (!CurrentUserManager.IsAdmin)
        {
            await Shell.Current.DisplayAlert("Access Denied", "You must be an administrator to access this page.", "OK");
        }
        else
        {
            await Shell.Current.GoToAsync("UsersView");
        }

        IsBusy = false;
    }

    [RelayCommand]
    internal async Task Logout()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            if (CurrentUserManager.IsAuthenticated)
            {
                bool answer = await Shell.Current.DisplayAlert("Logout",
                    "Are you sure you want to log out?", "Yes", "No");

                if (answer)
                {
                    CurrentUserManager.Logout();
                    await Shell.Current.DisplayAlert("Logout", "You have been successfully logged out.", "OK");
                    await Shell.Current.GoToAsync("//MainView");
                }
            }
            else
            {
                await Shell.Current.GoToAsync("LoginView");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Méthode pour obtenir l'image à afficher pour chaque étoile en fonction de la note de l'utilisateur
    // Méthode simplifiée pour obtenir l'image de l'étoile
    public string GetRatingImage(string parameter)
    {
        try
        {
            // Expected format: "starPosition|watchId"
            if (string.IsNullOrEmpty(parameter) || !parameter.Contains("|"))
                return "star_empty.png";

            var parts = parameter.Split('|');
            if (parts.Length != 2)
                return "star_empty.png";

            if (!int.TryParse(parts[0], out int starPosition))
                return "star_empty.png";

            string watchId = parts[1];

            // If the user has already rated this watch
            if (UserRatings.TryGetValue(watchId, out int userRating))
            {
                // If the star position is less than or equal to the rating
                // Return the filled star image, otherwise return the empty star image
                return starPosition <= userRating ? "star_filled.png" : "star_empty.png";
            }

            // By default, return an empty star
            return "star_empty.png";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRatingImage: {ex.Message}");
            return "star_empty.png";
        }
    }


    // RateWatch method
    [RelayCommand]
    public async Task RateWatch(string parameter)
    {
        if (IsBusy || string.IsNullOrEmpty(parameter))
            return;

        try
        {
            IsBusy = true;

            // Expected format: "watchId|rating"
            var parts = parameter.Split('|');
            if (parts.Length != 2)
                return;

            string watchId = parts[0];
            if (!int.TryParse(parts[1], out int rating))
                return;

            // Check if the user is logged in
            var userId = CurrentUserManager.CurrentUser?.Id;
            if (userId == null)
            {
                await Shell.Current.DisplayAlert("Login Required",
                    "You need to be logged in to rate watches.", "OK");
                await Shell.Current.GoToAsync("LoginView");
                return;
            }

            // Check if the user has already rated this watch
            bool isUpdate = UserRatings.ContainsKey(watchId);

            // Save the rating in the database
            await _mongoUserService.SetRatingForWatchAsync(userId, watchId, rating);

            // Immediately update the local rating collection
            if (UserRatings.ContainsKey(watchId))
                UserRatings[watchId] = rating;
            else
                UserRatings.Add(watchId, rating);

            // 1. Directly update the stars in the global collection
            var watchInGlobals = Globals.MyWatches.FirstOrDefault(w => w.Id == watchId);
            if (watchInGlobals != null)
            {
                watchInGlobals.Star1 = rating >= 1 ? "star_filled.png" : "star_empty.png";
                watchInGlobals.Star2 = rating >= 2 ? "star_filled.png" : "star_empty.png";
                watchInGlobals.Star3 = rating >= 3 ? "star_filled.png" : "star_empty.png";
                watchInGlobals.Star4 = rating >= 4 ? "star_filled.png" : "star_empty.png";
                watchInGlobals.Star5 = rating >= 5 ? "star_filled.png" : "star_empty.png";
            }

            // Find the watch information for a personalized message
            var ratedWatch = Globals.MyWatches.FirstOrDefault(w => w.Id == watchId);
            string watchInfo = ratedWatch != null ? $"{ratedWatch.Brand} {ratedWatch.Model}" : "watch";

            // Show a different message depending on whether it's an update or a new rating
            if (isUpdate)
            {
                await Shell.Current.DisplayAlert(
                    "Rating Updated",
                    $"Thank you for updating your appreciation of the {watchInfo}.\nYour new rating: {rating}/5",
                    "Close");
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Rating Submitted",
                    $"Thank you for your appreciation of the {watchInfo}.\nYour rating: {rating}/5",
                    "Close");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Refreshes the observable collection by syncing with the global watch collection
    internal void RefreshPage()
    {
        try
        {
            UpdatePermissions();

            // Check if Globals.MyWatches is initialized
            if (Globals.MyWatches == null)
            {
                Console.WriteLine("Warning: Globals.MyWatches is null in RefreshPage");
                return;
            }

            Console.WriteLine($"RefreshPage: {Globals.MyWatches.Count} watches to display");

            MyObservableList.Clear();
            foreach (var item in Globals.MyWatches)
            {
                item.Star1 = GetRatingImage($"1|{item.Id}");
                item.Star2 = GetRatingImage($"2|{item.Id}");
                item.Star3 = GetRatingImage($"3|{item.Id}");
                item.Star4 = GetRatingImage($"4|{item.Id}");
                item.Star5 = GetRatingImage($"5|{item.Id}");

                MyObservableList.Add(item);
            }

            Console.WriteLine($"RefreshPage: Observable collection now contains {MyObservableList.Count} watches");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RefreshPage: {ex.Message}");
        }
    }
}