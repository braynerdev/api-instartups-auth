namespace Api.Instartups.Auth.DTOs;

public sealed record BaseResponseDTO<T>
{
    public bool IsSuccess  { get; init; }
    public DateTimeOffset ResponseAt { get; init; }
    public string Message { get; init; }
    public T? Data { get; init; }

    public static BaseResponseDTO<T> Success(T data, string message = "Ação executada com sucesso.")
    {
        return new BaseResponseDTO<T>
        {
            IsSuccess = true, ResponseAt = DateTimeOffset.UtcNow, Message = message, Data = data
        };
    }

    public static BaseResponseDTO<T> Error(T? data, string message = "Erro ao executar a ação.")
    {
        return new BaseResponseDTO<T>
        {
            IsSuccess = false, ResponseAt = DateTimeOffset.UtcNow, Message = message, Data = data
        };
    }
}