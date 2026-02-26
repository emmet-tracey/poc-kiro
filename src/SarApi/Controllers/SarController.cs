using Microsoft.AspNetCore.Mvc;
using SarApi.Models;
using SarApi.Services;
using FluentValidation;

namespace SarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SarController : ControllerBase
{
    private readonly ISarService _sarService;
    private readonly IValidator<CreateSarRequest> _createValidator;
    private readonly IValidator<UpdateSarRequest> _updateValidator;
    private readonly ILogger<SarController> _logger;

    public SarController(
        ISarService sarService,
        IValidator<CreateSarRequest> createValidator,
        IValidator<UpdateSarRequest> updateValidator,
        ILogger<SarController> logger)
    {
        _sarService = sarService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new Suspicious Activity Report
    /// </summary>
    /// <param name="request">SAR creation request</param>
    /// <returns>Created SAR</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SuspiciousActivityReport>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<SuspiciousActivityReport>>> CreateSar([FromBody] CreateSarRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new SAR");

            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var sar = await _sarService.CreateSarAsync(request);

            return CreatedAtAction(
                nameof(GetSar),
                new { id = sar.Id },
                new ApiResponse<SuspiciousActivityReport>
                {
                    Success = true,
                    Data = sar,
                    Message = "SAR created successfully"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SAR");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while creating SAR"
            });
        }
    }

    /// <summary>
    /// Retrieves a SAR by ID
    /// </summary>
    /// <param name="id">SAR ID</param>
    /// <returns>SAR details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SuspiciousActivityReport>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<SuspiciousActivityReport>>> GetSar(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving SAR with ID: {SarId}", id);

            var sar = await _sarService.GetSarByIdAsync(id);
            if (sar == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"SAR with ID {id} not found"
                });
            }

            return Ok(new ApiResponse<SuspiciousActivityReport>
            {
                Success = true,
                Data = sar
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SAR with ID: {SarId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while retrieving SAR"
            });
        }
    }

    /// <summary>
    /// Lists SARs with optional filtering
    /// </summary>
    /// <param name="status">Filter by status</param>
    /// <param name="createdAfter">Filter by creation date (after)</param>
    /// <param name="createdBefore">Filter by creation date (before)</param>
    /// <param name="customerName">Filter by customer name</param>
    /// <param name="accountNumber">Filter by account number</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="nextToken">Pagination token</param>
    /// <returns>List of SARs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SarListResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<SarListResponse>>> GetSars(
        [FromQuery] SarStatus? status = null,
        [FromQuery] DateTime? createdAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] string? customerName = null,
        [FromQuery] string? accountNumber = null,
        [FromQuery] int limit = 50,
        [FromQuery] string? nextToken = null)
    {
        try
        {
            _logger.LogInformation("Retrieving SARs with filters");

            var parameters = new SarQueryParameters
            {
                Status = status,
                CreatedAfter = createdAfter,
                CreatedBefore = createdBefore,
                CustomerName = customerName,
                AccountNumber = accountNumber,
                Limit = Math.Min(limit, 100), // Cap at 100
                NextToken = nextToken
            };

            var result = await _sarService.GetSarsAsync(parameters);

            return Ok(new ApiResponse<SarListResponse>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SARs");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while retrieving SARs"
            });
        }
    }

    /// <summary>
    /// Updates an existing SAR
    /// </summary>
    /// <param name="id">SAR ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated SAR</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SuspiciousActivityReport>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<SuspiciousActivityReport>>> UpdateSar(string id, [FromBody] UpdateSarRequest request)
    {
        try
        {
            _logger.LogInformation("Updating SAR with ID: {SarId}", id);

            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var sar = await _sarService.UpdateSarAsync(id, request);
            if (sar == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"SAR with ID {id} not found"
                });
            }

            return Ok(new ApiResponse<SuspiciousActivityReport>
            {
                Success = true,
                Data = sar,
                Message = "SAR updated successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SAR with ID: {SarId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while updating SAR"
            });
        }
    }

    /// <summary>
    /// Deletes a SAR
    /// </summary>
    /// <param name="id">SAR ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSar(string id)
    {
        try
        {
            _logger.LogInformation("Deleting SAR with ID: {SarId}", id);

            var deleted = await _sarService.DeleteSarAsync(id);
            if (!deleted)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"SAR with ID {id} not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "SAR deleted successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SAR with ID: {SarId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while deleting SAR"
            });
        }
    }

    /// <summary>
    /// Submits a SAR for regulatory review
    /// </summary>
    /// <param name="id">SAR ID</param>
    /// <returns>Updated SAR</returns>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(ApiResponse<SuspiciousActivityReport>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<SuspiciousActivityReport>>> SubmitSar(string id)
    {
        try
        {
            _logger.LogInformation("Submitting SAR with ID: {SarId}", id);

            var sar = await _sarService.SubmitSarAsync(id);
            if (sar == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"SAR with ID {id} not found"
                });
            }

            return Ok(new ApiResponse<SuspiciousActivityReport>
            {
                Success = true,
                Data = sar,
                Message = "SAR submitted successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting SAR with ID: {SarId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while submitting SAR"
            });
        }
    }

    /// <summary>
    /// Files a SAR with regulatory authorities
    /// </summary>
    /// <param name="id">SAR ID</param>
    /// <param name="filingReference">Filing reference number</param>
    /// <returns>Updated SAR</returns>
    [HttpPost("{id}/file")]
    [ProducesResponseType(typeof(ApiResponse<SuspiciousActivityReport>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<SuspiciousActivityReport>>> FileSar(string id, [FromBody] string filingReference)
    {
        try
        {
            _logger.LogInformation("Filing SAR with ID: {SarId}", id);

            if (string.IsNullOrWhiteSpace(filingReference))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Filing reference is required"
                });
            }

            var sar = await _sarService.FileSarAsync(id, filingReference);
            if (sar == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"SAR with ID {id} not found"
                });
            }

            return Ok(new ApiResponse<SuspiciousActivityReport>
            {
                Success = true,
                Data = sar,
                Message = "SAR filed successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filing SAR with ID: {SarId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while filing SAR"
            });
        }
    }
}