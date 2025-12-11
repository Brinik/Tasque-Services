using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasqueManager.Abstractions.ServiceAbstractions
{
    public interface ICurrencyExchangeRateService
    {
        /// <summary>
        /// Получить курс валют
        /// </summary>
        /// <returns>json с курсом</returns>
        Task<string> GetExchangeRateAsync();
    }
}
