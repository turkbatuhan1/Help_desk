using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace DXApplication6
{
    public sealed class LoginResult
    {
        public int UserId { get; init; }
        public string Role { get; init; } = string.Empty;
    }

    public class DatabaseManager
    {
        public static DatabaseManager Instance { get; } = new DatabaseManager();

        private readonly string _connectionString;

        private DatabaseManager()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["HelpDeskDb"]?.ConnectionString
                ?? throw new InvalidOperationException("HelpDeskDb connection string App.config içinde bulunamadı.");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public LoginResult? Login(string username, string password)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT KullaniciID, Rol, Sifre FROM Kullanicilar WHERE KullaniciAdi = @username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using SqlDataReader reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        return null;
                    }

                    int userId = reader.GetInt32(0);
                    string role = reader.GetString(1);
                    string storedPassword = reader.GetString(2);
                    bool isValidPassword = PasswordHasher.IsHash(storedPassword)
                        ? PasswordHasher.Verify(password, storedPassword)
                        : string.Equals(password, storedPassword, StringComparison.Ordinal);

                    if (!isValidPassword)
                    {
                        return null;
                    }

                    if (!PasswordHasher.IsHash(storedPassword))
                    {
                        reader.Close();
                        UpdateUserPasswordHash(connection, userId, PasswordHasher.Hash(password));
                    }

                    return new LoginResult
                    {
                        UserId = userId,
                        Role = role
                    };
                }
            }
        }

        private static void UpdateUserPasswordHash(SqlConnection connection, int userId, string passwordHash)
        {
            string query = "UPDATE Kullanicilar SET Sifre = @passwordHash WHERE KullaniciID = @userId";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@userId", userId);
            command.ExecuteNonQuery();
        }

        public bool CreateTicket(int userId, string title, string description)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    string query = "INSERT INTO Talepler (GonderenID, Baslik, MesajText) VALUES (@userId, @title, @description)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@title", title);
                        command.Parameters.AddWithValue("@description", description);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database Error: " + ex.Message);
                return false;
            }
        }

        public DataTable GetAllTickets()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT T.TalepID, K.KullaniciAdi, T.Baslik, T.Durum, T.OlusturmaTarihi
                                     FROM Talepler T
                                     JOIN Kullanicilar K ON T.GonderenID = K.KullaniciID
                                     ORDER BY T.OlusturmaTarihi DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching tickets: " + ex.Message);
            }
            return dt;
        }

        public DataTable GetTicketsByUser(int userId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT TalepID, Baslik, MesajText, Durum, OlusturmaTarihi
                                     FROM Talepler
                                     WHERE GonderenID = @userId
                                     ORDER BY OlusturmaTarihi DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@userId", userId);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching user tickets: " + ex.Message);
            }
            return dt;
        }

        public bool UpdateTicketStatus(int ticketId, string newStatus)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    string query = "UPDATE Talepler SET Durum = @status WHERE TalepID = @id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@status", newStatus);
                        command.Parameters.AddWithValue("@id", ticketId);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
