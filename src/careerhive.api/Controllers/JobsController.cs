using System.Linq.Expressions;
using System.Security.Claims;
using AutoMapper;
using careerhive.application.DTOs.Response;
using careerhive.application.Interfaces.IRepository;
using careerhive.application.Interfaces.IService;
using careerhive.application.Request;
using careerhive.domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace careerhive.api.Controllers;
[Route("api/jobs")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly IGenericRepository<Job> _jobRepository;
    private readonly IGenericRepository<InvalidToken> _invalidTokenRepository;
    private readonly IGenericRepository<UserSubscription> _userSubscriptionRepository;
    private readonly IGenericRepository<SavedJob> _savedJobRepository;

    private readonly IEmailService _emailService;
    private readonly ISafeBrowsingService _safeBrowsingService;
    private readonly IMapper _mapper;

    public JobsController(IGenericRepository<Job> jobRepository, 
        IGenericRepository<InvalidToken> invalidTokenRepository, 
        IGenericRepository<UserSubscription> userSubscriptionRepository,
        IGenericRepository<SavedJob> savedJobRepository,
        IEmailService emailService,
        ISafeBrowsingService safeBrowsingService,
        IMapper mapper)
    {
        _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        _invalidTokenRepository = invalidTokenRepository;
        _userSubscriptionRepository = userSubscriptionRepository;
        _savedJobRepository = savedJobRepository;
        _emailService = emailService;
        _safeBrowsingService = safeBrowsingService;
        _mapper = mapper;
    }

    [HttpGet("all")]
    [Authorize]
    [EnableRateLimiting("get")]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, [FromQuery] bool includeUser = false, [FromQuery] string? userId = null)
    {
        if (pageNumber < 1)
        {
            return BadRequest(new
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Page number must be greater than or equal to 1."
            });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Page size must be between 1 and 100."
            });
        }

        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
        if (isTokenInvalid)
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        IEnumerable<Job> jobs;
        int totalCount;

        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out Guid userGuid))
        {
            if(userGuid.ToString() != currentUserId)
            {
                return Forbid();
            }

            jobs = await _jobRepository.GetPagedAsync(pageNumber, pageSize, j => j.CreatedAt, true, j => j.PostedByUserId == userGuid);
            totalCount = await _jobRepository.CountAsync(j => j.PostedByUserId == userGuid);
        }
        else
        {
            jobs = includeUser
                ? await _jobRepository.GetPagedAsync(pageNumber, pageSize, j => j.CreatedAt, true, null, j => j.PostedBy)
                : await _jobRepository.GetPagedAsync(pageNumber, pageSize, j => j.CreatedAt, true);

            totalCount = await _jobRepository.CountAsync();
        }

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

    [HttpGet("saved-jobs")]
    [Authorize]
    [EnableRateLimiting("get")]
    public async Task<IActionResult> SavedJobs(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1)
        {
            return BadRequest(new
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Page number must be greater than or equal to 1."
            });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Page size must be between 1 and 100."
            });
        }

        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
        if (isTokenInvalid)
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var savedJobs = await _savedJobRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            null,
            false,
            j => j.SavedByUserId == Guid.Parse(currentUserId),
            j => j.SavedJobDetails,
            j => j.SavedBy
        );

        var totalCount = await _savedJobRepository.CountAsync(j => j.SavedByUserId == Guid.Parse(currentUserId));
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var jobResponseDtos = _mapper.Map<IEnumerable<JobResponseDto>>(savedJobs);

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
    [EnableRateLimiting("get")]
    public async Task<IActionResult> GetJobById(string id, [FromQuery] bool includeUser = false)
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
        if (isTokenInvalid)
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
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
            job = await _jobRepository.GetByIdAsync(jobId, j => j.PostedBy);
        }
        else
        {
            job = await _jobRepository.GetByIdAsync(jobId);
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
    [EnableRateLimiting("post")]
    public async Task<IActionResult> AddJob([FromBody] AddJobRequestDto addJobRequestDto)
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
        if (isTokenInvalid)
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        bool isLinkSafe = await _safeBrowsingService.IsUrlSafeAsync(addJobRequestDto.ExternalLink!);

        if (!isLinkSafe)
        {
            return BadRequest(new
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "The provided link is not safe."
            });
        }

        var job = new Job
        {
            Title = addJobRequestDto.Title,
            Description = addJobRequestDto.Description,
            ExternalLink = addJobRequestDto.ExternalLink!,
            PostedByUserId = Guid.Parse(userId)
        };

        await _jobRepository.AddAsync(job);

        var subscribers = await _userSubscriptionRepository.FindAsync(s => s.IsActive && s.UserId.ToString() != userId, s => s.User);

        _ = Task.Run(async () =>
        {
            foreach (var subscriber in subscribers)
            {
                var subject = "New Job Posted: " + job.Title;
                var message = $@"<html><body>
                <p>Dear {subscriber.User.FullName},</p>
                <p>A new job has been posted that matches your subscription preferences.</p>
                <p><strong>Job Title:</strong> {job.Title}</p>
                <p><strong>Description:</strong> {job.Description}</p>
                <p><a href='{job.ExternalLink}' style='color: #007bff; text-decoration: none; font-weight: bold;'>Click here to apply</a></p>
                <p>If you did not request these notifications, please ignore this email.</p>
                <p>Regards,<br>Your Career Hive Team</p>
            </body></html>";

                await _emailService.SendEmailAsync(subscriber.Email, subject, message);
            }
        });

        return CreatedAtAction(nameof(GetJobById),
        new { id = job.Id },
        new
        {
            Success = true,
            StatusCode = StatusCodes.Status201Created,
            Message = "Job added successfully."
        });
    }

    [HttpPost("save")]
    [Authorize]
    public async Task<IActionResult> SaveJob([FromBody] Guid jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var jobExists = await _jobRepository.ExistsAsync(j => j.Id == jobId);
        if (!jobExists)
        {
            return NotFound(new
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Job not found."
            });
        }

        var alreadySaved = await _savedJobRepository.ExistsAsync(s => s.SavedByUserId == Guid.Parse(userId) && s.JobId == jobId);
        if (alreadySaved)
        {
            return Conflict(new
            {
                Success = false,
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Job is already saved."
            });
        }

        var savedJob = new SavedJob
        {
            JobId = jobId,
            SavedByUserId = Guid.Parse(userId),
        };

        await _savedJobRepository.AddAsync(savedJob);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Job saved successfully."
        });
    }

    [HttpDelete("unsave")]
    [Authorize]
    public async Task<IActionResult> UnsaveJob([FromQuery] Guid jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var savedJob = await _savedJobRepository.FirstOrDefaultAsync(s => s.SavedByUserId == Guid.Parse(userId) && s.JobId == jobId);
        if (savedJob == null)
        {
            return NotFound(new
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Job not found in saved list."
            });
        }

        await _savedJobRepository.RemoveAsync(savedJob);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Job removed from saved list."
        });
    }

    [HttpPut("{id}")]
    [Authorize]
    [EnableRateLimiting("put")]
    public async Task<IActionResult> UpdateJob(string id, [FromBody] UpdateJobRequestDto updateJobRequestDto)
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
        if (isTokenInvalid)
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
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

        bool isLinkSafe = await _safeBrowsingService.IsUrlSafeAsync(updateJobRequestDto.ExternalLink!);

        if (!isLinkSafe)
        {
            return BadRequest(new
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "The provided link is not safe."
            });
        }

        var job = await _jobRepository.GetByIdAsync(jobId);

        if (job == null)
        {
            return NotFound(new
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Job not found."
            });
        }

        if (job.PostedByUserId.ToString() != userId)
        {
            return Forbid();
        }

        job.Title = updateJobRequestDto.Title;
        job.Description = updateJobRequestDto.Description;
        job.ExternalLink = updateJobRequestDto.ExternalLink!;
        job.UpdatedAt = DateTime.UtcNow;

        await _jobRepository.UpdateAsync(job);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Job updated successfully."
        });
    }

    [HttpDelete("{id}")]
    [Authorize]
    [EnableRateLimiting("delete")]
    public async Task<IActionResult> DeleteJob(string id)
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
        if (isTokenInvalid)
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
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

        var job = await _jobRepository.GetByIdAsync(jobId);

        if (job == null)
        {
            return NotFound(new
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Job not found."
            });
        }

        if (job.PostedByUserId.ToString() != userId)
        {
            return Forbid();
        }

        await _jobRepository.RemoveAsync(job);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Job deleted successfully."
        });
    }

    [HttpGet("search")]
    [Authorize]
    [EnableRateLimiting("get")]
    public async Task<IActionResult> SearchJobs([FromQuery] string keyword)
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
        if (isTokenInvalid)
        {
            return Unauthorized(new
            {
                Success = false,
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid token."
            });
        }

        if (string.IsNullOrEmpty(keyword) || keyword.Length < 2)
        {
            return BadRequest(new
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Keyword must be at least 2 characters long."
            });
        }

        Expression<Func<Job, bool>> searchPredicate = j =>
            EF.Functions.Like(j.Title, $"%{keyword}%") ||
            EF.Functions.Like(j.Description, $"%{keyword}%") ||
            EF.Functions.Like(j.ExternalLink, $"%{keyword}%");

        int pageNumber = 1;
        int pageSize = 10;

        var jobs = await _jobRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            j => j.CreatedAt,
            true,
            searchPredicate,
            j => j.PostedBy
        );

        var jobResponseDtos = _mapper.Map<IEnumerable<JobResponseDto>>(jobs);

        return Ok(new
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Data = jobResponseDtos
        });
    }
}
