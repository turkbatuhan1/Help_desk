namespace DXApplication6
{
    public sealed class AuthService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(10);
        private readonly DatabaseManager _database;
        private readonly Dictionary<string, LoginAttempt> _attempts = new(StringComparer.OrdinalIgnoreCase);

        public AuthService(DatabaseManager? database = null)
        {
            _database = database ?? DatabaseManager.Instance;
        }

        public LoginResult? Login(string username, string password, out string? errorMessage)
        {
            errorMessage = null;
            string normalizedUsername = username.Trim();

            if (IsLocked(normalizedUsername, out TimeSpan remaining))
            {
                errorMessage = $"Çok fazla başarısız giriş denemesi. Lütfen {Math.Ceiling(remaining.TotalMinutes)} dakika sonra tekrar deneyin.";
                return null;
            }

            LoginResult? result = _database.Login(normalizedUsername, password);
            if (result == null)
            {
                RegisterFailure(normalizedUsername);
                errorMessage = "Geçersiz kullanıcı adı veya şifre.";
                return null;
            }

            _attempts.Remove(normalizedUsername);
            return result;
        }

        private bool IsLocked(string username, out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;
            if (!_attempts.TryGetValue(username, out LoginAttempt? attempt) || attempt.FailedCount < MaxFailedAttempts)
            {
                return false;
            }

            DateTime unlockAt = attempt.LastFailureUtc.Add(LockoutDuration);
            DateTime now = DateTime.UtcNow;
            if (unlockAt <= now)
            {
                _attempts.Remove(username);
                return false;
            }

            remaining = unlockAt - now;
            return true;
        }

        private void RegisterFailure(string username)
        {
            if (!_attempts.TryGetValue(username, out LoginAttempt? attempt))
            {
                attempt = new LoginAttempt();
                _attempts[username] = attempt;
            }

            attempt.FailedCount++;
            attempt.LastFailureUtc = DateTime.UtcNow;
        }

        private sealed class LoginAttempt
        {
            public int FailedCount { get; set; }
            public DateTime LastFailureUtc { get; set; }
        }
    }
}
