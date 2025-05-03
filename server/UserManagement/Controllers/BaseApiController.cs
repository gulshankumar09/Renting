using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Controllers;

/// <summary>
/// Base API controller that provides common functionality and can be used for API versioning
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // Common controller functionality can be added here
    // When versioning is implemented, version-specific attributes and routing can be added
}