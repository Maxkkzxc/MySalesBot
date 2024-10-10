using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyApp.Models;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000/api/") };
    }

    public async Task<List<Drink>> GetDrinksAsync()
    {
        var response = await _httpClient.GetAsync("drinks");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Drink>>();
    }

    public async Task AddDrinkAsync(Drink drink)
    {
        var response = await _httpClient.PostAsJsonAsync("drinks", drink);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteDrinkAsync(int Id)
    {
        var response = await _httpClient.DeleteAsync($"drinks/{Id}");
        response.EnsureSuccessStatusCode();
    }
}