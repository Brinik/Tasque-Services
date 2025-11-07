using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TasqueManager.Abstractions.RepositoryAbstractions;
using TasqueManager.Abstractions.ServiceAbstractions;
using TasqueManager.Contracts.Assignment;
using TasqueManager.WebHost.Controllers;
using TasqueManager.WebHost.Models;
using Xunit;

namespace TasqueManager.Tests.WebHost.Controllers.Assignment
{
    public class GetListAsyncTests
    {
        private readonly Mock<IAssignmentService> _assignmentServiceMock;
        private readonly AssignmentController _assignmentController;
        private readonly Mock<IMapper> _mapperMock;

        public GetListAsyncTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _assignmentServiceMock = fixture.Freeze<Mock<IAssignmentService>>();
            _mapperMock = fixture.Freeze<Mock<IMapper>>();
            _assignmentController = fixture.Build<AssignmentController>().OmitAutoProperties().Create();
        }

        [Fact]
        public async Task GetListAsync_PageZeroWithItems_ReturnsBadRequest() 
        {
            var assignment = new Fixture().Create<AssignmentDto>();
            List<AssignmentDto> list = new List<AssignmentDto> { assignment };
            AssignmentFilterDto assignmentFilterDto = new AssignmentFilterDto() { ItemsPerPage = 1, Page = 0 };
            AssignmentFilterModel assignmentFilterModel = new AssignmentFilterModel() { ItemsPerPage = 1, Page = 0 };
            _assignmentServiceMock.Setup(service => service.GetPagedAsync(assignmentFilterDto)).ReturnsAsync(list);

            var result = await _assignmentController.GetListAsync(assignmentFilterModel);

            result.Should().BeAssignableTo<BadRequestResult>();
        }
    }
}
