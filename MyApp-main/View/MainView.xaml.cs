namespace MyApp.View;

public partial class MainView : ContentPage
{
	MainViewModel viewModel;
	public MainView(MainViewModel viewModel)
	{
		this.viewModel = viewModel;
		InitializeComponent();
		BindingContext=viewModel;
	}
	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
        base.OnNavigatedTo(args);

        BindingContext = null;
		viewModel.RefreshPage();    
		BindingContext = viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Charge automatiquement la data via la commande LoadJSON
        await viewModel.LoadJSONCommand.ExecuteAsync(null);
        await viewModel.LoadUserRatings();

        viewModel.RefreshPage();
    }
}