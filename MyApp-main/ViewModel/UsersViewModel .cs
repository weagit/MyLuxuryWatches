using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Model;
using MyApp.Service;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace MyApp.ViewModel;

public partial class UsersViewModel : BaseViewModel
{
    private readonly MongoUserService _userService;

    [ObservableProperty]
    private ObservableCollection<User> users = new();

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
    private bool isAdmin;

    [ObservableProperty]
    private User selectedUser;

    public UsersViewModel(MongoUserService userService)
    {
        _userService = userService;
    }

    [RelayCommand]
    private async Task LoadUsers()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            // Check access rights
            if (!CurrentUserManager.IsAuthenticated || !CurrentUserManager.IsAdmin)
            {
                await Shell.Current.DisplayAlert("Access Denied", "You must be an administrator to access this page.", "OK");
                await Shell.Current.GoToAsync("//MainView");
                return;
            }

            var userList = await _userService.GetAllUsers();
            Users.Clear();

            foreach (var user in userList)
            {
                Users.Add(user);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Unable to load users: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddUser()
    {
        if (IsBusy)
            return;

        // Required field validation
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(Email))
        {
            await Shell.Current.DisplayAlert("Validation", "Username, password, and email are required", "OK");
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

        try
        {
            IsBusy = true;

            // Check if the user already exists
            var existingUser = await _userService.GetUserByUsername(Username);
            if (existingUser != null)
            {
                await Shell.Current.DisplayAlert("Error", "A user with this username already exists", "OK");
                return;
            }

            var newUser = new User
            {
                Username = Username,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                PasswordHash = Password, // Will be hashed in the service
                IsAdmin = IsAdmin
            };

            await _userService.AddUser(newUser);
            await LoadUsers();

            // Reset fields
            Username = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            IsAdmin = false;

            await Shell.Current.DisplayAlert("Success", "User successfully added", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Unable to add user: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteUser(User user)
    {
        if (IsBusy)
            return;

        // Prevent deleting the current admin user
        if (user.Id == CurrentUserManager.CurrentUser?.Id)
        {
            await Shell.Current.DisplayAlert("Action Not Allowed", "You cannot delete your own account", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            bool answer = await Shell.Current.DisplayAlert("Confirmation",
                $"Are you sure you want to delete user {user.Username}?", "Yes", "No");

            if (answer)
            {
                await _userService.DeleteUser(user.Id);
                await LoadUsers();
                await Shell.Current.DisplayAlert("Success", "User successfully deleted", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Unable to delete user: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
