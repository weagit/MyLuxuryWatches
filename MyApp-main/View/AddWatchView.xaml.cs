namespace MyApp.View;

public partial class AddWatchView : ContentPage
{
    AddWatchViewModel viewModel;
    public AddWatchView(AddWatchViewModel viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
}