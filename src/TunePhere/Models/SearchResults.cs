using System.Collections.Generic;

namespace TunePhere.Models
{
    public class SearchResults
    {
        public List<Song> Songs { get; set; } = new List<Song>();
        public List<Artists> Artists { get; set; } = new List<Artists>();
        public List<Album> Albums { get; set; } = new List<Album>();
        public string? SearchTerm { get; set; }
    }
} 