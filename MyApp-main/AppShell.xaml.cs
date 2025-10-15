namespace MyApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DetailsView), typeof(DetailsView));
            Routing.RegisterRoute(nameof(AddWatchView), typeof(AddWatchView));
            Routing.RegisterRoute(nameof(StatsView), typeof(StatsView));
            Routing.RegisterRoute(nameof(UsersView), typeof(UsersView));
            Routing.RegisterRoute(nameof(LoginView), typeof(LoginView));
            Routing.RegisterRoute(nameof(RegisterView), typeof(RegisterView));
        }
    }
}
