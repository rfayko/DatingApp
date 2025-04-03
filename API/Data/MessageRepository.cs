using System;
using System.IO.Compression;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public void AddMessage(Message msg)
    {
        context.Messages.Add(msg);
    }

    public void DeleteMessage(Message msg)
    {
        context.Messages.Remove(msg);
    }

    public async Task<Connection?> GetConnection(string connectionId)
    {
        return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<Group?> GetMessageGroup(string groupName)
    {
        return await context.Groups
            .Include(g => g.Connections)
            .FirstOrDefaultAsync(g => g.Name == groupName);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.Username && !m.RecipientDeleted),
            "Outbox" => query.Where(m => m.Sender.UserName == messageParams.Username && !m.SenderDeleted),
            _ => query.Where(m => m.Recipient.UserName == messageParams.Username && m.DateRead == null && !m.RecipientDeleted)
        };

        var messagesQuery = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

        return await PagedList<MessageDto>.CreateAsync(messagesQuery, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string chatPartnerUserName)
    {
        var query = context.Messages
                .Where(
                    x => x.RecipientUsername == currentUserName 
                    && x.SenderUsername == chatPartnerUserName
                    && !x.RecipientDeleted 
                    ||
                    x.SenderUsername == currentUserName 
                    && x.RecipientUsername == chatPartnerUserName 
                    && !x.SenderDeleted)
                .OrderBy(x => x.MessageSent)
                .AsQueryable();

            //Get unread messages and mark as read (set DateRead = Now)
            var unread = query.Where(x => x.DateRead == null 
                && x.RecipientUsername == currentUserName).ToList();
            if(unread.Any())
            {
                unread.ForEach(um => um.DateRead = DateTime.UtcNow);
            }

        return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
        return await context.Groups
            .Include(g => g.Connections)
            .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }
}
