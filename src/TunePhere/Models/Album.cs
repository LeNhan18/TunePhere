namespace TunePhere.Models
{
    public class Album
    {
        public int AlbumId { get; set; }

        public string? AlbumName { get; set; }
        public string? AlbumDescription { get; set; }

        public string? ImageUrl { get; set; }

        public string? ReleaseDate { get; set; }

        public int numberSongs { get; set; }

        public TimeSpan Time { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public Album() {
            Songs = new HashSet<Song>();
        }
    }
}
