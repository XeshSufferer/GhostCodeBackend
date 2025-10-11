namespace GhostCodeBackend.Shared.DTO.Interservice;

public class Message<T>
{
    public string CorrelationId { get; set; }
    public T Data { get; set; }
}