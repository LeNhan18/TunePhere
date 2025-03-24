using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace TunePhere.Models
{
    public class ArtistSong
    {
       [ForeignKey("SongId")]
       public int SongId { get; set; }
       public Song? Song { get; set; }

       [ForeignKey("ArtistId")]
       public int ArtistId { get; set; }
       public Artists? Artist { get; set; }

    }
}
