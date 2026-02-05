using System.ComponentModel.DataAnnotations;

public class FcmTokenUpdateRequest
{
    [Required]
    public string FcmToken { get; set; }
}
