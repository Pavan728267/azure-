using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

WebHost.CreateDefaultBuilder().
ConfigureServices(services =>
{
    var redisPassword = Environment.GetEnvironmentVariable("RedisPassword");

    services.AddSignalR(hubOptions =>
    {
        hubOptions.EnableDetailedErrors = true;
    })
    // .AddStackExchangeRedis($"redis-master.redis.svc.cluster.local,password={redisPassword}");
    .AddAzureSignalR();
})
.Configure(app =>
{
    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHub<Chat>("/default", options =>
            options.Transports = HttpTransportType.WebSockets);

        endpoints.MapGet("/", c => c.Response.WriteAsync("Hello from SignalR!"));
    });
})
.Build().Run();

public class Chat : Hub
{
    public override Task OnConnectedAsync()
    {
        return Clients.All.SendAsync("Send", $"joined the chat");
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        return Clients.All.SendAsync("Send", $"left the chat");
    }

    public async Task Echo(string name, string message) =>
        await Clients.Caller.SendAsync("Send", $"{name}: {message}");
}