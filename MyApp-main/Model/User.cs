using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApp.Model;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    // Pour distinguer les administrateurs des utilisateurs normaux
    public bool IsAdmin { get; set; }

    // Relation avec les montres de l'utilisateur (IDs des montres)
    public List<string> WatchIds { get; set; } = new List<string>();

    public Dictionary<string, int> WatchRatings { get; set; } = new Dictionary<string, int>();

}