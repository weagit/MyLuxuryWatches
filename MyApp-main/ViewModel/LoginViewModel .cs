using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Model;
using MyApp.Service;
using System.Text.RegularExpressions;

namespace MyApp.ViewModel;

public partial class LoginViewModel : BaseViewModel
{
    private readonly MongoUserService _userService;

    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    public LoginViewModel(MongoUserService userService)
    {
        _userService = userService;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (IsBusy)
            return;

        // Validation des champs
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter a username and a password", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            var user = await _userService.AuthenticateUser(Username, Password);

            if (user != null)
            {
                // Définir l'utilisateur actuel
                CurrentUserManager.SetCurrentUser(user);

                // Rediriger vers la page principale
                await Shell.Current.GoToAsync("//MainView");

                // Message de bienvenue
                await Shell.Current.DisplayAlert("Welcome", $"Login successful. Welcome {user.FirstName} {user.LastName}!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Authentication Error", "Incorrect username or password", "OK");
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

    [RelayCommand]
    private async Task Register()
    {
        // Rediriger vers la page d'inscription
        await Shell.Current.GoToAsync("RegisterView");
    }
}
