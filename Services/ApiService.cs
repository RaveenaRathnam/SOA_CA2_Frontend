namespace SOA_CA2_Frontend.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.JSInterop;
    using SOA_CA2_Frontend.Models;


    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly IJSRuntime _jsRuntime;

        public bool IsLoggedIn { get; private set; } = false;
        public string SuccessMessage { get; set; } = string.Empty;
        public string UserToken { get; private set; } = string.Empty;
        public string UserApiKey { get; private set; } = string.Empty;
        public int CurrentUserRole { get; private set; } = 0;
        public int userId { get; private set; } = 0;
        public bool IsAdmin => CurrentUserRole == 1;
        public bool IsUser => CurrentUserRole == 0;

        public event Action? OnChange;

        public ApiService(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiBaseUrl"];
            _jsRuntime = jsRuntime;
        }

        [JSInvokable]
        public static async Task SetSessionDetails(string token, string apiKey, int role, IJSRuntime jsRuntime)
        {
            var apiService = new ApiService(
                new HttpClient(),
                new ConfigurationBuilder().Build(),  
                jsRuntime);

            apiService.UserToken = token;
            apiService.UserApiKey = apiKey;
            apiService.CurrentUserRole = role;
            apiService.IsLoggedIn = !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(apiKey);

            await apiService.StoreSessionDataAsync(); 
            apiService.NotifyStateChanged();
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
                System.Console.WriteLine(deserializedData.apiKey);
                System.Console.WriteLine(deserializedData.userId);
                UserToken = deserializedData?.jwtToken ?? string.Empty; 
                UserApiKey= deserializedData?.apiKey ?? string.Empty;
                userId = deserializedData.userId;
                CurrentUserRole = deserializedData.role; // 0: User, 1: Admin
                IsLoggedIn = true;

                await StoreSessionDataAsync();
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

        public async Task LogoutAsync()
        {
            IsLoggedIn = false;
            System.Console.WriteLine(IsLoggedIn);
            UserToken = string.Empty;
            SuccessMessage = string.Empty;
            CurrentUserRole = 0;
            userId = 0;
            await ClearSessionDataAsync();
            NotifyStateChanged();
           
        }

        public async Task InitializeSessionAsync()
        {
            UserToken = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "userToken") ?? string.Empty;
            UserApiKey = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "userApiKey") ?? string.Empty;
            CurrentUserRole = int.TryParse(await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "role"), out var role) ? role : 0;
            userId = int.TryParse(await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "userId"), out var id) ? id : 0;

            IsLoggedIn = !string.IsNullOrEmpty(UserToken) && !string.IsNullOrEmpty(UserApiKey);
            NotifyStateChanged();
        }

        private async Task StoreSessionDataAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userToken", UserToken);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userApiKey", UserApiKey);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "role", CurrentUserRole);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userId", userId);
        }

        private async Task ClearSessionDataAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "userToken");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "userApiKey");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "role");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "userId");
        }

        private void SetAuthHeaders()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {UserToken}");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Apikey", UserApiKey);
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
            

            return new List<ProductModel>(); 
        }

        public async Task<string> GetCategoryNameByIdAsync(int categoryId)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Category/{categoryId}");
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                System.Console.WriteLine(responseData);

                
                var deserializedData = System.Text.Json.JsonSerializer.Deserialize<CategoryModel>(responseData);

                
                return deserializedData?.categoryName ?? "Unknown";
            }

            
            return "Unknown";
        }

        public async Task<List<CategoryModel>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Category");
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                System.Console.WriteLine(responseData);
                var deserielizedData = System.Text.Json.JsonSerializer.Deserialize<List<CategoryModel>>(responseData);
                System.Console.WriteLine(deserielizedData);
                return deserielizedData;
            }
            return new List<CategoryModel>(); 
        }

        public async Task<bool> AddToCartAsync(CartItemModel cartItem)
        {
            try
            {
                SetAuthHeaders();

                
                if (userId==0)
                {
                    Console.WriteLine("Error: User ID is null or empty.");
                    return false;
                }

                
                string requestUrl = $"{_apiBaseUrl}/Cart/{userId}";

                
                Console.WriteLine($"Sending POST request to: {requestUrl}");
                Console.WriteLine($"With API Key: {UserApiKey}");

                
                var response = await _httpClient.PostAsJsonAsync(requestUrl, cartItem);

                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Item added to cart successfully.");
                    return true;
                }
                else
                {
                    
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Failed to add item to cart.");
                    Console.WriteLine($"StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                    Console.WriteLine($"Response Content: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
        public async Task<ProductModel> GetProductByIdAsync(int productId)
        {
            try
            {
                
                string requestUrl = $"{_apiBaseUrl}/Product/{productId}";

                
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductModel>();
                }
                else
                {
                    Console.WriteLine($"Failed to fetch product details for product ID: {productId}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching product details: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CartItemModel>> GetCartItemsAsync()
        {

            SetAuthHeaders();

            
            string requestUrl = $"{_apiBaseUrl}/Cart/{userId}";

                
                var response = await _httpClient.GetAsync(requestUrl);
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var cart = await response.Content.ReadFromJsonAsync<CartModel>();
                    return cart?.items ?? new List<CartItemModel>();
                }
                else
                {
                    Console.WriteLine($"Failed to fetch cart items. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                    return new List<CartItemModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching cart items: {ex.Message}");
                return new List<CartItemModel>();
            }


            return new List<CartItemModel>(); 

        }
        public async Task<bool> RemoveFromCartAsync(int productId)
        {
            try
            {
                SetAuthHeaders();
                
                string requestUrl = $"{_apiBaseUrl}/Cart/{userId}/product/{productId}";

                
                var response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Item removed from cart successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to remove item from cart. StatusCode: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while removing item from cart: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> ClearCartAsync()
        {
            try
            {
                SetAuthHeaders();
                
                string requestUrl = $"{_apiBaseUrl}/Cart/{userId}";

               
                var response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Cart cleared successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to clear cart. StatusCode: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while clearing the cart: {ex.Message}");
                return false;
            }
        }
        public async Task<UserModel> GetUserByIdAsync()
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/User/{userId}";
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserModel>();
                }
                else
                {
                    Console.WriteLine($"Failed to fetch user details. StatusCode: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching user details: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> UpdateUserAsync(UserModel user)
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/User/{userId}";
                var response = await _httpClient.PutAsJsonAsync(requestUrl, user);

                if (response.IsSuccessStatusCode)
                {
                     
                    Console.WriteLine("User details updated successfully.");
                    return true;
                }
                else
                {
                    
                    Console.WriteLine($"Failed to update user details. StatusCode: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating user details: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteUserAsync()
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/User/{userId}";
                var response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                     
                    Console.WriteLine("User account deleted successfully.");
                    return true;
                }
                else
                {
                    
                    Console.WriteLine($"Failed to delete user account. StatusCode: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the user account: {ex.Message}");
                return false;
            }
        }

        public async Task<List<OrderModel>> GetOrdersByUserIdAsync()
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/Order/user/{userId}";
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<OrderModel>>();
                }
                else
                {
                    Console.WriteLine($"Failed to fetch orders. StatusCode: {response.StatusCode}");
                    return new List<OrderModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching orders: {ex.Message}");
                return new List<OrderModel>();
            }
        }
        public async Task<bool> CreateOrderAsync(OrderModel order)
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/Order";
                var response = await _httpClient.PostAsJsonAsync(requestUrl, order);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Order created successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to create order. StatusCode: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating the order: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> AddProductAsync(ProductModel product)
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/Product";

                // Make the POST request
                var response = await _httpClient.PostAsJsonAsync(requestUrl, product);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Product added successfully.");
                    return true;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to add product. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                    Console.WriteLine($"Response Content: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding the product: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(ProductModel product)
        {
            try
            {
                SetAuthHeaders();

                
                string requestUrl = $"{_apiBaseUrl}/Product/{product.product_Id}";

                
                var response = await _httpClient.PutAsJsonAsync(requestUrl, product);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Product updated successfully.");
                    return true;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to update product. StatusCode: {response.StatusCode}, Content: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the product: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/Product/{productId}";
                
                var response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Product deleted successfully.");
                    return true;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to delete product. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                    Console.WriteLine($"Response Content: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the product: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteUserByIdAsync(int userId)
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/User/{userId}";
                var response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("User deleted successfully.");
                    return true;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to delete user. StatusCode: {response.StatusCode}, Content: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the user: {ex.Message}");
                return false;
            }
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            try
            {
                SetAuthHeaders();
                string requestUrl = $"{_apiBaseUrl}/User";
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<UserModel>>();
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to fetch users. StatusCode: {response.StatusCode}, Content: {responseContent}");
                    return new List<UserModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching users: {ex.Message}");
                return new List<UserModel>();
            }
        }

    }

}
