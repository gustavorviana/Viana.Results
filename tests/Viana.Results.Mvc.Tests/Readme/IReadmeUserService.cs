namespace Viana.Results.Mvc.Tests.Readme;

/// <summary>README snippet service.</summary>
public interface IReadmeUserService
{
    Task<Result<ReadmeUser>> GetUserByIdAsync(int id);

    Task<Result<ReadmeUser>> CreateAsync(ReadmeCreateUserDto dto);

    Task<ListResult<ReadmeUser>> GetAllAsync();
}
