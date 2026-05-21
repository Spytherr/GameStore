using System;

namespace GameStore.api;

public class Game
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public Genre? Genre { get; set; }
    public int GenreId { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsOnSale { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public DateOnly ReleaseDate { get; set; }
}
