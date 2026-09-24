using Api.Instartups.Auth.Common.Pagination;

namespace Api.Instartups.Auth.src.Interfaces;

public interface ICursorEncoder
{
    string Encode(CursorPayload payload);
    CursorPayload Decode(string cursor);
}
