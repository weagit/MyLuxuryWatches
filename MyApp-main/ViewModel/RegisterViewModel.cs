using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Model;
using MyApp.Service;
using System.Text.RegularExpressions;

namespace MyApp.ViewModel;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly MongoUserService _userService;

    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string firstName;

    [ObservableProperty]
    private string lastName;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string confirmPassword;

    public RegisterViewModel(MongoUserService userService)
    {
        _userService = userService;
    }

    [RelayCommand]
    private async Task Register()
    {
        if (IsBusy)
            return;

        // Field validation
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            await Shell.Current.DisplayAlert("Validation", "All required fields must be filled", "OK");
            return;
        }

        // Username validation
        if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9_]{3,20}$"))
        {
            await Shell.Current.DisplayAlert("Validation", "Username must be between 3 and 20 alphanumeric characters or underscores", "OK");
            return;
        }

        // Email validation
        if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            await Shell.Current.DisplayAlert("Validation", "Email address is not valid", "OK");
            return;
        }

        // Password validation
        if (Password.Length < 6)
        {
            await Shell.Current.DisplayAlert("Validation", "Password must be at least 6 characters long", "OK");
            return;
        }

        // Check password confirmation
        if (Password != ConfirmPassword)
        {
            await Shell.Current.DisplayAlert("Validation", "Passwords do not match", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            // Check if user already exists
            var existingUser = await _userService.GetUserByUsername(Username);
            if (existingUser != null)
            {
                await Shell.Current.DisplayAlert("Error", "This username is already taken", "OK");
                return;
            }

            // Create new user
            var newUser = new User
            {
                Username = Username,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                PasswordHash = Password, // Will be hashed in the service
                IsAdmin = false // By default, a new user is not an admin
            };

            await _userService.AddUser(newUser);

            await Shell.Current.DisplayAlert("Success", "Your account has been successfully created! You can now log in.", "OK");

            // Redirect to login page
            await Shell.Current.GoToAsync("//LoginView");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"An error occurred during registration: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackToLogin()
    {
        IsBusy = true;
        await Shell.Current.GoToAsync("LoginView");
        IsBusy = false;
    }
}
