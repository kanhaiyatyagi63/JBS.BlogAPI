using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JBS.Utility.CustomAttributes;
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;
    private readonly string? _message;
    public AllowedExtensionsAttribute(string[] extensions, string? message)
    {
        _extensions = extensions;
        _message = message;
    }

    protected override ValidationResult IsValid(
    object value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!_extensions.Contains(extension.ToLower()))
            {
                return new ValidationResult(GetErrorMessage());
            }
        }

        return ValidationResult.Success;
    }

    public string GetErrorMessage()
    {
        return _message ?? $"This extension is not allowed!";
    }
}