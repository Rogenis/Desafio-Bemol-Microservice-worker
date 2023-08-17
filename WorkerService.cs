using System.Threading.Tasks;
using Grpc;
using MicroserviceWorker;
using Microsoft.Extensions.Logging;

namespace MicroserviceWorker
{
    public class WorkerService : WorkerBase
    {
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }

        public override Task<ProcessItemResponse> ProcessItem(ProcessItemRequest request, ServerCallContext context)
        {
            // Implemente o processamento aqui
            _logger.LogInformation($"Processing item {request.Id}: {request.Data}");
            
            return Task.FromResult(new ProcessItemResponse
            {
                Success = true,
                Message = $"Item {request.Id} processed successfully"
            });
        }
    }
}
