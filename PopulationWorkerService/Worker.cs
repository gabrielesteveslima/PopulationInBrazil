using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using PopulationWorkerService.Extensions;
using PopulationWorkerService.Projections;

namespace PopulationWorkerService
{
    internal class Worker : BackgroundService
    {
        private const string BaseUrlIbge = "https://servicodados.ibge.gov.br";
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger) => _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = new HttpClient
                {BaseAddress = new Uri(BaseUrlIbge)};

            var waitAndRetryPolicyAsync = Policy
                .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2),
                    (result, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            $"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                    });

            while (!stoppingToken.IsCancellationRequested)
            {
                var response =
                    await waitAndRetryPolicyAsync.ExecuteAsync(() =>
                        httpClient.GetAsync("/api/v1/projecoes/populacao", stoppingToken));

                var populationInBrazil = await response.Content.ReadAsJsonAsync<IbgeResponse>();

                _logger.LogInformation("---- Projection is {projection} at: {time}",
                    populationInBrazil.ProjectionInBrazil.Population,
                    DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}