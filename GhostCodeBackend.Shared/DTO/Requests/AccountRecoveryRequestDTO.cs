namespace GhostCodeBackend.Shared.DTO.Requests;

public class AccountRecoveryRequestDTO
{
    public string Login { get; set; }
    public string RecoveryCode { get; set; }
    public string NewPassword { get; set; }
}