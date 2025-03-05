using System;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddMessage(Message msg)
    {
        context.Messages.Add(msg);
    }

    public void DeleteMessage(Message msg)
    {
        context.Messages.Remove(msg);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.Username),
            "Outbox" => query.Where(m => m.Sender.UserName == messageParams.Username),
            _ => query.Where(m => m.Recipient.UserName == messageParams.Username && m.DateRead == null)
        };

        var messagesQuery = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

        return await PagedList<MessageDto>.CreateAsync(messagesQuery, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
    {
        var messages = await context.Messages
                .Include(x => x.Sender).ThenInclude(x => x.Photos)
                .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                .Where(x => x.RecipientUserName == currentUserName && x.SenderUserName == recipientUserName ||
                    x.SenderUserName == currentUserName && x.RecipientUserName == recipientUserName )
                .OrderBy(x => x.MessageSent)
                .ToListAsync();

            //Get unread messages and mark as read (set DateRead = Now)
            var unread = messages.Where(x => x.DateRead == null && x.RecipientUserName == currentUserName).ToList();
            if(unread.Any())
            {
                unread.ForEach(um => um.DateRead = DateTime.UtcNow);
                await context.SaveChangesAsync();
            }

        return mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
