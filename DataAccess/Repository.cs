using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incidents_Api.DataAccess
{
    public interface IRepository
    {
        Task<IEnumerable<Incident>> GetIncidentsAsync();
        Task<Incident> GetIncidentByIdAsync(string id);
        Task<object> UpsertIncidentAsync(Incident incident);
        Task<object> DeleteIncidentAsync(string id, string partitionKey);
    }

    public class Repository : IRepository
    {
        private readonly Container _container;
        private readonly string _databaseName;
        private readonly string _containerName;

        public Repository(CosmosClient dbClient, string databaseName, string containerName)
        {
            _databaseName = (!string.IsNullOrEmpty(databaseName)) ? databaseName : throw new ArgumentNullException(nameof(databaseName));
            _containerName = (!string.IsNullOrEmpty(containerName)) ? containerName : throw new ArgumentNullException(nameof(containerName));
            _container = dbClient.GetContainer(_databaseName, _containerName);
        }

        public async Task<IEnumerable<Incident>> GetIncidentsAsync()
        {
            var feedIterator = _container.GetItemQueryIterator<Incident>();
            var incidents = new List<Incident>();

            while (feedIterator.HasMoreResults)
            {
                foreach (var item in await feedIterator.ReadNextAsync())
                {
                    incidents.Add(item);
                }
            }

            return incidents;
        }

        public async Task<Incident> GetIncidentByIdAsync(string id)
        {
            var query = "SELECT * FROM c WHERE c.id = @id ";
            QueryDefinition queryDefinition = new QueryDefinition(query).WithParameter("@id", id);

            var readResult = await (_container.GetItemQueryIterator<Incident>(queryDefinition)).ReadNextAsync();

            return readResult.AsEnumerable().FirstOrDefault();
        }

        public async Task<object> UpsertIncidentAsync(Incident incident)
        {
            incident.CreatedDateTime = DateTime.UtcNow;
            var itemResponse = await _container.UpsertItemAsync(incident, new PartitionKey(incident.WorkerId));

            var isSuccessAction =  itemResponse.StatusCode == System.Net.HttpStatusCode.OK
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Created
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Accepted;

            return new { IsSuccess = isSuccessAction, Incidence = incident, StatusCode = System.Net.HttpStatusCode.Created };
        }

        public async Task<object> DeleteIncidentAsync(string id, string partitionKey)
        {
            var itemResponse = await _container.DeleteItemAsync<Incident>(id, new PartitionKey(partitionKey));

            var isSuccessAction = itemResponse.StatusCode == System.Net.HttpStatusCode.OK
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Created
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Accepted
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.NoContent;

            return new { IsSuccess = isSuccessAction, DeletedIncidenceId = id, StatusCode = System.Net.HttpStatusCode.Accepted };
        }
    }
}
