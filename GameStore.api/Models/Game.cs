namespace GameStore.api;

public class Game
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public List<Genre> Genres { get; set; } = [];
    public List<Platform> Platforms { get; set; } = [];
    public string? ImageUrl { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public int? RawgId { get; set; }
    public double? Rating { get; set; }
    public List<GameOffer> Offers { get; set; } = [];
}
