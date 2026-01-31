using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiftServer.Data;
using GiftServer.Contracts;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonRepository personRepository;
        private readonly IUserRepository userRepository;
        private readonly ILogger<PersonController> logger;

        public PersonController(
            IPersonRepository personRepository,
            IUserRepository userRepository,
            ILogger<PersonController> logger)
        {
            this.personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/person/my?id={userId}
        [HttpGet("my")]
        public async Task<IActionResult> GetMyPeople([FromQuery] int id)
        {
            try
            {
                var people = await this.personRepository.GetMyPeople(id);
                return Ok(people);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting people for user {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving people.");
            }
        }

        // GET: api/person?id={id}
        [HttpGet]
        public async Task<IActionResult> GetPerson([FromQuery] int id)
        {
            try
            {
                var person = await this.personRepository.GetPerson(id);

                if (person == null)
                {
                    return NotFound();
                }

                return Ok(person);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting person {PersonId}", id);
                return StatusCode(500, "An error occurred while retrieving the person.");
            }
        }

        // POST: api/person
        [HttpPost]
        public async Task<IActionResult> AddNewPerson([FromBody] Person person)
        {
            if (person == null)
            {
                return BadRequest("Person data is required.");
            }

            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            
            if (userEmail == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            try
            {
                // Look up the authenticated user to get their ID
                var currentUser = await this.userRepository.GetUser(userEmail);
                
                if (currentUser == null)
                {
                    return Unauthorized("User not found: " + userEmail);
                }

                await this.personRepository.AddNewPerson(person.Name, person.Birthday ?? string.Empty, currentUser.Id);
                return Created(nameof(AddNewPerson), person);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error adding new person");
                return StatusCode(500, "An error occurred while adding the person.");
            }
        }

        // DELETE: api/person?id={id}
        [HttpDelete]
        public async Task<IActionResult> DeletePerson([FromQuery] int id)
        {
            try
            {
                await this.personRepository.DeletePerson(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error deleting person {PersonId}", id);
                return StatusCode(500, "An error occurred while deleting the person.");
            }
        }
    }
}
