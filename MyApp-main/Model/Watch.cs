using System.ComponentModel;

namespace MyApp.Model
{
    public class Watch : INotifyPropertyChanged
    {
        // Infos de base
        public string Id { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public bool IsLimitedEdition { get; set; }
        public string Picture { get; set; } = string.Empty;

        // Initialisation des etoiles
        string star1 = "star_empty.png", star2 = "star_empty.png",
               star3 = "star_empty.png", star4 = "star_empty.png", star5 = "star_empty.png";

        // ⭐️ Propriétés avec notification pour MAUI (interface se met à jour)
        public string Star1 { get => star1; set { if (star1 != value) { star1 = value; OnPropertyChanged(nameof(Star1)); } } }
        public string Star2 { get => star2; set { if (star2 != value) { star2 = value; OnPropertyChanged(nameof(Star2)); } } }
        public string Star3 { get => star3; set { if (star3 != value) { star3 = value; OnPropertyChanged(nameof(Star3)); } } }
        public string Star4 { get => star4; set { if (star4 != value) { star4 = value; OnPropertyChanged(nameof(Star4)); } } }
        public string Star5 { get => star5; set { if (star5 != value) { star5 = value; OnPropertyChanged(nameof(Star5)); } } }

        // 🔄 Notifie le système de mise à jour
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
