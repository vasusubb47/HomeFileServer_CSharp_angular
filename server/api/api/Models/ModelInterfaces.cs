namespace api.Models;

internal interface IModelTimeInfo
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}

internal interface IIsPublic
{
    bool IsPublic { get; set; }
}

internal interface IIsActive 
{
    bool IsActive { get; set; }
}
