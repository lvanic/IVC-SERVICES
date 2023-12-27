using IVC_DDNS.models;
using System.Collections.Concurrent;


var nodes = new ConcurrentDictionary<string, Node>();

var app = WebApplication.Create(args);

app.MapGet("/ping", async (string id, HttpContext context) =>
{
    if (nodes.TryGetValue(id, out var node))
    {
        node.LastPing = DateTime.UtcNow;
        await context.Response.WriteAsync("Pong");
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Node not found");
    }
});

app.MapPost("/register", (Node node) =>
{
    node.LastPing = DateTime.UtcNow;
    nodes.AddOrUpdate(node.Id, node, (_, existingNode) => node);
    return Results.Ok();
});

app.MapPost("/update", (Node node) =>
{
    if (nodes.TryGetValue(node.Id, out _))
    {
        node.LastPing = DateTime.UtcNow;
        nodes[node.Id] = node;
        return Results.Ok();
    }
    else
    {
        return Results.NotFound();
    }
});

app.MapPost("/unregister", (string id) =>
{
    nodes.TryRemove(id, out _);
    return Results.Ok();
});

var cancellationTokenSource = new CancellationTokenSource();
_ = Task.Run(async () =>
{
    while (!cancellationTokenSource.Token.IsCancellationRequested)
    {
        await PingNodes();
        await Task.Delay(TimeSpan.FromMinutes(5), cancellationTokenSource.Token);
    }
}, cancellationTokenSource.Token);

async Task PingNodes()
{
    var cutoff = DateTime.UtcNow.AddMinutes(-5);
    foreach (var pair in nodes.Where(x => x.Value.LastPing < cutoff).ToList())
    {
        nodes.TryRemove(pair.Key, out _);
    }
}

app.Run();
