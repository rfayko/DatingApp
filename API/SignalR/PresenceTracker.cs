using System;
using Humanizer;

namespace API.SignalR;

public class PresenceTracker
{
  private static readonly Dictionary<string, List<string>> OnLineUsers = [];

  public Task<bool> UserConnected(string username, string connectionId)
  {
    var isOnline = false;
    lock (OnLineUsers)
    {
      if (OnLineUsers.ContainsKey(username))
      {
        OnLineUsers[username].Add(connectionId);
      }
      else
      {
        OnLineUsers.Add(username, [connectionId]);
        isOnline = true;
      }
    }

    return Task.FromResult(isOnline);
  }

  
  public Task<bool> UserDisconnected(string username, string connectionId)
  {
    var isOffline = false;
    lock (OnLineUsers)
    {
      if (OnLineUsers.ContainsKey(username))
      {
        OnLineUsers[username].Remove(connectionId); 
        if(!OnLineUsers[username].Any())
        {
          OnLineUsers.Remove(username);
          isOffline = true;
        }
      }
    }

    return Task.FromResult(isOffline);
  }

  public Task<string[]> GetOnlineUsers()
  {
    string[] onlineUsers;
    lock (OnLineUsers)
    {
      onlineUsers = OnLineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
    }

    return Task.FromResult(onlineUsers);
  }

  // Below gets the list of presence connections for a given user. 
  // A user may possibly have presence connections from multiple devices.
  public static Task<List<string>> GetPresenceHubConnectionsForUser(string username)
  {
    List<string> connectionIds;

    if(OnLineUsers.TryGetValue(username, out var connections))
    {
      lock(connections)
      {
        connectionIds = [.. connections];  // C# 12 collections spread operator
      }
    }
    else
    {
      connectionIds = [];
    }

    return Task.FromResult(connectionIds);
  }
}
