using MyApp.Model;

namespace MyApp.Service;

public static class CurrentUserManager
{
    public static User CurrentUser { get; private set; }

    public static bool IsAuthenticated => CurrentUser != null;

    public static bool IsAdmin => CurrentUser?.IsAdmin ?? false;

    public static void SetCurrentUser(User user)
    {
        CurrentUser = user;
    }

    public static void Logout()
    {
        CurrentUser = null;
    }
}