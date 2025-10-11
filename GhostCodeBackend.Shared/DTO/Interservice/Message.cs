namespace GhostCodeBackend.Shared.DTO.Interservice;

public class Message<T>
{
    public string CorrelationId { get; set; }
    public T Data { get; set; }
    public bool IsSuccess { get; set; } = true;
}