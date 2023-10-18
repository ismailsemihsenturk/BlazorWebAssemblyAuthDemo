using Blazored.LocalStorage;
using BlazorWebAssemblyAuthDemo.Client.Helper;
using BlazorWebAssemblyAuthDemo.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BlazorWebAssemblyAuthDemo.Client.Service
{
    public class AuthService : IAuthService
    {

        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;


        public AuthService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }


        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            var loginAsJson = JsonSerializer.Serialize(loginModel);
            var response = await _httpClient.PostAsync("api/login", new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
            var loginResult = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync(),new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

            if(!response.IsSuccessStatusCode){
                return loginResult!;
            }

            await _localStorage.SetItemAsync("authToken", loginResult!.Token);
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAuthenticated(loginModel.Email!);
            return loginResult;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<RegisterResult> Register(RegisterModel registerModel)
        {
            var result = await _httpClient.PostAsJsonAsync("api/accounts", registerModel);
            if (!result.IsSuccessStatusCode)
            {
                return new RegisterResult
                { 
                    Successful=false, 
                    Errors=new List<string> { "Error Occured!" } 
                };

            }
            return new RegisterResult
            {
                Successful = true,
                Errors = new List<string> { "Registered Successfully!" }
            };
        }
    }
}
