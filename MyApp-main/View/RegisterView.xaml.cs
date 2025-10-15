namespace MyApp.View;

public partial class RegisterView : ContentPage
{
    public RegisterView(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}