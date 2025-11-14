using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageLogic _logic;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageLogic logic, ILogger<MessagesController> logger)
    {
        _logic = logic;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        var messages = await _logic.GetAllMessagesAsync(organizationId);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        var message = await _logic.GetMessageAsync(organizationId, id);
        if (message == null)
        {
            return NotFound();
        }
        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        var result = await _logic.CreateMessageAsync(organizationId, request);
        return result switch
        {
            Created<Message> c => CreatedAtAction(nameof(GetById), new { organizationId, id = c.Value.Id }, c.Value),
            Conflict c => Conflict(c.Message),
            ValidationError v => BadRequest(v.Errors),
            _ => StatusCode(500)
        };
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        var result = await _logic.UpdateMessageAsync(organizationId, id, request);
        return result switch
        {
            Updated => NoContent(),
            NotFound n => NotFound(n.Message),
            Conflict c => Conflict(c.Message),
            ValidationError v => BadRequest(v.Errors),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        var result = await _logic.DeleteMessageAsync(organizationId, id);
        return result switch
        {
            Deleted => NoContent(),
            NotFound n => NotFound(n.Message),
            ValidationError v => BadRequest(v.Errors),
            _ => StatusCode(500)
        };
    }
}
