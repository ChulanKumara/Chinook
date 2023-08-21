using Chinook.ClientModels;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Controller
{
    public class TracksController
    {
        private readonly ChinookContext _dbContext;
        private readonly ILogger _logger;

        /// <summary>
        /// DB context class injected using DI - Constructor injection
        /// </summary>
        /// <param name="dbContext"></param>
        public TracksController(ChinookContext dbContext, ILogger<TracksController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Get tracks based on ArtistId and CurrentUserId
        /// </summary>
        /// <param name="ArtistId"></param>
        /// <param name="CurrentUserId"></param>
        /// <returns>PlaylistTrack client model is being returned here</returns>
        public List<PlaylistTrack> GetTracks(long ArtistId, string CurrentUserId)
        {
            try
            {
                var tracks = _dbContext.Tracks;
                return tracks.Where(a => a.Album.ArtistId == ArtistId)
                .Include(a => a.Album)
                .Select(t => new PlaylistTrack()
                {
                    AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUserId && up.Playlist.Name == Constants.FavouriteListName)).Any()
                })
                .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new List<PlaylistTrack>();

        }
    }
}
