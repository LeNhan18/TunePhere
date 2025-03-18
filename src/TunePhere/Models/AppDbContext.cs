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
            base.OnModelCreating(modelBuilder);

            // Cấu hình ListeningRoomParticipant
            modelBuilder.Entity<ListeningRoomParticipant>(entity =>
            {
                entity.HasOne(lrp => lrp.Room)
                    .WithMany(r => r.Participants)
                    .HasForeignKey(lrp => lrp.RoomId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(lrp => lrp.User)
                    .WithMany(u => u.ListeningRoomParticipants)
                    .HasForeignKey(lrp => lrp.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Cấu hình ListeningRoom
            modelBuilder.Entity<ListeningRoom>(entity =>
            {
                entity.HasOne(r => r.Creator)
                    .WithMany(u => u.ListeningRooms)
                    .HasForeignKey(r => r.CreatorId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(r => r.CurrentSong)
                    .WithMany()
                    .HasForeignKey(r => r.CurrentSongId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Lyrics -> Songs
            modelBuilder.Entity<Lyric>()
                .HasOne(l => l.Song)
                .WithMany()
                .HasForeignKey(l => l.SongId)
                .OnDelete(DeleteBehavior.NoAction);

            // Playlists -> Users
            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Remixes -> Users
            modelBuilder.Entity<Remix>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Remixes -> Songs
            modelBuilder.Entity<Remix>()
                .HasOne(r => r.OriginalSong)
                .WithMany()
                .HasForeignKey(r => r.OriginalSongId)
                .OnDelete(DeleteBehavior.NoAction);

            // UserPreferences -> Users
            modelBuilder.Entity<UserPreference>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserPreference>(up => up.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // PlaylistSong relationships
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany()
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Song)
                .WithMany()
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