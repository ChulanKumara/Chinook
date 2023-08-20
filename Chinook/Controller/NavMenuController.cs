using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Controller
{
    public class NavMenuController
    {
        private readonly ChinookContext _dbContext;
        public event EventHandler OnMenuUpdated;
        private readonly ILogger _logger;


        /// <summary>
        /// DB context class injected using DI - Constructor injection
        /// </summary>
        /// <param name="dbContext"></param>
        public NavMenuController(ChinookContext dbContext, ILogger<NavMenuController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }


        /// <summary>
        /// Refresh Nav menu without full page reload
        /// </summary>
        public void GetUpdatedMenuByUser(string userId)
        {
            try
            {
                var list = _dbContext.UserPlaylists.Where(x => x.UserId == userId)
                                .Include(x => x.Playlist)
                                .ThenInclude(x => x.Tracks)
                                .OrderBy(x => x.PlaylistId)
                                .ToList();

                list.Add(AddDefaultPlayList());
                list.OrderBy(x => x.PlaylistId).ToList();

                OnMenuUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

        }

        /// <summary>
        /// Add the default playlist
        /// </summary>
        /// <returns>UsersPlaylist client model is being returned here</returns>
        private UserPlaylist AddDefaultPlayList()
        {
            var defaultPlayList = new UserPlaylist();
            defaultPlayList.Playlist = new PlaylistData();
            defaultPlayList.PlaylistId = Constants.DefaultPlayListId;
            defaultPlayList.Playlist.Name = Constants.DefaultPlayListName;

            return defaultPlayList;
        }
    }
}
