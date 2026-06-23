namespace PaymentsService.Application.Transfers;

public class TransferResult
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }

    private TransferResult(bool success, string? error)
    {
        Success = success;
        Error = error;
    }

    public static TransferResult Ok() => new(true, null);
    public static TransferResult Fail(string error) => new(false, error);
}
