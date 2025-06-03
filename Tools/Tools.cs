using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpSse.Tools;

[McpServerToolType]
public class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";
}

//[McpServerToolType]
//public static class ChuckNorrisJokeTool
//{
//    private static readonly HttpClient httpClient = new HttpClient();

//    [McpServerTool, Description("Get a random Chuck Norris joke")]
//    public static async Task<string> GetChuckJoke()
//    {
//        try
//        {
//            var response = await httpClient.GetStringAsync("https://api.chucknorris.io/jokes/random");
//            var jsonDoc = JsonDocument.Parse(response);
//            var joke = jsonDoc.RootElement.GetProperty("value").GetString();
//            return joke ?? "No joke available";
//        }
//        catch (Exception ex)
//        {
//            return $"Error fetching Chuck Norris joke: {ex.Message}";
//        }
//    }

//    [McpServerTool, Description("Get a random Chuck Norris joke by category")]
//    public static async Task<string> GetChuckJokeByCategory(string category)
//    {
//        try
//        {
//            var response = await httpClient.GetStringAsync($"https://api.chucknorris.io/jokes/random?category={category}");
//            var jsonDoc = JsonDocument.Parse(response);
//            var joke = jsonDoc.RootElement.GetProperty("value").GetString();
//            return joke ?? "No joke available";
//        }
//        catch (Exception ex)
//        {
//            return $"Error fetching Chuck Norris joke by category: {ex.Message}";
//        }
//    }

//    [McpServerTool, Description("Get all available categories for Chuck Norris jokes")]
//    public static async Task<string> GetChuckCategories()
//    {
//        try
//        {
//            var response = await httpClient.GetStringAsync("https://api.chucknorris.io/jokes/categories");
//            var categories = JsonSerializer.Deserialize<string[]>(response);
//            return string.Join(", ", categories ?? Array.Empty<string>());
//        }
//        catch (Exception ex)
//        {
//            return $"Error fetching Chuck Norris categories: {ex.Message}";
//        }
//    }
//}

//[McpServerToolType]
//public static class DadJokeTool
//{
//    private static readonly HttpClient httpClient = new HttpClient();

//    static DadJokeTool()
//    {
//        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
//    }

//    [McpServerTool, Description("Get a random dad joke")]
//    public static async Task<string> GetDadJoke()
//    {
//        try
//        {
//            var response = await httpClient.GetStringAsync("https://icanhazdadjoke.com/");
//            var jsonDoc = JsonDocument.Parse(response);
//            var joke = jsonDoc.RootElement.GetProperty("joke").GetString();
//            return joke ?? "No dad joke available";
//        }
//        catch (Exception ex)
//        {
//            return $"Error fetching dad joke: {ex.Message}";
//        }
//    }
//}