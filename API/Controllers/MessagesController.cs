using System;
using API.DTOs;
using API.Entities;
using API.Externsions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IUnitOfWork uow, IMapper mapper) : BaseApiController
{
  [HttpPost]
  public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
  {
    var userName = User.GetUserName();

    if (userName == createMessageDto.RecipientUserName.ToLower()) 
      return BadRequest("Can't send a message to yourself.");

    var sender = await uow.UserRepository.GetUserByUsernameAsync(userName);
    var recipient = await uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUserName);

    if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null) return BadRequest("Cant' send message at this time.");

    var msg = new Message
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };

    uow.MessageRepository.AddMessage(msg);

    if (await uow.Complete()) return Ok(mapper.Map<MessageDto>(msg));

    return BadRequest("Failed to save message");
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
  {
    messageParams.Username = User.GetUserName();
    var messages = await uow.MessageRepository.GetMessagesForUser(messageParams);

    Response.AddPaginationHeader(messages);

    return messages;
  }

  [HttpGet("thread/{username}")]
  public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
  {
    var currentUserName = User.GetUserName();
    return Ok(await uow.MessageRepository.GetMessageThread(currentUserName, username));
  }

  [HttpDelete("{id:int}")]
  public async Task<ActionResult> DeleteMessage(int id)
  {
    var currentUserName = User.GetUserName();
    var msg = await uow.MessageRepository.GetMessage(id);
    if(msg == null) return BadRequest("Could not mark message for deletion.");

    if(msg.SenderUsername != currentUserName || msg.RecipientUsername != currentUserName) Forbid();

    if (msg.SenderUsername == currentUserName) msg.SenderDeleted = true;
    if (msg.RecipientUsername == currentUserName) msg.RecipientDeleted = true;

    if (msg is {SenderDeleted: true, RecipientDeleted: true})
    {
      uow.MessageRepository.DeleteMessage(msg);
    }

    if (await uow.Complete()) 
    {
      return Ok();
    }

    return BadRequest("Problem deleting message");
  } 

}
