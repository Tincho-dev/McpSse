using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

// Log to console (stderr) for debugging
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register HttpContextAccessor and MCP server
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen()
        .AddMcpServer()
        .WithToolsFromAssembly(); ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Text("MCP SSE Server está en línea"))
.WithOpenApi();

// SSE endpoint for MCP
app.MapGet("/sse", async context =>
{
    context.Response.Headers.ContentType = "text/event-stream";

    // Create SSE transport using response stream
    await using var transport = new SseResponseStreamTransport(
        context.Response.Body,
        "McpSse"
    );

    // Prepare server
    var options = context.RequestServices.GetRequiredService<IOptions<McpServerOptions>>().Value;
    var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
    await using var server = McpServerFactory.Create(
        transport,
        options,
        loggerFactory,
        context.RequestServices
    );

    // Run transport and server
    var transportTask = transport.RunAsync(context.RequestAborted);
    await server.RunAsync(context.RequestAborted);
    await transportTask;
})
.WithOpenApi();

// POST endpoint to receive client messages
app.MapPost("/message", async context =>
{
    var transport = context.RequestServices.GetRequiredService<SseResponseStreamTransport>();
    var message = await context.Request.ReadFromJsonAsync<JsonRpcMessage>(
        cancellationToken: context.RequestAborted
    );
    if (message != null)
    {
        await transport.OnMessageReceivedAsync(message, context.RequestAborted);
        context.Response.StatusCode = StatusCodes.Status202Accepted;
    }
})
.WithOpenApi();

app.Run();


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