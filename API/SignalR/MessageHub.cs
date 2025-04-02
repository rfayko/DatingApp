using System.Formats.Asn1;
using API.DTOs;
using API.Entities;
using API.Externsions;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(
  IMessageRepository messageRepo, 
  IUserRepository userRepo, 
  IMapper mapper,
  IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        // API expectations for messaging are that the client will send a SignalR http request including the
        // target user plus the token in the inital request query string. We use the Identity User to get logged in User.
        var httpContext = Context.GetHttpContext(); //SignalR requests uses Http to initiate a connection with Server which then the server negotiates best protocol to use going forward. 
        var otherUser = httpContext?.Request.Query["user"][0];  // Query returns a StringValues object which is array-like
        var caller = Context.User?.GetUserName();

        if( string.IsNullOrEmpty(caller) || string.IsNullOrEmpty(otherUser)) throw new HubException("Group Error: Caller and Other User cannot be null");
        var groupName = GetGroupName(caller, otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);

        // Below is to allow client to interrogate a group to see if the other user is in the group
        // before marking message as read.
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await messageRepo.GetMessageThread(caller, otherUser);

        // Now sending thread to caller instead of group.
        await Clients.Caller.SendAsync("ReceivedMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        //When disconnecting from Message Hub group is auto removed from Groups.
        var group = await RemoveConnectionFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
      var userName = Context.User?.GetUserName();

      if (userName == null || userName == createMessageDto.RecipientUserName.ToLower()) 
        throw new HubException("User can't be null nor can you send a message to yourself.");

      var sender = await userRepo.GetUserByUsernameAsync(userName);
      var recipient = await userRepo.GetUserByUsernameAsync(createMessageDto.RecipientUserName);

      if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null) throw new HubException("Cant' send message at this time.");

      var msg = new Message
        {
          Sender = sender,
          Recipient = recipient,
          SenderUserName = sender.UserName,
          RecipientUserName = recipient.UserName,
          Content = createMessageDto.Content
        };

      var groupName = GetGroupName(sender.UserName, recipient.UserName);
      var group = await messageRepo.GetMessageGroup(groupName);
      
      //If below true than recipient is in an active chat thread and can mark message as read
      //Also, below indicates that the user is online with connections in both Message and Presence hubs.
      //If false, user only has a connection in the presence Hub. 
      if (group != null && group.Connections.Any(c => c.Username == recipient.UserName))
      {
        msg.DateRead = DateTime.UtcNow;
      }
      else
      {
        var connections = await PresenceTracker.GetPresenceHubConnectionsForUser(recipient.UserName);
        if(connections != null && connections.Any())
        {
          await presenceHub.Clients.Clients(connections)
            .SendAsync("NewMessageReceived", new {username = sender.UserName, knownAs = sender.KnownAs});
        }
      }


      messageRepo.AddMessage(msg);

      if (await messageRepo.SaveAllAsync())
      {
        await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(msg));
      } 
      
      return;
    }

    private async Task<Group> AddToGroup(string groupName)
    {
      var username = Context.User?.GetUserName() ?? throw new HubException("Cannot get username");
      var group = await messageRepo.GetMessageGroup(groupName);
      var connection = new Connection{ConnectionId = Context.ConnectionId, Username = username};

      if (group == null)
      {
        group = new Group{Name = groupName};
        messageRepo.AddGroup(group);
      }

      group.Connections.Add(connection);

      if(await messageRepo.SaveAllAsync()) return group;

      throw new HubException("Unable to add connection to group.");
    }

    private async Task<Group> RemoveConnectionFromMessageGroup()
    {
      var group = await messageRepo.GetGroupForConnection(Context.ConnectionId) ?? throw new HubException("Failed to obtain group from connection.");
      var connection = group?.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
      if (connection != null && group != null)
      {
        messageRepo.RemoveConnection(connection);
        if (await messageRepo.SaveAllAsync()) return group;
      }

      throw new HubException("Failed to remove connection from group.");
    }

    private string GetGroupName(string caller, string other) 
    {
      var stringCompare = string.CompareOrdinal(caller, other) < 0;
      return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
