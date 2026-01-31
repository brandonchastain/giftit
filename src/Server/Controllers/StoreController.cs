using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiftServer.Data;
using GiftServer.Contracts;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreRepository storeRepository;
        private readonly ILogger<StoreController> logger;

        public StoreController(
            IStoreRepository storeRepository,
            ILogger<StoreController> logger)
        {
            this.storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/store/person/{personId}
        [HttpGet("person/{personId}")]
        public async Task<IActionResult> GetStoresForPersonAsync(int personId)
        {
            try
            {
                var stores = await this.storeRepository.GetStoresForPerson(personId);
                return Ok(stores);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting stores for person {PersonId}", personId);
                return StatusCode(500, "An error occurred while retrieving stores.");
            }
        }

        // POST: api/store
        [HttpPost]
        public async Task<IActionResult> AddStoreAsync([FromBody] Store store)
        {
            if (store == null)
            {
                return BadRequest("Store data is required.");
            }

            if (string.IsNullOrWhiteSpace(store.Name))
            {
                return BadRequest("Store name is required.");
            }

            try
            {
                await this.storeRepository.AddStoreAsync(store.PersonId, store.Name, store.Url ?? string.Empty);
                return Created(nameof(AddStoreAsync), store);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error adding new store");
                return StatusCode(500, "An error occurred while adding the store.");
            }
        }

        // DELETE: api/store/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStoreAsync(int id)
        {
            try
            {
                await this.storeRepository.DeleteStoreAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error deleting store {StoreId}", id);
                return StatusCode(500, "An error occurred while deleting the store.");
            }
        }
    }
}
