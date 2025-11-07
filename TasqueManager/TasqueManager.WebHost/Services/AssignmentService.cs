using AutoMapper;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using TasqueManager.Abstractions.RepositoryAbstractions;
using TasqueManager.Abstractions.ServiceAbstractions;
using TasqueManager.Contracts.Assignment;
using TasqueManager.Domain;

namespace TasqueManager.WebHost.Services
{
    public class AssignmentService: IAssignmentService
    {
        private readonly IMapper _mapper;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient; 
        private readonly IMemoryCache _cache;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(
            IMapper mapper,
            IAssignmentRepository assignmentRepository,
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<AssignmentService> logger)
        {
            _mapper = mapper;
            _assignmentRepository = assignmentRepository;
            _unitOfWork = unitOfWork;
            _httpClient = new HttpClient();
            _cache = cache;
            _logger = logger;
        }
        /// <summary>
        /// Получить задачу.
        /// </summary>
        /// <param name="id"> Идентификатор. </param>
        /// <returns> ДТО задачи. </returns>
        public async Task<AssignmentDto> GetByIdAsync(Guid id) 
        {
            var assignment = await _assignmentRepository.GetAsync(id, CancellationToken.None);
            return _mapper.Map<Assignment, AssignmentDto>(assignment);
        }

        /// <summary>
        /// Создать задачу.
        /// </summary>
        /// <param name="creatingAssignmentDto"> ДТО создаваемой задачи. </param>
        public async Task<Guid> CreateAsync(CreatingAssignmentDto creatingAssignmentDto) 
        {
            var assignment = _mapper.Map<CreatingAssignmentDto, Assignment>(creatingAssignmentDto);
            var createdAssignment = await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveChangesAsync();
            return createdAssignment.Id;
        }

        /// <summary>
        /// Изменить задачу.
        /// </summary>
        /// <param name="id"> Иентификатор. </param>
        /// <param name="updatingAssignmentDto"> ДТО редактируемой задачи. </param>
        public async Task UpdateAsync(Guid id, UpdatingAssignmentDto updatingAssignmentDto) 
        {
            var assignment = await _assignmentRepository.GetAsync(id, CancellationToken.None) ?? 
                throw new KeyNotFoundException($"Task with id: {id} not found");

            assignment.Title = updatingAssignmentDto.Title;
            assignment.Description = updatingAssignmentDto.Description;
            assignment.Status = updatingAssignmentDto.Status;
            assignment.DueDate = updatingAssignmentDto.DueDate;

            _assignmentRepository.Update(assignment);
            await _assignmentRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id"> Идентификатор. </param>
        public async Task DeleteAsync(Guid id) 
        {

            var assignment = await _assignmentRepository.GetAsync(id, CancellationToken.None) ?? 
                throw new KeyNotFoundException($"Task with id: {id} not found");

            assignment.Deleted = true;
            await _assignmentRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Получить постраничный список.
        /// </summary>
        /// <param name="filterDto"> ДТО фильтра. </param>
        /// <returns> Список задач. </returns>
        public async Task<ICollection<AssignmentDto>> GetPagedAsync(AssignmentFilterDto filterDto) 
        {
            ICollection<Assignment> entities = await _assignmentRepository.GetPagedAsync(filterDto);
            return _mapper.Map<ICollection<Assignment>, ICollection<AssignmentDto>>(entities);
        }

        /// <summary>
        /// Получить курс валют.
        /// </summary>
        public async Task<string> GetExchangeRateAsync() 
        {
            string cacheKey = $"ExchangeRate";
            if (_cache.TryGetValue(cacheKey, out string? cachedRate) && cachedRate != null)
            {
                _logger.LogInformation("Exchange rate was retrieved from cache");
                return cachedRate;
            }
            var freshRate = await FetchExchangeRate();

            // Сохраняем полученные данные в кэш на 5 минут
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, freshRate, cacheOptions);

            return freshRate;
        }

        private async Task<string> FetchExchangeRate() 
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://www.cbr-xml-daily.ru/daily_json.js");
                response.EnsureSuccessStatusCode();
                string Result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Exchange rate was fetched from external API");
                return Result;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"External API access failure: {ex.Message}", ex);
            }
        }
    }
}
