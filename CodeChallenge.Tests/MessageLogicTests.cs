using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using CodeChallenge.Api.Logic;

namespace CodeChallenge.Tests
{
    public class MessageLogicTests
    {
        private readonly Mock<IMessageRepository> _repoMock;
        private readonly MessageLogic _logic;
        private readonly Guid _orgId;

        public MessageLogicTests()
        {
            _repoMock = new Mock<IMessageRepository>();
            _logic = new MessageLogic(_repoMock.Object);
            _orgId = Guid.NewGuid();
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldCreateMessage_WhenValid()
        {
            var request = new CreateMessageRequest { Title = "New Message", Content = "This is valid content." };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title)).ReturnsAsync((Message?)null);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldReturnConflict_WhenTitleExists()
        {
            var request = new CreateMessageRequest { Title = "Duplicate", Content = "Some valid content" };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title)).ReturnsAsync(new Message { Title = "Duplicate" });

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldReturnValidationError_WhenContentInvalid()
        {
            var request = new CreateMessageRequest { Title = "Valid Title", Content = "short" };

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            var request = new UpdateMessageRequest { Title = "Updated", Content = "Updated content", IsActive = true };

            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>())).ReturnsAsync((Message?)null);

            var result = await _logic.UpdateMessageAsync(_orgId, Guid.NewGuid(), request);

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnValidationError_WhenMessageInactive()
        {
            var existing = new Message { IsActive = false };
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>())).ReturnsAsync(existing);

            var request = new UpdateMessageRequest { Title = "Updated", Content = "Updated content", IsActive = true };

            var result = await _logic.UpdateMessageAsync(_orgId, Guid.NewGuid(), request);

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>())).ReturnsAsync((Message?)null);

            var result = await _logic.DeleteMessageAsync(_orgId, Guid.NewGuid());

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnValidationError_WhenMessageInactive()
        {
            var existing = new Message { IsActive = false };
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>())).ReturnsAsync(existing);

            var result = await _logic.DeleteMessageAsync(_orgId, Guid.NewGuid());

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnConflict_WhenTitleAlreadyExistsForAnotherMessage()
        {
            var existingTitle = new Message { Id = Guid.NewGuid(), IsActive = true, Title = "Other" };
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>())).ReturnsAsync(new Message { Id = Guid.NewGuid(), IsActive = true });
            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, "Other")).ReturnsAsync(existingTitle);

            var request = new UpdateMessageRequest { Title = "Other", Content = "Valid content", IsActive = true };

            var result = await _logic.UpdateMessageAsync(_orgId, Guid.NewGuid(), request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnUpdated_WhenValid()
        {
            var id = Guid.NewGuid();
            var existing = new Message { Id = id, OrganizationId = _orgId, Title = "Original", Content = "Original content", IsActive = true };

            _repoMock.Setup(r => r.GetByIdAsync(_orgId, id)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, "Updated Title")).ReturnsAsync((Message?)null);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

            var request = new UpdateMessageRequest { Title = "Updated Title", Content = "Updated content", IsActive = true };

            var result = await _logic.UpdateMessageAsync(_orgId, id, request);

            result.Should().BeOfType<Updated>();
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnDeleted_WhenValid()
        {
            var id = Guid.NewGuid();
            var existing = new Message { Id = id, OrganizationId = _orgId, Title = "ToDelete", Content = "Content", IsActive = true };

            _repoMock.Setup(r => r.GetByIdAsync(_orgId, id)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.DeleteAsync(_orgId, id)).ReturnsAsync(true);

            var result = await _logic.DeleteMessageAsync(_orgId, id);

            result.Should().BeOfType<Deleted>();
        }

        [Fact]
        public async Task CreateMessageAsync_TitleLengthExactly3_IsValid()
        {
            var request = new CreateMessageRequest { Title = new string('A', 3), Content = new string('C', 10) };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title)).ReturnsAsync((Message?)null);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task CreateMessageAsync_ContentLengthExactly10_IsValid()
        {
            var request = new CreateMessageRequest { Title = "Tit", Content = new string('C', 10) };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title)).ReturnsAsync((Message?)null);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task CreateMessageAsync_TitleLengthExactly200_IsValid()
        {
            var request = new CreateMessageRequest { Title = new string('T', 200), Content = new string('C', 10) };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title)).ReturnsAsync((Message?)null);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task CreateMessageAsync_ContentLengthExactly1000_IsValid()
        {
            var request = new CreateMessageRequest { Title = "Title", Content = new string('C', 1000) };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title)).ReturnsAsync((Message?)null);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Created<Message>>();
        }
    }
}
