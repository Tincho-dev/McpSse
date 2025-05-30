using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

app.Run();


static void MapAbsoluteEndpointUriMcp(IEndpointRouteBuilder endpoints)
{
    var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var options = endpoints.ServiceProvider.GetRequiredService<IOptions<McpServerOptions>>().Value;
    var routeGroup = endpoints.MapGroup("");
    SseResponseStreamTransport? session = null;

    routeGroup.MapGet("/sse", async context =>
    {
        context.Response.Headers.ContentType = "text/event-stream";

        // Construct the absolute base URI dynamically.
        // var host = $"{context.Request.Scheme}://{context.Request.Host}";
        var host = $"https://sse-mcp-gfhrb0aydyc8e3gf.canadacentral-01.azurewebsites.net";
        var transport = new SseResponseStreamTransport(context.Response.Body, $"{host}/message");
        session = transport;
        try
        {
            await using (transport)
            {
                var transportTask = transport.RunAsync(context.RequestAborted);
                await using var server = McpServerFactory.Create(transport, options, loggerFactory, endpoints.ServiceProvider);

                try
                {
                    await server.RunAsync(context.RequestAborted);
                }
                catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                {
                    // Normal SSE disconnect.
                }
                catch (Exception ex)
                {
                    // Handle other exceptions as needed.
                }

                await transportTask;
            }
        }
        catch (Exception ex)
        {

        }
    });

    routeGroup.MapPost("/message", async context =>
    {
        if (session is null)
        {
            await Results.BadRequest("Session not started.").ExecuteAsync(context);
            return;
        }

        var message = await context.Request.ReadFromJsonAsync<JsonRpcMessage>(
            McpJsonUtilities.DefaultOptions, context.RequestAborted);
        if (message is null)
        {
            await Results.BadRequest("No message in request body.").ExecuteAsync(context);
            return;
        }

        await session.OnMessageReceivedAsync(message, context.RequestAborted);
        context.Response.StatusCode = StatusCodes.Status202Accepted;
        await context.Response.WriteAsync("Accepted");
    });
}


[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";
}


[McpServerToolType]
public static class ChuckNorrisJokeTool
{
    private static readonly HttpClient httpClient = new HttpClient();

    [McpServerTool, Description("Get a random Chuck Norris joke")]
    public static async Task<string> GetChuckJoke()
    {
        try
        {
            var response = await httpClient.GetStringAsync("https://api.chucknorris.io/jokes/random");
            var jsonDoc = JsonDocument.Parse(response);
            var joke = jsonDoc.RootElement.GetProperty("value").GetString();
            return joke ?? "No joke available";
        }
        catch (Exception ex)
        {
            return $"Error fetching Chuck Norris joke: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get a random Chuck Norris joke by category")]
    public static async Task<string> GetChuckJokeByCategory(string category)
    {
        try
        {
            var response = await httpClient.GetStringAsync($"https://api.chucknorris.io/jokes/random?category={category}");
            var jsonDoc = JsonDocument.Parse(response);
            var joke = jsonDoc.RootElement.GetProperty("value").GetString();
            return joke ?? "No joke available";
        }
        catch (Exception ex)
        {
            return $"Error fetching Chuck Norris joke by category: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get all available categories for Chuck Norris jokes")]
    public static async Task<string> GetChuckCategories()
    {
        try
        {
            var response = await httpClient.GetStringAsync("https://api.chucknorris.io/jokes/categories");
            var categories = JsonSerializer.Deserialize<string[]>(response);
            return string.Join(", ", categories ?? Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return $"Error fetching Chuck Norris categories: {ex.Message}";
        }
    }
}

[McpServerToolType]
public static class DadJokeTool
{
    private static readonly HttpClient httpClient = new HttpClient();

    static DadJokeTool()
    {
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    [McpServerTool, Description("Get a random dad joke")]
    public static async Task<string> GetDadJoke()
    {
        try
        {
            var response = await httpClient.GetStringAsync("https://icanhazdadjoke.com/");
            var jsonDoc = JsonDocument.Parse(response);
            var joke = jsonDoc.RootElement.GetProperty("joke").GetString();
            return joke ?? "No dad joke available";
        }
        catch (Exception ex)
        {
            return $"Error fetching dad joke: {ex.Message}";
        }
    }
}