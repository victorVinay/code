using System.Net.Http.Headers;
using System.Text.Json;

namespace LeaveService.Client;

public class EmployeeClient
{
    private readonly HttpClient _httpClient;

    public EmployeeClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Guid?> GetManagerId(Guid employeeId, string token)
{
    var request = new HttpRequestMessage(
        HttpMethod.Get,
        $"api/employee/employee/{employeeId}/manager");

    request.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    var response = await _httpClient.SendAsync(request);

    // 🔥 DEBUG FIRST
    var raw = await response.Content.ReadAsStringAsync();

    Console.WriteLine($"Status: {response.StatusCode}");
    Console.WriteLine($"Response: {raw}");

    if (!response.IsSuccessStatusCode)
        return null;

    // 🔥 VERY IMPORTANT FIX
    if (string.IsNullOrWhiteSpace(raw))
        return null;

    var result = JsonSerializer.Deserialize<ManagerResponse>(raw, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    return result?.ManagerId;
}
}

public class ManagerResponse
{
    public Guid? ManagerId { get; set; }
}