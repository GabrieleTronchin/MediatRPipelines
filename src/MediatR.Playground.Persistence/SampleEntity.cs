namespace MediatR.Playground.Persistence;

public class SampleEntity
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;

    public DateTime RegistrationTime { get; set; }
}
