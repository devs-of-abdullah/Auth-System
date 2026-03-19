using Business.Interfaces;
using DTO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthorizationService _authorizationService;

    public UsersController(IUserService userService, IAuthorizationService authorizationService)
    {
        _userService = userService;
        _authorizationService = authorizationService;
    }

    [Authorize(Roles = "admin")]
    [HttpGet(Name = "GetAllUsers")]
    public async Task<ActionResult<PaginatedResponse<ReadUserDTO>>> GetAll([FromQuery] PaginationFilterDTO filter)
    {
        var result = await _userService.GetPagedAsync(filter);
        return Ok(result);
    }




    [HttpGet("{id:int}", Name = "GetUserById")]
    
    [EnableRateLimiting("AuthLimiter")]

    public async Task<ActionResult<ReadUserDTO>> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid user ID.");

        var authResult = await _authorizationService.AuthorizeAsync(User, id, "UserOwnerOrAdmin");
        if (!authResult.Succeeded)
            return Forbid();

        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }


    [HttpPost(Name = "CreateUser")]
    [EnableRateLimiting("AuthLimiter")]

    public async Task<ActionResult<ReadUserDTO>> Create([FromBody] CreateUserDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdId = await _userService.CreateAsync(dto);
        if (createdId <= 0)
            return BadRequest("Error while creating user.");

        var createdUser = await _userService.GetByIdAsync(createdId);

        return CreatedAtRoute("GetUserById", new { id = createdId }, createdUser);
    }

  
    [Authorize]
    [HttpPut("change-password", Name = "ChangeUserPassword")]
    [EnableRateLimiting("AuthLimiter")]

    public async Task<IActionResult> ChangePassword([FromBody] UpdateUserPasswordDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _userService.UpdatePasswordAsync(userId, dto);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("self", Name = "SelfDelete")]
    public async Task<IActionResult> SelfDelete([FromBody] SoftUserDeleteDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _userService.SoftDeleteAsync(userId, dto);

        return NoContent();
    }


    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}", Name = "AdminDelete")]
    public async Task<IActionResult> AdminDelete(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid user ID.");

        await _userService.AdminSoftDeleteAsync(id);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me", Name = "GetMe")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<ActionResult<ReadUserDTO>> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }

    [Authorize]
    [HttpPut("update-email", Name = "UpdateUserEmail")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateUserEmailDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _userService.UpdateEmailAsync(userId, dto);

        return Ok(new { message = "Email updated successfully. Please verify your new email." });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/role", Name = "UpdateUserRole")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDTO dto)
    {
        if (!ModelState.IsValid || id <= 0)
            return BadRequest("Invalid request.");

        await _userService.UpdateRoleAsync(id, dto);

        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}/hard", Name = "HardDeleteUser")]
    public async Task<IActionResult> HardDelete(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid user ID.");

        await _userService.HardDeleteAsync(id);

        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/restore", Name = "RestoreUser")]
    public async Task<IActionResult> RestoreUser(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid user ID.");

        await _userService.RestoreUserAsync(id);

        return NoContent();
    }
}
