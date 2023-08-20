using Microsoft.EntityFrameworkCore;

namespace Chinook.ClientModels
{
    [Keyless]
    public class ArtistModel
    {
        public long ArtistId { get; set; }
        public string? Name { get; set; }
        public ICollection<AlbumModel> Albums { get; set; }
    }
}
