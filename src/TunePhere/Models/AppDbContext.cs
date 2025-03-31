using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TunePhere.Models  
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ListeningRoomParticipant> ListeningRoomParticipants { get; set; }
        public DbSet<ListeningRoom> ListeningRooms { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Lyric> Lyrics { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Remix> Remixes { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        public DbSet<Artists> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<UserFollower> UserFollowers { get; set; }
        public DbSet<ArtistFollower> ArtistFollowers { get; set; }

        public DbSet<UserFavoriteSong> UserFavoriteSongs { get; set; }

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
                .WithMany(n => n.Lyrics)
                .HasForeignKey(l => l.SongId)
                .OnDelete(DeleteBehavior.NoAction);

            // Playlists -> Users
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

            // ArtistSong relationships
            modelBuilder.Entity<Artists>()
                .HasMany(a => a.Songs)
                .WithOne(s => s.Artists)
                .HasForeignKey(s => s.ArtistId);

            // Cấu hình mối quan hệ một-nhiều giữa Album và Song
            modelBuilder.Entity<Song>()
                .HasOne(s => s.Albums)
                .WithMany(a => a.Songs)
                .HasForeignKey(s => s.AlbumId);

            // Cấu hình mối quan hệ Followers
            modelBuilder.Entity<UserFollower>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserFollower>()
                .HasOne(uf => uf.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowingId)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình mối quan hệ ArtistFollower
            modelBuilder.Entity<ArtistFollower>()
                .HasOne(af => af.User)
                .WithMany()
                .HasForeignKey(af => af.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ArtistFollower>()
                .HasOne(af => af.Artist)
                .WithMany(a => a.Followers)
                .HasForeignKey(af => af.ArtistId)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình mối quan hệ UserFavoriteSong
            modelBuilder.Entity<UserFavoriteSong>()
                .HasOne(ufs => ufs.User)
                .WithMany(u => u.FavoriteSongs)
                .HasForeignKey(ufs => ufs.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<UserFavoriteSong>()
                .HasOne(ufs => ufs.Song)
                .WithMany(s => s.FavoritedBy)
                .HasForeignKey(ufs => ufs.SongId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}