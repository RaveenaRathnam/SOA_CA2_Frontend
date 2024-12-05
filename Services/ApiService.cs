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
        public string ApiKey { get; private set; } = string.Empty;

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
                ApiKey = deserielizedData?.apiKey ?? string.Empty;
                NotifyStateChanged();
                return true;
            }

            SuccessMessage = string.Empty;  
            NotifyStateChanged();
            return false;
        }

        public async Task<List<ProductModel>> GetProductsAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Product");
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine(responseData);
                    var deserielizedData = System.Text.Json.JsonSerializer.Deserialize<List<ProductModel>>(responseData);
                    System.Console.WriteLine(deserielizedData);
                    return deserielizedData;
                }
                else
                {
                    System.Console.WriteLine(response);
                }
            }
            catch (Exception error)
            {
                System.Console.WriteLine(error);
            }
            

            return new List<ProductModel>(); // Return empty list on failure
        }

        public async Task<string> GetCategoryNameByIdAsync(int categoryId)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Category/{categoryId}");
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                System.Console.WriteLine(responseData);

                // Deserialize into the CategoryModel
                var deserializedData = System.Text.Json.JsonSerializer.Deserialize<CategoryModel>(responseData);

                // Return the CategoryName property
                return deserializedData?.categoryName ?? "Unknown";
            }

            // Default if the category name can't be fetched
            return "Unknown";
        }

    }

}
