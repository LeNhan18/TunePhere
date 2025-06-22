using Microsoft.AspNetCore.Mvc;
using TunePhere.Services;

namespace TunePhere.Controllers
{
    public class FirebaseAuthController : Controller
    {
        private readonly FirebaseAuthService _firebaseAuthService;

        public FirebaseAuthController(FirebaseAuthService firebaseAuthService)
        {
            _firebaseAuthService = firebaseAuthService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _firebaseAuthService.AuthenticateFirebaseUserAsync(request.IdToken);
                return Ok(new { success = true, user = user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string IdToken { get; set; }
    }
} 