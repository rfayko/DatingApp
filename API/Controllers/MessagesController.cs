using System;
using API.DTOs;
using API.Entities;
using API.Externsions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(IMessageRepository messageRepo, IUserRepository userRepo, IMapper mapper) : BaseApiController
{
  [HttpPost]
  public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
  {
    var userName = User.GetUserName();

    if (userName == createMessageDto.RecipientUserName.ToLower()) 
      return BadRequest("Can't send a message to yourself.");

    var sender = await userRepo.GetUserByUsernameAsync(userName);
    var recipient = await userRepo.GetUserByUsernameAsync(createMessageDto.RecipientUserName);

    if (recipient == null || sender == null) return BadRequest("Cant' send message at this time.");

    var msg = new Message
      {
        Sender = sender,
        Recipient = recipient,
        SenderUserName = sender.UserName,
        RecipientUserName = recipient.UserName,
        Content = createMessageDto.Content
      };

    messageRepo.AddMessage(msg);

    if (await messageRepo.SaveAllAsync()) return Ok(mapper.Map<MessageDto>(msg));

    return BadRequest("Failed to save message");
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
  {
    messageParams.Username = User.GetUserName();
    var messages = await messageRepo.GetMessagesForUser(messageParams);

    Response.AddPaginationHeader(messages);

    return messages;
  }

  [HttpGet("thread/{username}")]
  public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
  {
    var currentUserName = User.GetUserName();
    return Ok(await messageRepo.GetMessageThread(currentUserName, username));
  }

}
