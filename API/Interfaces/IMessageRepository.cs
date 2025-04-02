using System;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
  void AddMessage(Message msg);
  void DeleteMessage(Message msg);
  Task<Message?> GetMessage(int id);
  Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
  Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName);
  Task<bool> SaveAllAsync();

  //SignalR IF Methods
  void AddGroup(Group group);
  void RemoveConnection(Connection connection);
  Task<Connection?> GetConnection(string connectionId);
  Task<Group?> GetMessageGroup(string groupName);
  Task<Group?> GetGroupForConnection(string connectionId);

}
