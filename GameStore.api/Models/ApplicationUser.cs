using Microsoft.AspNetCore.Identity;

namespace GameStore.api;

public class ApplicationUser : IdentityUser
{
    public required string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<GameOffer> GameOffers { get; set; } = [];
}
