using Microsoft.EntityFrameworkCore;

namespace TunePhere.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Remix> Remixes { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }
        public DbSet<Lyric> Lyrics { get; set; }
        public DbSet<ListeningRoom> ListeningRooms { get; set; }
        public DbSet<ListeningRoomParticipant> ListeningRoomParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}