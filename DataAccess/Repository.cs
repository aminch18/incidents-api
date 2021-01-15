using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Incidents_Api.DataAccess
{
    public interface IRepository
    {
        Task<IEnumerable<Incident>> GetIncidentsAsync();
        Task<Incident> GetIncidentByIdAsync(string id, string partitionKey);
        Task<bool> UpsertIncidentAsync(Incident incident);
        Task<bool> DeleteIncidentAsync(string id, string partitionKey);
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
                    {
                        incidents.Add(item);
                    }
                }
            }

            return incidents;
        }

        public async Task<Incident> GetIncidentByIdAsync(string id, string partitionKey)
        {
            var itemResponse = await _container.ReadItemAsync<Incident>(id, new PartitionKey(partitionKey));

            return (itemResponse.StatusCode == System.Net.HttpStatusCode.OK
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Created
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Accepted) ? itemResponse.Resource : null;

        }

        public async Task<bool> UpsertIncidentAsync(Incident incident)
        {
            var itemResponse = await _container.UpsertItemAsync(incident, new PartitionKey(incident.WorkerId));

            return itemResponse.StatusCode == System.Net.HttpStatusCode.OK
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Created
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Accepted;
        }

        public async Task<bool> DeleteIncidentAsync(string id, string partitionKey)
        {
            var itemResponse = await _container.DeleteItemAsync<Incident>(id, new PartitionKey(partitionKey));

            return itemResponse.StatusCode == System.Net.HttpStatusCode.OK
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Created
                 || itemResponse.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }
}
