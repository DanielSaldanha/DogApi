using DogApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace DogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DogController : ControllerBase
    {
        private readonly IHttpClientFactory _IHttpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly IDistributedCache _cacheRedis;
        public DogController(IDistributedCache cacheRedis,IMemoryCache cache,IHttpClientFactory ihttpclientfactory)
        {
            _IHttpClientFactory = ihttpclientfactory;
            _cache = cache;
            _cacheRedis = cacheRedis;
        }
        [HttpGet("pegue um cão")]
        public async Task<ActionResult> DogerGet()
        {
            // Definir a chave de cache
            string cacheKey = "DogBreeds";

            // Tentar obter os dados do cache
            if (_cache.TryGetValue(cacheKey, out DogModel cachedData))
            {
                // Retornar os dados do cache
                return Ok(cachedData);
            }
            var RedisCache = await _cacheRedis.GetStringAsync(cacheKey);
            if(RedisCache != null)
            {
                var RedisDog = JsonSerializer.Deserialize<DogModel>(RedisCache);
                var CacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), // Expira em 10 minutos
                    SlidingExpiration = TimeSpan.FromMinutes(5) // Renova se acessado dentro de 5 minutos
                };
                _cache.Set(cacheKey, RedisDog, CacheEntryOptions);
                return Ok(RedisDog);
            }
            // Caso os dados não estejam no cache, fazer a chamada à API
            var client = _IHttpClientFactory.CreateClient("CurrencyAPI");
            var response = await client.GetAsync("breeds/list/all");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound("Não foi possível obter os dados.");
            }

            var dogJson = await response.Content.ReadFromJsonAsync<DogModel>();
            if (dogJson == null)
            {
                return NotFound("A resposta da API está vazia ou inválida.");
            }

            // Armazenar os dados no cache com um tempo de expiração
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), // Expira em 10 minutos
                SlidingExpiration = TimeSpan.FromMinutes(5) // Renova se acessado dentro de 5 minutos
            };
            var cacheredisOptions = new DistributedCacheEntryOptions
            {

                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)

            };
            _cache.Set(cacheKey, dogJson, cacheEntryOptions);
            await _cacheRedis.SetStringAsync(cacheKey, JsonSerializer.Serialize(dogJson), cacheredisOptions);


            // Retornar os dados obtidos da API
            return Ok(dogJson);
        }
    }
}
