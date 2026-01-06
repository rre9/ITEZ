using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = default!;
    
    public bool IsActive { get; set; } = true;
}

