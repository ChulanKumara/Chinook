using Chinook.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chinook.ClientModels
{
    [Keyless]
    public class AlbumModel
    {
        public long AlbumId { get; set; }
        public string Title { get; set; } = null!;
        public long ArtistId { get; set; }
        [NotMapped]
        public ArtistModel Artist { get; set; } = null!;
        public ICollection<Track> Tracks { get; set; }
    }
}
