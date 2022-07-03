using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class User:IdentityUser
    {
        [System.Text.Json.Serialization.JsonIgnore]
        virtual public ICollection<EntryTag>? Tags { get; set; }

    }
}
