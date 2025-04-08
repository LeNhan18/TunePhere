using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class AppUser : IdentityUser
    {

        public required string FullName { get; set; } // Họ và tên đầy đủ

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo tài khoản

        public DateTime? LastLogin { get; set; } // Lần đăng nhập gần nhất

        public string? ImageUrl { get; set; } // Đường dẫn ảnh đại diện

        public string? CoverImage { get; set; } // Đường dẫn ảnh nền

        // Các thuộc tính điều hướng (Navigation properties)
        public virtual ICollection<ListeningRoom> ListeningRooms { get; set; } // Danh sách phòng do người dùng tạo
        public virtual ICollection<ListeningRoomParticipant> ListeningRoomParticipants { get; set; } // Danh sách phòng đã tham gia
        public virtual ICollection<Playlist> Playlists { get; set; } // Danh sách playlist của người dùng
        public virtual ICollection<Remix> Remixes { get; set; } // Danh sách remix của người dùng
        public virtual UserPreference? Preferences { get; set; } // Tùy chọn người dùng

        // Followers and Following
        public virtual ICollection<UserFollower> Followers { get; set; }
        public virtual ICollection<UserFollower> Following { get; set; }

        // Artist Following
        public virtual ICollection<ArtistFollower> ArtistFollowing { get; set; }

        // Favorite Songs
        public virtual ICollection<UserFavoriteSong> FavoriteSongs { get; set; }

        // Computed property for followed artists
        [NotMapped]
        public virtual ICollection<Artists> FollowedArtists { get; set; }

        public AppUser()
        {
            ListeningRooms = new HashSet<ListeningRoom>();
            ListeningRoomParticipants = new HashSet<ListeningRoomParticipant>();
            Playlists = new HashSet<Playlist>();
            Remixes = new HashSet<Remix>();
            Followers = new HashSet<UserFollower>();
            Following = new HashSet<UserFollower>();
            ArtistFollowing = new HashSet<ArtistFollower>();
            FavoriteSongs = new HashSet<UserFavoriteSong>();
            FollowedArtists = new HashSet<Artists>();
        }

        // Helper methods to get counts
        public int GetPlaylistCount()
        {
            return Playlists?.Count ?? 0;
        }

        public int GetFollowersCount()
        {
            return Followers?.Count ?? 0;
        }

        public int GetFollowingCount()
        {
            return Following?.Count ?? 0;
        }

        public int GetPublicPlaylistCount()
        {
            return Playlists?.Count(p => p.IsPublic) ?? 0;
        }

        public int GetFavoriteSongsCount()
        {
            return FavoriteSongs?.Count ?? 0;
        }

        public int GetFollowedArtistsCount()
        {
            return ArtistFollowing?.Count ?? 0;
        }
    }
}
