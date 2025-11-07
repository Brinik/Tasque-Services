using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TasqueManager.Abstractions.ServiceAbstractions;
using TasqueManager.Contracts.Assignment;
using TasqueManager.Domain;
using TasqueManager.WebHost.Attributes;
using TasqueManager.WebHost.Models;


namespace TasqueManager.WebHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssignmentController: ControllerBase
    {
        private readonly IAssignmentService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<AssignmentController> _logger;
        public AssignmentController(IAssignmentService service, IMapper mapper,
            ILogger<AssignmentController> logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("ExchangeRate")]
        [RequestTimeout(10)]
        public async Task<IActionResult> GetExchangeRateAsync()
        {
            var cancellationToken = HttpContext.RequestAborted;
            try
            {
                return Ok(await _service.GetExchangeRateAsync());
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) 
            {
                throw;
            }
        }

        /// <summary>
        /// Проверка остановки по таймауту
        /// </summary>
        [HttpGet("slow-operation")]
        [RequestTimeout(5)]
        public async Task<IActionResult> SlowOperation()
        {
            var cancellationToken = HttpContext.RequestAborted;
            _logger.LogInformation("Starting slow operation at {Time}", DateTime.Now);

            try
            {
                await Task.Delay(10000, cancellationToken);

                _logger.LogInformation("Slow operation completed at {Time}", DateTime.Now);
                return Ok("Operation completed");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
        }

        /// <summary>
        /// Проверка отлавливания исключений
        /// </summary>
        /// <returns>Ошибка 500</returns>
        [HttpGet("null-reference")]
        public IActionResult NullReference()
        {
            string test = null!;
            return Ok(test.Length);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var assignment = await _service.GetByIdAsync(id);
            if (assignment == null) 
            {
                return NotFound($"Assignment with ID {id} not found");
            }
            return Ok(_mapper.Map<Assignment>(assignment)); 
        }

        [HttpPost("list")]
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
            try
            {
                await _service.UpdateAsync(id, _mapper.Map<UpdatingAssignmentModel, UpdatingAssignmentDto>(assignmentModel));
                return Ok();
            }
            catch (KeyNotFoundException ex) 
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id) 
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok();
            }
            catch (KeyNotFoundException ex) 
            {
                return NotFound(ex.Message);
            }
        }
    }
}
