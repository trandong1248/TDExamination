using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
    }
}
