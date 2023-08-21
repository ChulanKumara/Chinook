using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Chinook.Controller
{
    public class PlayListsController
    {
        private readonly ChinookContext _dbContext;
        public event EventHandler OnMenuUpdated;
        private readonly ILogger _logger;

        /// <summary>
        /// DB context class injected using DI - Constructor injection
        /// </summary>
        /// <param name="dbContext"></param>
        public PlayListsController(ChinookContext dbContext, ILogger<PlayListsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Get the Play lists by the currentUserId and the playListId
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="playListId"></param>
        /// <returns>Playlist client model is being returned here</returns>
        public ClientModels.Playlist GetUserPlayList(string currentUserId, long playListId)
        {
            return _dbContext.Playlists
            .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
            .Where(p => p.PlaylistId == playListId)
            .Select(p => new ClientModels.Playlist()
            {
                Name = p.Name,
                Tracks = p.Tracks.Select(t => new PlaylistTrack()
                {
                    AlbumTitle = t.Album.Title,
                    ArtistName = t.Album.Artist.Name,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == Constants.FavouriteListName)).Any()
                }).ToList()
            }).FirstOrDefault();
        }

        /// <summary>
        /// Method to add track to the play list. Same goes for favourites and other playlists.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="playListId"></param>
        /// <param name="trackId"></param>
        /// <param name="addToFavouriteTracks"></param>
        /// <param name="isFavourite"></param>
        public void AddToPlayList(string userId, long playListId, long trackId, bool addToFavouriteTracks = false, bool isFavourite = false, string playListName = "")
        {
            if (playListId == Constants.DefaultPlayListId)
            {
                var userList = _dbContext.UserPlaylists.Where(x => x.UserId == userId).Include(x => x.Playlist).OrderBy(x => x.PlaylistId).ToList();
                var playListByName = userList.FirstOrDefault(x => x.Playlist.Name == playListName);
                playListId = playListByName != null ? playListByName.PlaylistId : playListId;
            }

            if (!_dbContext.Playlists.Any(x => x.PlaylistId == playListId))
                playListId = _dbContext.Playlists.Select(x => x.PlaylistId).Max() + 1;

            var model = CreatePlayListModel(userId, playListId, trackId, addToFavouriteTracks, isFavourite, playListName);
            try
            {
                var existingUserPlayList = _dbContext.UserPlaylists.Local.FirstOrDefault(o => o.UserId == userId && o.Playlist.PlaylistId == playListId);
                if (existingUserPlayList != null)
                {
                    _dbContext.Entry(existingUserPlayList).State = EntityState.Detached;

                    var playList = _dbContext.Playlists.Local.FirstOrDefault(c => c.PlaylistId == playListId);
                    if (playList != null)
                    {
                        playList.Tracks = model.Playlist.Tracks;
                        model.Playlist = playList;
                    }
                    else
                        _dbContext.Attach(model.Playlist);

                    _dbContext.Update(model);
                    _dbContext.SaveChanges();
                }
                else
                {
                    _dbContext.Save.Add(model);
                    _dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

        }

        /// <summary>
        /// Creating the play list model to save
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="playListId"></param>
        /// <param name="trackId"></param>
        /// <param name="addToFavouriteTracks"></param>
        /// <param name="isFavourite"></param>
        /// <returns>Users</returns>
        private UserPlaylist CreatePlayListModel(string userId, long listId, long trackId, bool addToFavouriteTracks = false, bool isFavourite = false, string playListName = "")
        {
            var model = new UserPlaylist();
            if (trackId <= 0)
                return model;

            model.UserId = userId;
            model.Playlist = new Models.PlaylistData();
            model.PlaylistId = listId;
            model.Playlist.PlaylistId = listId;
            if (!playListName.IsNullOrEmpty())
                model.Playlist.Name = playListName;
            var track = _dbContext.Tracks.FirstOrDefault(x => x.TrackId == trackId);
            var trackList = new List<Track>();

            var existingList = _dbContext.UserPlaylists.Include(x => x.Playlist).Include(y => y.Playlist.Tracks).FirstOrDefault(x => x.PlaylistId == listId);
            if (existingList != null)
            {
                trackList = existingList.Playlist.Tracks.ToList();
            }
            if (addToFavouriteTracks)
            {
                if (playListName == Constants.FavouriteListName)
                {
                    model.Playlist.Name = Constants.FavouriteListName;
                    model.PlaylistId = listId;
                }
                else
                    model.Playlist.Name = existingList.Playlist.Name;

                if (isFavourite && existingList != null)
                {
                    existingList.Playlist.Tracks.Add(track);
                    trackList = existingList.Playlist.Tracks.ToList();
                }
                else if (!isFavourite && existingList != null)
                {
                    existingList.Playlist.Tracks.Remove(track);
                    trackList = existingList.Playlist.Tracks.ToList();
                }
                else
                {
                    trackList.Add(track);
                }
            }
            else
            {
                trackList.Add(track);

            }
            model.Playlist.Tracks = trackList;
            return model;
        }

        /// <summary>
        /// Get play list by logged in user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>UsersPlaylist client model is being returned here</returns>
        public List<UsersPlaylistModel> GetPlayListsByUser(string userId)
        {
            var list = _dbContext.UserPlaylists.Where(x => x.UserId == userId)
                .Include(x => x.Playlist)
                .ThenInclude(x => x.Tracks)
                .OrderBy(x => x.PlaylistId).Select(x => new UsersPlaylistModel()
                {
                    UserId = userId,
                    PlaylistId = x.PlaylistId,
                    User = x.User,
                    Playlist = x.Playlist
                }).ToList();

            list.Add(AddDefaultPlayList());

            return list.OrderBy(x => x.PlaylistId).ToList();
        }

        /// <summary>
        /// Add the default playlist
        /// </summary>
        /// <returns>UsersPlaylist client model is being returned here</returns>
        private UsersPlaylistModel AddDefaultPlayList()
        {
            var defaultPlayList = new UsersPlaylistModel();
            defaultPlayList.Playlist = new PlaylistData();
            defaultPlayList.PlaylistId = Constants.DefaultPlayListId;
            defaultPlayList.Playlist.Name = Constants.DefaultPlayListName;

            return defaultPlayList;
        }
    }
}
