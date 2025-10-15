namespace MyApp.View;

public partial class UsersView : ContentPage
{
    private UsersViewModel _viewModel;

    public UsersView(UsersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Charger les utilisateurs lorsque la page apparaît
        await _viewModel.LoadUsersCommand.ExecuteAsync(null);
    }
}