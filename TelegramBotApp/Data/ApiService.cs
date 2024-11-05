using Microsoft.Extensions.Configuration;
using MyApp.Models;
using System.Net.Http.Json;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(IConfiguration configuration)
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"];
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
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

    public async Task DeleteDrinkAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"drinks/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        var response = await _httpClient.GetAsync("orders");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Order>>();
    }

    public async Task ConfirmOrderAsync(int id)
    {
        var response = await _httpClient.PostAsync($"orders/{id}/confirm", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelOrderAsync(int id)
    {
        var response = await _httpClient.PostAsync($"orders/{id}/cancel", null);
        response.EnsureSuccessStatusCode();
    }
}