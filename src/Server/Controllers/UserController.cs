using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GiftServer.Data;
using GiftServer.Contracts;
using System.Threading.Tasks;
using System.Security.Claims;
using GiftServer.Config;


namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);
        private readonly ILogger<UserController> logger;
        private readonly GiftServerConfig config;

        public UserController(
            IUserRepository userRepository,
            ILogger<UserController> logger,
            GiftServerConfig config)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // GET: api/user?email={email}
        [HttpGet]
        public async Task<IActionResult> GetUserAsync([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required.");
            }

            try
            {
                var user = await this.userRepository.GetUser(email);
                logger.LogError("user info retrieved: {@User}", user);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting user by email {Email}", email);
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        // POST: api/user
        [HttpPost]
        public async Task<IActionResult> AddNewUserAsync([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User data is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Name))
            {
                return BadRequest("Email and Name are required.");
            }

            await locker.WaitAsync();
            try
            {
                var existingUser = await this.userRepository.GetUser(user.Email);
                if (existingUser != null)
                {
                    return Conflict("User already exists.");
                }

                await this.userRepository.AddNewUser(user.Email, user.Name);
                
                // Retrieve the newly created user to return it with the generated ID
                var newUser = await this.userRepository.GetUser(user.Email);
                return Created(nameof(AddNewUserAsync), newUser);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error adding new user");
                return StatusCode(500, "An error occurred while adding the user.");
            }
            finally
            {
                locker.Release();
            }
        }

        // GET: api/user/current - Returns the currently authenticated user
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            
            if (userEmail == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            try
            {
                var user = await this.userRepository.GetUser(userEmail);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting current user");
                return StatusCode(500, "An error occurred while retrieving the current user.");
            }
        }
    }
}
