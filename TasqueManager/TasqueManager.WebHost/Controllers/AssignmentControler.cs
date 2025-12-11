using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using TasqueManager.Abstractions.ServiceAbstractions;
using Message;
using TasqueManager.Contracts.Assignment;
using TasqueManager.Domain;
using TasqueManager.WebHost.Models;


namespace TasqueManager.WebHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssignmentController: ControllerBase
    {
        private readonly IAssignmentService _service;
        private readonly IMapper _mapper;
        public AssignmentController(IAssignmentService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var assignment = await _service.GetByIdAsync(id);
            return Ok(_mapper.Map<Assignment>(assignment)); 
        }

        [HttpPost("getList")]
        public async Task<IActionResult> GetListAsync(AssignmentFilterModel filterModel)
        {
            if (filterModel == null || (filterModel.ItemsPerPage != 0 & filterModel.Page <= 0)) 
            {
                return BadRequest();
            }
            var filterDto = _mapper.Map<AssignmentFilterModel, AssignmentFilterDto>(filterModel);
            return Ok(_mapper.Map<List<AssignmentModel>>(await _service.GetPagedAsync(filterDto)));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreatingAssignmentModel assignmentModel)
        {
            return Ok(await _service.CreateAsync(_mapper.Map<CreatingAssignmentDto>(assignmentModel)));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditAsync(Guid id, UpdatingAssignmentModel assignmentModel)
        {
            await _service.UpdateAsync(id, _mapper.Map<UpdatingAssignmentModel, UpdatingAssignmentDto>(assignmentModel));
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id) 
        {
            await _service.DeleteAsync(id);
            return Ok();
        }
    }
}
