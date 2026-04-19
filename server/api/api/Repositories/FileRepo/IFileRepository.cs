using api.Models;

namespace api.Repositories.FileRepo;

public interface IFileRepository
{
    public Task<Guid> InsertFile(UserBucketId userBucketId, IFormFile file);
}
