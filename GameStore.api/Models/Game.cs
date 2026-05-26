namespace GameStore.api;

public class Game
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public Genre? Genre { get; set; }
    public int GenreId { get; set; }
    public string? ImageUrl { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public int? RawgId { get; set; }
    public double? Rating { get; set; }
    public List<GameOffer> Offers { get; set; } = [];
}
