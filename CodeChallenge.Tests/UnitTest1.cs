using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Tests
{
    public class UnitTest1
    {
        private readonly Mock<IMessageRepository> _repoMock;
        private readonly MessageLogic _logic;
        private readonly Guid _orgId;

        public UnitTest1()
        {
            _repoMock = new Mock<IMessageRepository>();
            _logic = new MessageLogic(_repoMock.Object);
            _orgId = Guid.NewGuid();
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldCreateMessage_WhenValid()
        {
            var request = new CreateMessageRequest
            {
                Title = "New Message",
                Content = "This is valid content."
            };
            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title))
                     .ReturnsAsync((Message?)null);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                     .ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Created<Message>>();
            var created = (Created<Message>)result;
            created.Value.Title.Should().Be(request.Title);
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldReturnConflict_WhenTitleExists()
        {
            var request = new CreateMessageRequest
            {
                Title = "Duplicate",
                Content = "Some valid content"
            };
            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title))
                     .ReturnsAsync(new Message { Title = "Duplicate" });

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldReturnValidationError_WhenContentInvalid()
        {
            var request = new CreateMessageRequest
            {
                Title = "Valid Title",
                Content = "short"
            };

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            var request = new UpdateMessageRequest
            {
                Title = "Updated",
                Content = "Updated content",
                IsActive = true
            };
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>()))
                     .ReturnsAsync((Message?)null);

            var result = await _logic.UpdateMessageAsync(_orgId, Guid.NewGuid(), request);

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnValidationError_WhenMessageInactive()
        {
            var existing = new Message { IsActive = false };
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>()))
                     .ReturnsAsync(existing);

            var request = new UpdateMessageRequest
            {
                Title = "Updated",
                Content = "Updated content",
                IsActive = true
            };

            var result = await _logic.UpdateMessageAsync(_orgId, Guid.NewGuid(), request);

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>()))
                     .ReturnsAsync((Message?)null);

            var result = await _logic.DeleteMessageAsync(_orgId, Guid.NewGuid());

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnValidationError_WhenMessageInactive()
        {
            var existing = new Message { IsActive = false };
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>()))
                     .ReturnsAsync(existing);

            var result = await _logic.DeleteMessageAsync(_orgId, Guid.NewGuid());

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnConflict_WhenTitleAlreadyExistsForAnotherMessage()
        {
            var existing = new Message { Id = Guid.NewGuid(), IsActive = true, Title = "Other" };
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>()))
                     .ReturnsAsync(new Message { Id = Guid.NewGuid(), IsActive = true });
            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, "Other"))
                     .ReturnsAsync(existing);

            var request = new UpdateMessageRequest
            {
                Title = "Other",
                Content = "Valid content",
                IsActive = true
            };

            var result = await _logic.UpdateMessageAsync(_orgId, Guid.NewGuid(), request);

            result.Should().BeOfType<Conflict>();
        }
    }
}
