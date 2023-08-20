using Chinook.ClientModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Chinook.Controller
{
    public class ArtistsController
    {
        private readonly ChinookContext _dbContext;
        private readonly ILogger _logger;


        /// <summary>
        /// DB context class injected using DI - Constructor injection
        /// </summary>
        /// <param name="dbContext"></param>
        public ArtistsController(ChinookContext dbContext, ILogger<ArtistsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Get All Artists based on searchTerm
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns>Artist client model is being returned here</returns>
        public async Task<List<ArtistModel>> GetArtists(string searchTerm = "")
        {
            try
            {
                var users = _dbContext.Users.Include(a => a.UserPlaylists).ToList();
                var artists = searchTerm.IsNullOrEmpty() ? _dbContext.Artists.Include(x => x.Albums).ToList() : _dbContext.Artists.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).Include(x => x.Albums).ToList();
                var artistModel = artists.Select(x => new ArtistModel
                {
                    ArtistId = x.ArtistId,
                    Name = x.Name,
                    Albums = (ICollection<AlbumModel>)x.Albums,
                }).ToList();

                return artistModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return new List<ArtistModel>();

        }

        /// <summary>
        /// Get Albums of an artist based on artistId
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns>Album client model is being returned here</returns>
        public async Task<List<AlbumModel>> GetAlbumsForArtist(int artistId)
        {
            try
            {
                return _dbContext.Albums.Where(a => a.ArtistId == artistId).Select(x => new AlbumModel()
                {
                    AlbumId = x.AlbumId,
                    Title = x.Title,
                    ArtistId = x.ArtistId,
                    Artist = x.Artist,
                    Tracks = x.Tracks
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new List<AlbumModel>();
        }
    }
}
