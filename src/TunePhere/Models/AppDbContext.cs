using Microsoft.EntityFrameworkCore;

namespace TunePhere.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ListeningRoomParticipant> ListeningRoomParticipants { get; set; }
        public DbSet<ListeningRoom> ListeningRooms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Lyric> Lyrics { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Remix> Remixes { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ListeningRoomParticipant relationships
            modelBuilder.Entity<ListeningRoomParticipant>()
                .HasOne(p => p.Room)
                .WithMany(r => r.Participants)
                .HasForeignKey(p => p.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ListeningRoomParticipant>()
                .HasOne(p => p.User)
                .WithMany(u => u.ListeningRoomParticipants)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ListeningRoom relationships
            modelBuilder.Entity<ListeningRoom>()
                .HasOne(r => r.Creator)
                .WithMany(u => u.ListeningRooms)
                .HasForeignKey(r => r.CreatorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ListeningRoom>()
                .HasOne(r => r.CurrentSong)
                .WithMany()
                .HasForeignKey(r => r.CurrentSongId)
                .OnDelete(DeleteBehavior.NoAction);

            // Song-related relationships
            modelBuilder.Entity<Lyric>()
                .HasOne(l => l.Song)
                .WithMany(s => s.Lyrics)
                .HasForeignKey(l => l.SongId)
                .OnDelete(DeleteBehavior.NoAction);

            // User-related relationships
            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Remix>()
                .HasOne(r => r.User)
                .WithMany(u => u.Remixes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Remix>()
                .HasOne(r => r.OriginalSong)
                .WithMany(s => s.Remixes)
                .HasForeignKey(r => r.OriginalSongId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserPreference>()
                .HasOne(up => up.User)
                .WithOne(u => u.Preferences)
                .HasForeignKey<UserPreference>(up => up.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // PlaylistSong relationships
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistSongs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistSongs)
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.AddedByUser)
                .WithMany()
                .HasForeignKey(ps => ps.AddedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}