using MongoDB.Driver;
using MyApp.Model;
using BCrypt.Net;

namespace MyApp.Service;

public class MongoUserService
{
    private readonly IMongoCollection<User> _users;

    public MongoUserService()
    {
        var clientSettings = MongoClientSettings.FromConnectionString("mongodb://student:IAmTh3B3st@185.157.245.38:5003");
        var client = new MongoClient(clientSettings);
        var database = client.GetDatabase("LuxuryWatchesDB");
        _users = database.GetCollection<User>("users");
    }

    // Obtenir tous les utilisateurs
    public async Task<List<User>> GetAllUsers()
    {
        return await _users.Find(_ => true).ToListAsync();
    }

    // Obtenir un utilisateur par son ID
    public async Task<User> GetUserById(string id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    // Obtenir un utilisateur par son nom d'utilisateur
    public async Task<User> GetUserByUsername(string username)
    {
        return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    // Ajouter un nouvel utilisateur
    public async Task<User> AddUser(User user)
    {
        // Vérifier si l'utilisateur existe déjà
        var existingUser = await GetUserByUsername(user.Username);
        if (existingUser != null)
        {
            throw new Exception("A user with this username already exists");
        }

        // Hasher le mot de passe
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

        // Insérer l'utilisateur dans la base de données
        await _users.InsertOneAsync(user);
        return user;
    }

    // Mettre à jour un utilisateur
    public async Task UpdateUser(User userToUpdate)
    {
        await _users.ReplaceOneAsync(u => u.Id == userToUpdate.Id, userToUpdate);
    }

    // Supprimer un utilisateur
    public async Task DeleteUser(string id)
    {
        await _users.DeleteOneAsync(u => u.Id == id);
    }

    // Authentifier un utilisateur
    public async Task<User> AuthenticateUser(string username, string password)
    {
        var user = await GetUserByUsername(username);
        if (user == null)
        {
            return null;
        }

        // Vérifier le mot de passe
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return isPasswordValid ? user : null;
    }

    // Cette méthode va créer ou mettre à jour la note que l’utilisateur donne à une montre
    public async Task SetRatingForWatchAsync(string userId, string watchId, int rating)
    {
        var user = await GetUserById(userId);
        if (user == null)
            throw new Exception("User not found");

        if (user.WatchRatings == null)
            user.WatchRatings = new Dictionary<string, int>();

        user.WatchRatings[watchId] = rating;

        await UpdateUser(user);
    }
}
