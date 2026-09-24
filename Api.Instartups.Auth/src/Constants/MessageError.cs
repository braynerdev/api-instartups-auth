namespace Api.Instartups.Auth.Constants;

public class MessageError
{
    public static string NotEmptyOrNull() 
        => "O campo é obrigatório.";
    
    public static string PasswordComplexity()
        => "O campo {PropertyName} deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um caractere especial.";
    
    public static string MinLength(int minLength) 
        => $"O valor precisa ter no mínimo {minLength} caracteres.";
    
    public static string MaxLength(int maxLength) 
        => $"O valor não pode exceder {maxLength} caracteres.";

    public static string Format()
        => "Formato inválido.";
}