using System.ComponentModel.DataAnnotations;

namespace Chinook.Models
{
    public class PlaylistData
    {
        public PlaylistData()
        {
            Tracks = new HashSet<Track>();
        }

        [Key]
        public long PlaylistId { get; set; }
        public string? Name { get; set; }
        public virtual ICollection<Track> Tracks { get; set; }
        public virtual ICollection<UserPlaylist> UserPlaylists { get; set; }
    }
}
