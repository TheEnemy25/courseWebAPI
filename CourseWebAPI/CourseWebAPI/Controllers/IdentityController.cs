using CourseWebAPI.Dto.Auth;
using CourseWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourseWebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService identityService;

        public IdentityController(IIdentityService identityService)
        {
            this.identityService = identityService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterStudent(RegistrationDto registrationDto)
        {
            var (isSuccessful, authResult) = await identityService.RegisterAsync(registrationDto);
            return isSuccessful ? Ok(authResult) : BadRequest();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var (isSuccessful, authResult) = await identityService.LoginAsync(loginDto);
            return isSuccessful ? Ok(authResult) : BadRequest();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var (isSuccessful, authResult) = await identityService.RefreshAsync(refreshTokenDto);
            return isSuccessful ? Ok(authResult) : Forbid();
        }
    }
}