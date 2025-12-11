using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using TasqueManager.Abstractions.ServiceAbstractions;
using TasqueManager.WebHost.Settings;

namespace TasqueManager.WebHost.Services
{
    public class CurrencyExchangeService : ICurrencyExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyExchangeService> _logger;
        private readonly ForeignUrlStrings _foreignUrlStrings;

        public CurrencyExchangeService(IMemoryCache cache, ILogger<CurrencyExchangeService> logger, IOptions<ForeignUrlStrings> options)
        {
            _httpClient = new HttpClient();
            _cache = cache;
            _logger = logger;
            _foreignUrlStrings = options.Value;
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
                HttpResponseMessage response = await _httpClient.GetAsync(_foreignUrlStrings.ExchangeRateUrl);
                response.EnsureSuccessStatusCode();
                string Result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Exchange rate was fetched from external API");
                return Result;

            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"External API access failure: {ex.Message}");
            }
        }
    }
}
