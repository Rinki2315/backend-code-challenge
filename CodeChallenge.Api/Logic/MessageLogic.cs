using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic;

public class MessageLogic : IMessageLogic
{
    private readonly IMessageRepository _repository;

    public MessageLogic(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3 || request.Title.Length > 200)
            errors["Title"] = new[] { "Title is required and must be between 3 and 200 characters." };

        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 10 || request.Content.Length > 1000)
            errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };

        if (errors.Count > 0)
            return new ValidationError(errors);

        var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (existing != null)
            return new Conflict("A message with the same title already exists.");

        var message = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(message);
        return new Created<Message>(created);
    }

    public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
    {
        var message = await _repository.GetByIdAsync(organizationId, id);
        if (message == null)
            return new NotFound("Message not found.");

        if (!message.IsActive)
            return new ValidationError(new Dictionary<string, string[]> { { "IsActive", new[] { "Cannot update inactive message." } } });

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3 || request.Title.Length > 200)
            errors["Title"] = new[] { "Title is required and must be between 3 and 200 characters." };

        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 10 || request.Content.Length > 1000)
            errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };

        if (errors.Count > 0)
            return new ValidationError(errors);

        var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (existing != null && existing.Id != id)
            return new Conflict("A message with the same title already exists.");

        message.Title = request.Title;
        message.Content = request.Content;
        message.IsActive = request.IsActive;
        message.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(message);
        return updated != null ? new Updated() : new NotFound("Message not found.");
    }

    public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
    {
        var message = await _repository.GetByIdAsync(organizationId, id);
        if (message == null)
            return new NotFound("Message not found.");

        if (!message.IsActive)
            return new ValidationError(new Dictionary<string, string[]> { { "IsActive", new[] { "Cannot delete inactive message." } } });

        var deleted = await _repository.DeleteAsync(organizationId, id);
        return deleted ? new Deleted() : new NotFound("Message not found.");
    }

    public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
    {
        return await _repository.GetByIdAsync(organizationId, id);
    }

    public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
    {
        return await _repository.GetAllByOrganizationAsync(organizationId);
    }
}
