using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModel;

public partial class AddWatchViewModel : BaseViewModel
{
    private readonly JSONServices _myJSONService;

    [ObservableProperty] private string id;
    [ObservableProperty] private string brand;
    [ObservableProperty] private string model;
    [ObservableProperty] private string type;
    [ObservableProperty] private string material;
    [ObservableProperty] private int year;
    [ObservableProperty] private decimal price;
    [ObservableProperty] private bool isLimitedEdition;
    [ObservableProperty] private string picture;

    // On injecte JSONServices par le constructeur
    public AddWatchViewModel(JSONServices myJSONService)
    {
        _myJSONService = myJSONService;
    }

    [RelayCommand]
    private async Task AddWatch()
    {
        try
        {
            IsBusy = true;

            // Vérification des champs obligatoires (code déjà correcte)
            if (string.IsNullOrWhiteSpace(Id))
            {
                await Shell.Current.DisplayAlert("Error", "Watch ID is required.", "OK");
                return;
            }

            // ...reste des validations déjà présentes...

            // Si toutes les validations sont passées, on crée la montre
            var newWatch = new Watch
            {
                Id = Id.Trim(),
                // ...reste du code...
            };

            // Ajout à la collection globale
            Globals.MyWatches.Add(newWatch);

            // On sauvegarde dans le JSON
            await _myJSONService.SetWatches(Globals.MyWatches);

            // Réinitialisation des champs après ajout réussi
            ClearFields();

            // Retour à la page précédente
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            // Message d'erreur
            await Shell.Current.DisplayAlert("Error", $"Unable to add the watch: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Méthode pour réinitialiser les champs du formulaire
    private void ClearFields()
    {
        Id = string.Empty;
        Brand = string.Empty;
        Model = string.Empty;
        Type = string.Empty;
        Material = string.Empty;
        Year = DateTime.Now.Year;
        Price = 0;
        IsLimitedEdition = false;
        Picture = string.Empty;
    }
}
