using System.Security.Cryptography;
using api.Models;
using api.Repositories.BucketRepo;
using api.Repositories.FileRepo;
using api.Services.UserContextService;
using api.UtilityClass.databaseErrors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
public class BucketController(
    IBucketRepository bucketRepo,
    IFileRepository fileRepo,
    ILogger<BucketController> logger,
    IUserContext userContext
) : ControllerBase
{
    
    [HttpPost]
    public async Task<ActionResult<InsertBucketInfo>> CreateBucket([FromBody] InsertBucketInfo bucket)
    {
        logger.LogInformation("Creating Bucket {BucketName}", bucket.BucketName);
        
        var result = await bucketRepo.InsertBucket(InsertUserBucketInfo.MapFrom(bucket, userContext.User!.UserId));
        if (!result.IsSuccess)
        {
            if (result.Error?.Code == PostgresCodes.UniqueViolation)
            {
                return Conflict(new { message = result.Error });
            }
        
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(CreateBucket), new { id = result.Value!.UserId }, result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<List<Bucket>>> GetAllBuckets()
    {
        var result = await bucketRepo.GetAllBucketsForUser(userContext.User!.UserId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<bool>> IsBucketOwner([FromQuery] Guid bucketId)
    {
        var result = await bucketRepo.IsBucketOwner(bucketId, userContext.User!.UserId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> UploadFile([FromForm] IFormFile file, [FromForm] Guid bucketId)
    {

        // 2. Get File Hash (SHA256)
        // string fileHash;
        // using (var sha256 = SHA256.Create())
        // {
        //     using var stream = file.OpenReadStream();
        //     byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        //     fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        // }

        // // Now you can save these to your database (Buckets table, etc.)
        // return Ok(new {
        //     FileName = fileName,
        //     Size = fileSize,
        //     Type = mimeType,
        //     Hash = fileHash
        // });

        var fileId = await fileRepo.InsertFile(new UserBucketId
        {
            UserId = userContext.User!.UserId,
            BucketId = bucketId
        }, file);
        
        return Ok(fileId);

    }
}
