namespace SOA_CA2_Frontend.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using SOA_CA2_Frontend.Models;

    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public bool IsLoggedIn { get; private set; } = false;
        public string SuccessMessage { get; set; } = string.Empty;
        public string UserToken { get; private set; } = string.Empty;

        public event Action? OnChange;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiBaseUrl"];
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var payload = new { email, password };
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/auth/login", payload);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var deserializedData = System.Text.Json.JsonSerializer.Deserialize<LoginModel>(responseData);
                System.Console.WriteLine(deserializedData.jwtToken);
                System.Console.WriteLine(deserializedData.refreshToken);
                UserToken = deserializedData?.jwtToken ?? string.Empty; ;
                IsLoggedIn = true;
                System.Console.WriteLine(IsLoggedIn);
                SuccessMessage = "Login successful! Welcome back!";
                NotifyStateChanged();
                return true;
            }
            else {
                IsLoggedIn = false;
                SuccessMessage = string.Empty;
                NotifyStateChanged();
                return false;
            }
           
        }

        public void Logout()
        {
            IsLoggedIn = false;
            System.Console.WriteLine(IsLoggedIn);
            UserToken = string.Empty;
            SuccessMessage = string.Empty;
            NotifyStateChanged();
           
        }

        public void NotifyStateChanged() => OnChange?.Invoke();

        public async Task<bool> RegisterAsync(string firstName, string lastName, string email, string password, string address)
        {
            var payload = new
            {
                First_Name = firstName,
                Last_Name = lastName,
                Email = email,
                Password = password,
                Role = 0, // Default role
                Address = address
            };

            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/auth/register", payload);
            if (response.IsSuccessStatusCode)
            { 
                var responseData = await response.Content.ReadAsStringAsync();
                var deserielizedData = System.Text.Json.JsonSerializer.Deserialize<RegisterModel>(responseData);
                System.Console.WriteLine(deserielizedData.apiKey);
                SuccessMessage = "Registration successful! You can now log in.";
                NotifyStateChanged();
                return true;
            }

            SuccessMessage = string.Empty;  
            NotifyStateChanged();
            return false;
        }
    }

}
