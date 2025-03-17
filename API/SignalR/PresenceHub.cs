using System;
using API.Externsions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker tracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.GetUserName();
        if(string.IsNullOrEmpty(username)) throw new HubException("Cannot get current user claim");
        
        await tracker.UserConnected(username, Context.ConnectionId);

        await Clients.Others.SendAsync("UserIsOnline", username);

        var currentUsers = await tracker.GetOnlineUsers();
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User?.GetUserName();
        if(string.IsNullOrEmpty(username)) throw new HubException("Cannot get current user claim");
         
        await tracker.UserDisconnected(username, Context.ConnectionId);

        await Clients.Others.SendAsync("UserIsOffline", username);

        var currentUsers = await tracker.GetOnlineUsers();
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
    }
}
