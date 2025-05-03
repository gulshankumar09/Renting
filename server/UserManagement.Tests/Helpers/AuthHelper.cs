using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UserManagement.DTOs;

namespace UserManagement.Tests.Helpers;

public static class AuthHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> GetJwtTokenAsync(HttpClient client, string email, string password)
    {
        var loginRequest = new LoginRequestDTO
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to log in: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDTO>(responseContent, JsonOptions);

        return authResponse?.Token ?? throw new Exception("No token received");
    }

    public static void AuthenticateClient(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static StringContent CreateJsonContent<T>(T data)
    {
        var json = JsonSerializer.Serialize(data);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}