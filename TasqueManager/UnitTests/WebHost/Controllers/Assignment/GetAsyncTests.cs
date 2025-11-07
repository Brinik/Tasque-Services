using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TasqueManager.Abstractions.RepositoryAbstractions;
using TasqueManager.Abstractions.ServiceAbstractions;
using TasqueManager.Contracts.Assignment;
using TasqueManager.Domain;
using TasqueManager.WebHost.Controllers;
using TasqueManager.WebHost.Models;
using Xunit;

namespace TasqueManager.Tests.WebHost.Controllers.Assignment
{
    public class GetAsyncTests
    {
        private readonly Mock<IAssignmentService> _assignmentServiceMock;
        private readonly AssignmentController _assignmentController;
        private readonly IFixture _fixture;
        private readonly Mock<IMapper> _mapperMock;

        public GetAsyncTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _assignmentServiceMock = _fixture.Freeze<Mock<IAssignmentService>>();
            _mapperMock = _fixture.Freeze<Mock<IMapper>>();
            _assignmentController = _fixture.Build<AssignmentController>().OmitAutoProperties().Create();
        }

        [Fact]
        public async Task GetAsync_AssignmentIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var assignmentId = Guid.Parse("def47943-7aaf-44a1-ae21-05aa4948b165");
            AssignmentDto assignment = null!;

            _assignmentServiceMock
                .Setup(service => service.GetByIdAsync(assignmentId))
                .ReturnsAsync(assignment);

            // Act
            var result = await _assignmentController.GetAsync(assignmentId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAsync_AssignmentExists_ReturnsOkWithMappedAssignment()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var assignmentDto = _fixture.Create<AssignmentDto>();
            var assignmentResult = _fixture.Create<Domain.Assignment>();

            _assignmentServiceMock
                .Setup(service => service.GetByIdAsync(assignmentId))
                .ReturnsAsync(assignmentDto);

            _mapperMock
                .Setup(mapper => mapper.Map<Domain.Assignment>(assignmentDto))
                .Returns(assignmentResult);

            // Act
            var result = await _assignmentController.GetAsync(assignmentId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            if (okResult != null)
            {
                okResult.Value.Should().BeEquivalentTo(assignmentResult);
            }
        }
    }
}
