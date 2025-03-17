using API.DTOs;
using API.Entities;
using API.Externsions;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(IMessageRepository messageRepo, IUserRepository userRepo, IMapper mapper) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"][0];  // Query returns a StringValues object which is array-like
        var caller = Context.User?.GetUserName();

        if( string.IsNullOrEmpty(caller) || string.IsNullOrEmpty(otherUser)) throw new HubException("Group Error: Caller and Other User cannot be null");
        var groupName = GetGroupName(caller, otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName);

        var messages = await messageRepo.GetMessageThread(caller, otherUser);

        await Clients.Group(groupName).SendAsync("ReceivedMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        //When disconnecting from Message Hub group is auto removed from Groups.
        await RemoveFromMessageGroup();
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
      if (group != null && group.Connections.Any(c => c.UserName == recipient.UserName))
      {
        msg.DateRead = DateTime.UtcNow;
      }

      messageRepo.AddMessage(msg);

      if (await messageRepo.SaveAllAsync())
      {
        await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(msg));
      } 
      
      return;
    }

    private async Task<bool> AddToGroup(string groupName)
    {
      var username = Context.User?.GetUserName() ?? throw new HubException("Cannot get username");
      var group = await messageRepo.GetMessageGroup(groupName);
      var connection = new Connection{ConnectionId = Context.ConnectionId, UserName = username};

      if (group == null)
      {
        group = new Group{Name = groupName};
        messageRepo.AddGroup(group);
      }

      group.Connections.Add(connection);

      return await messageRepo.SaveAllAsync();
    }

    private async Task RemoveFromMessageGroup()
    {
      var connection = await messageRepo.GetConnection(Context.ConnectionId);
      if (connection != null)
      {
        messageRepo.RemoveConnection(connection);
        await messageRepo.SaveAllAsync();
      }
    }

    private string GetGroupName(string caller, string other) 
    {
      var stringCompare = string.CompareOrdinal(caller, other) < 0;
      return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
