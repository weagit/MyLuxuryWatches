namespace MyApp.View;

public partial class StatsView : ContentPage
{
    public StatsView(StatsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}