using System.Security.Claims;
using AutoMapper;
using careerhive.application.DTOs.Request;
using careerhive.application.DTOs.Response;
using careerhive.application.Interfaces.IRepository;
using careerhive.domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace careerhive.api.Controllers;
[Route("api/jobs")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly IGenericRepository<Job> _genericRepository;
    private readonly IMapper _mapper;

    public JobsController(IGenericRepository<Job> genericRepository, IMapper mapper)
    {
        _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
        _mapper = mapper;
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, [FromQuery] bool includeUser = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid authentication token."
            });
        }

        IEnumerable<Job> jobs;

        if (includeUser)
        {
            jobs = await _genericRepository.GetPagedAsync(pageNumber, pageSize, j => j.CreatedAt, true, j => j.PostedBy);
        }
        else
        {
            jobs = await _genericRepository.GetPagedAsync(pageNumber, pageSize, j => j.CreatedAt, true);
        }

        var totalCount = await _genericRepository.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var jobResponseDtos = _mapper.Map<IEnumerable<JobResponseDto>>(jobs);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Data = jobResponseDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetJobById(string id, [FromQuery] bool includeUser = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid authentication token."
            });
        }

        if (!Guid.TryParse(id, out Guid jobId))
        {
            return BadRequest(new 
            {
                Success = true,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid job ID format." 
            });
        }

        Job? job;
        if (includeUser)
        {
            job = await _genericRepository.GetByIdAsync(jobId, j => j.PostedBy);
        }
        else
        {
            job = await _genericRepository.GetByIdAsync(jobId);
        }

        if (job == null)
        {
            return NotFound(new 
            {
                Success = true,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Job not found." 
            });
        }

        var jobResponseDto = _mapper.Map<JobResponseDto>(job);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Data = jobResponseDto
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddJob([FromBody] AddJobRequestDto addJobRequestDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid authentication token."
            });
        }

        var job = new Job
        {
            Title = addJobRequestDto.Title,
            Description = addJobRequestDto.Description,
            ExternalLink = addJobRequestDto.ExternalLink,
            PostedByUserId = Guid.Parse(userId)
        };

        await _genericRepository.AddAsync(job);
        return CreatedAtAction(nameof(GetJobById),
        new { id = job.Id },
        new
        {
            Success = true,
            StatusCode = StatusCodes.Status201Created,
            Message = "Job added successfully."
        });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateJob(string id, [FromBody] UpdateJobRequestDto updateJobRequestDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid authentication token."
            });
        }

        if (!Guid.TryParse(id, out Guid jobId))
        {
            return BadRequest(new
            {
                Success = true,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid job ID format."
            });
        }

        var job = await _genericRepository.GetByIdAsync(jobId);

        if (job == null)
        {
            return NotFound(new
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Job not found."
            });
        }

        job.Title = updateJobRequestDto.Title;
        job.Description = updateJobRequestDto.Description;
        job.ExternalLink = updateJobRequestDto.ExternalLink;
        job.UpdatedAt = DateTime.UtcNow;

        await _genericRepository.UpdateAsync(job);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Job updated successfully."
        });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteJob(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid authentication token."
            });
        }

        if (!Guid.TryParse(id, out Guid jobId))
        {
            return BadRequest(new
            {
                Success = true,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid job ID format."
            });
        }

        var job = await _genericRepository.GetByIdAsync(jobId);

        if (job == null)
        {
            return NotFound(new
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Job not found."
            });
        }

        await _genericRepository.RemoveAsync(job);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Job deleted successfully."
        });
    }
}
