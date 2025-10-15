using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.ViewModel;

[QueryProperty(nameof(Id), "selectedWatch")]
public partial class DetailsViewModel : BaseViewModel
{
    [ObservableProperty]
    public partial string? Id { get; set; }

    [ObservableProperty]
    public partial string? Brand { get; set; }

    [ObservableProperty]
    public partial string? Model { get; set; }

    [ObservableProperty]
    public partial string? Type { get; set; }

    [ObservableProperty]
    public partial string? Material { get; set; }

    [ObservableProperty]
    public partial int Year { get; set; }

    [ObservableProperty]
    public partial decimal Price { get; set; }

    [ObservableProperty]
    public partial bool IsLimitedEdition { get; set; }

    [ObservableProperty]
    public partial string? Picture { get; set; }

    [ObservableProperty]
    public partial string? SerialBufferContent { get; set; }

    readonly DeviceOrientationService MyScanner;
    readonly JSONServices _jsonServices;

    // Injection des deux services via le constructeur
    public DetailsViewModel(DeviceOrientationService myScanner, JSONServices jsonServices)
    {
        MyScanner = myScanner;
        _jsonServices = jsonServices;
        // On ne ré-ouvre pas le port ici car il est déjà ouvert depuis MainView (Sinon ça provoque une incoherence au niveau du scanner)
        myScanner.SerialBuffer.Changed += OnSerialDataReception;
    }

    private void OnSerialDataReception(object sender, EventArgs arg)
    {
        DeviceOrientationService.QueueBuffer MyLocalBuffer = (DeviceOrientationService.QueueBuffer)sender;
        if (MyLocalBuffer.Count > 0)
        {
            SerialBufferContent += MyLocalBuffer.Dequeue().ToString();
        }
    }

    internal void RefreshPage()
    {
        // Parcourt la collection globale de montres pour actualiser les détails de l'objet sélectionné
        var watch = Globals.MyWatches.FirstOrDefault(item => Id == item.Id);
        if (watch != null)
        {
            Brand = watch.Brand;
            Model = watch.Model;
            Type = watch.Type;
            Material = watch.Material;
            Year = watch.Year;
            Price = watch.Price;
            IsLimitedEdition = watch.IsLimitedEdition;
            Picture = watch.Picture;
        }
        else
        {
            // L'objet n'existe plus, on pourrait gérer ce cas (par ex. message d'erreur)
            Shell.Current.DisplayAlert("Error", "The selected watch no longer exists in the collection.", "OK");
            Shell.Current.GoToAsync(".."); // Retour à la page précédente
        }
    }

    internal void ClosePage()
    {
        MyScanner.SerialBuffer.Changed -= OnSerialDataReception;
        MyScanner.ClosePort();
    }

    // Commande pour valider les modifications, sauvegarder et rediriger
    [RelayCommand]
    internal async Task ChangeObjectParameters()
    {
        if (IsBusy)
            return; // Protection supplémentaire contre les clics multiples

        try
        {
            IsBusy = true;

            // Validation des données avec messages d'erreur spécifiques
            if (string.IsNullOrWhiteSpace(Brand))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Brand is required.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Model))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Model is required.", "OK");
                return;
            }

            if (Year <= 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Year must be a positive number.", "OK");
                return;
            }

            if (Year > DateTime.Now.Year)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Year cannot be in the future.", "OK");
                return;
            }

            if (Price < 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Price cannot be negative.", "OK");
                return;
            }

            // Recherche de l'objet à mettre à jour dans la collection
            var itemToUpdate = Globals.MyWatches.FirstOrDefault(item => item.Id == Id);
            if (itemToUpdate == null)
            {
                await Shell.Current.DisplayAlert("Error", "The watch to be edited no longer exists in the collection.", "OK");
                await Shell.Current.GoToAsync(".."); // Retour à la page principale
                return;
            }

            // Sauvegarde des valeurs actuelles pour pouvoir les restaurer en cas d'erreur
            var backup = new Watch
            {
                Id = itemToUpdate.Id,
                Brand = itemToUpdate.Brand,
                Model = itemToUpdate.Model,
                Type = itemToUpdate.Type,
                Material = itemToUpdate.Material,
                Year = itemToUpdate.Year,
                Price = itemToUpdate.Price,
                IsLimitedEdition = itemToUpdate.IsLimitedEdition,
                Picture = itemToUpdate.Picture
            };

            try
            {
                // Mise à jour de l'objet avec les nouvelles valeurs
                itemToUpdate.Brand = Brand?.Trim() ?? string.Empty;
                itemToUpdate.Model = Model?.Trim() ?? string.Empty;
                itemToUpdate.Type = Type?.Trim() ?? string.Empty;
                itemToUpdate.Material = Material?.Trim() ?? string.Empty;
                itemToUpdate.Year = Year;
                itemToUpdate.Price = Price;
                itemToUpdate.IsLimitedEdition = IsLimitedEdition;
                itemToUpdate.Picture = Picture?.Trim() ?? string.Empty;

                // Sauvegarde la collection mise à jour dans le JSON
                await _jsonServices.SetWatches(Globals.MyWatches);

                // Redirige vers la page principale (MainView) après sauvegarde réussie
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                // Restauration des valeurs d'origine en cas d'erreur lors de la sauvegarde
                itemToUpdate.Brand = backup.Brand;
                itemToUpdate.Model = backup.Model;
                itemToUpdate.Type = backup.Type;
                itemToUpdate.Material = backup.Material;
                itemToUpdate.Year = backup.Year;
                itemToUpdate.Price = backup.Price;
                itemToUpdate.IsLimitedEdition = backup.IsLimitedEdition;
                itemToUpdate.Picture = backup.Picture;

                // Message d'erreur avec détails
                await Shell.Current.DisplayAlert("Save Error",
                    $"An error occurred while saving changes: {ex.Message}", "OK");
            }
        }
        catch (Exception ex)
        {
            // Gestion des erreurs générales non liées à la sauvegarde
            await Shell.Current.DisplayAlert("System Error",
                $"A system error occurred: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
