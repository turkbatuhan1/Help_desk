using Microsoft.Data.SqlClient;
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
        // Bağlantı adresi sabit tutulur
        private readonly string _connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=HelpDeskDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        // SQL bağlantı nesnesini döner
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Giriş Kontrolü
        public LoginResult? Login(string username, string password)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT KullaniciID, Rol FROM Kullanicilar WHERE KullaniciAdi=@p1 AND Sifre=@p2";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@p1", username);
                    command.Parameters.AddWithValue("@p2", password);

                    using SqlDataReader reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new LoginResult
                    {
                        UserId = reader.GetInt32(0),
                        Role = reader.GetString(1)
                    };
                }
            }
        }

        // Yeni Talep Oluşturma
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
                // Hata durumunda konsola yazdırabiliriz (Geliştirme aşaması için)
                Console.WriteLine("Database Error: " + ex.Message);
                return false;
            }
        }
    

    // Tüm talepleri bir liste (DataTable) olarak döner.
public DataTable GetAllTickets()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    // Talepleri ve gönderen kişilerin isimlerini birleştirerek getiriyoruz (JOIN)
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
