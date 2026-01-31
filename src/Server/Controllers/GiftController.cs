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
    public class GiftController : ControllerBase
    {
        private readonly IGiftRepository giftRepository;
        private readonly ILogger<GiftController> logger;

        public GiftController(
            IGiftRepository giftRepository,
            ILogger<GiftController> logger)
        {
            this.giftRepository = giftRepository ?? throw new ArgumentNullException(nameof(giftRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/gift/person/{personId}
        [HttpGet("person/{personId}")]
        public async Task<IActionResult> GetGiftIdeasForPersonAsync(int personId)
        {
            try
            {
                var gifts = await this.giftRepository.GetGiftIdeasForPerson(personId);
                return Ok(gifts);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting gifts for person {PersonId}", personId);
                return StatusCode(500, "An error occurred while retrieving gifts.");
            }
        }

        // GET: api/gift/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGiftAsync(int id)
        {
            try
            {
                var gift = await this.giftRepository.GetGift(id);

                if (gift == null)
                {
                    return NotFound();
                }

                return Ok(gift);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting gift {GiftId}", id);
                return StatusCode(500, "An error occurred while retrieving the gift.");
            }
        }

        // POST: api/gift
        [HttpPost]
        public async Task<IActionResult> AddNewGiftAsync([FromBody] Gift gift)
        {
            if (gift == null)
            {
                return BadRequest("Gift data is required.");
            }

            if (string.IsNullOrWhiteSpace(gift.Name))
            {
                return BadRequest("Gift name is required.");
            }

            try
            {
                // Note: Assuming PersonName in the Gift record contains the PersonId
                // You may need to adjust this based on your actual data model
                var newGift = await this.giftRepository.AddNewGift(
                    gift.Name,
                    gift.PersonId,
                    gift.Link ?? string.Empty, 
                    gift.Date ?? string.Empty);
                    
                return Created(nameof(AddNewGiftAsync), newGift);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error adding new gift");
                return StatusCode(500, "An error occurred while adding the gift.");
            }
        }

        // PUT: api/gift/{id}/purchased
        [HttpPut("{id}/purchased")]
        public async Task<IActionResult> MarkAsPurchasedAsync(int id)
        {
            try
            {
                await this.giftRepository.SetIsPurchased(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error marking gift {GiftId} as purchased", id);
                return StatusCode(500, "An error occurred while updating the gift.");
            }
        }

        // DELETE: api/gift/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGiftAsync(int id)
        {
            try
            {
                await this.giftRepository.DeleteGift(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error deleting gift {GiftId}", id);
                return StatusCode(500, "An error occurred while deleting the gift.");
            }
        }
    }
}
