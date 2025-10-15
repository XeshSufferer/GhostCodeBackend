namespace GhostCodeBackend.Shared.DTO.Requests;

public class PromocodeCreationRequestDTO
{
    public string Code { get; set; }
    public string Type { get; set; }
    public int ActivatesCount { get; set; }
    public string BonusSize { get; set; }
}