using Incidents_Api.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Incidents_Api
{
    public class IncidencesApi
    {
        private readonly IRepository _repository;

        public IncidencesApi(IRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        }

        [FunctionName("Incident_Get")]
        public async Task<IActionResult> GetAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "incidents")] HttpRequest req,
            ILogger log)
        {

            return new OkObjectResult(JsonConvert.SerializeObject(await _repository.GetIncidentsAsync()));
        }

        [FunctionName("Incident_GetByWorkerId")]
        public async Task<IActionResult> GetByPartitionKeyAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "incidents/{id}")] HttpRequest req,
           ILogger log, string id)
        {
            var incident = await _repository.GetIncidentByIdAsync(id);
            return new OkObjectResult(JsonConvert.SerializeObject(incident));
        }

        [FunctionName("Incident_Create")]
        public async Task<IActionResult> CreateAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "incidents")] HttpRequest req,
           ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var incident = JsonConvert.DeserializeObject<Incident>(requestBody);

            return new OkObjectResult(JsonConvert.SerializeObject(await _repository.UpsertIncidentAsync(incident)));
        }

        [FunctionName("Incident_Update")]
        public async Task<IActionResult> UpdateAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "incidents/{id}")] HttpRequest req,
           ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var incident = JsonConvert.DeserializeObject<Incident>(requestBody);
            return new OkObjectResult(JsonConvert.SerializeObject(await _repository.UpsertIncidentAsync(incident)));
        }

        [FunctionName("Incident_Delete")]
        public async Task<IActionResult> DeleteAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "incidents/{id}/{workerId}")] HttpRequest req,
           ILogger log, string id, string workerId)
        {
            return new OkObjectResult(JsonConvert.SerializeObject(await _repository.DeleteIncidentAsync(id, workerId)));
        }
    }
}
