namespace Domain.Exceptions;

public class VersionConflictException : Exception
{
    public VersionConflictException(string entityName, int id, int expectedVersion, int actualVersion)
        : base($"{entityName} with Id {id} was modified by another user. Expected version {expectedVersion}, but current version is {actualVersion}.")
    {
        EntityName = entityName;
        Id = id;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public string EntityName { get; }
    public int Id { get; }
    public int ExpectedVersion { get; }
    public int ActualVersion { get; }
}
