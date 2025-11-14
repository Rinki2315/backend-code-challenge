using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        var messages = await _repository.GetAllByOrganizationAsync(organizationId);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
       
        var message = await _repository.GetByIdAsync(organizationId, id);
        if (message == null)
        {
            return NotFound();
        }
        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        
        var newMessage = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content
        };

        var createdMessage = await _repository.CreateAsync(newMessage);
        return CreatedAtAction(nameof(GetById), new { organizationId, id = createdMessage.Id }, createdMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        
        var existingMessage = await _repository.GetByIdAsync(organizationId, id);
        if (existingMessage == null)
        { 
            return NotFound(); 
        }

        existingMessage.Title = request.Title;
        existingMessage.Content = request.Content;
        existingMessage.IsActive = request.IsActive;

        await _repository.UpdateAsync(existingMessage);
        return Ok(existingMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        
        var deleted = await _repository.DeleteAsync(organizationId, id);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}
