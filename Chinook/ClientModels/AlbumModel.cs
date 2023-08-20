using Chinook.Models;

namespace Chinook.ClientModels
{
    public class AlbumModel
    {
        public long AlbumId { get; set; }
        public string Title { get; set; } = null!;
        public long ArtistId { get; set; }
        public ArtistModel Artist { get; set; } = null!;
        public ICollection<Track> Tracks { get; set; }
    }
}
