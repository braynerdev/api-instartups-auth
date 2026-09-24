using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text.Json;
using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Options;
using Api.Instartups.Auth.src.Interfaces;
using Microsoft.Extensions.Options;

namespace Api.Instartups.Auth.Common.Pagination;

public class CursorEncoder(IOptions<CursorOptions> options) : ICursorEncoder
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key = Convert.FromBase64String(options.Value.SecretKey);

    public string Encode(CursorPayload payload)
    {
        var plaintext = JsonSerializer.SerializeToUtf8Bytes(payload);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var ciphertext = new byte[plaintext.Length];

        using (var aesGcm = new AesGcm(_key, TagSize))
        {
            aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        var encrypted = new byte[NonceSize + TagSize + ciphertext.Length];
        nonce.CopyTo(encrypted, 0);
        tag.CopyTo(encrypted, NonceSize);
        ciphertext.CopyTo(encrypted, NonceSize + TagSize);

        return Base64Url.EncodeToString(encrypted);
    }

    public CursorPayload Decode(string cursor)
    {
        try
        {
            var encrypted = Base64Url.DecodeFromChars(cursor);

            var nonce = encrypted[..NonceSize];
            var tag = encrypted[NonceSize..(NonceSize + TagSize)];
            var ciphertext = encrypted[(NonceSize + TagSize)..];

            var plaintext = new byte[ciphertext.Length];

            using (var aesGcm = new AesGcm(_key, TagSize))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
            }

            return JsonSerializer.Deserialize<CursorPayload>(plaintext)
                ?? throw new InvalidCursorException();
        }
        catch (Exception ex) when (ex is not InvalidCursorException)
        {
            throw new InvalidCursorException();
        }
    }
}
