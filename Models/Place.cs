namespace WebApplication1.Models
{
    public class Place
    {
        public string name { get; set; }
        public string image { get; set; }
        public string Description { get; set; }
        public string url { get; set; }
    }
    public class PhotoInfo
    {
        public Candidate[] candidates { get; set; }
        public string status { get; set; }
    }

    public class Candidate
    {
        public Photo[] photos { get; set; }
        public string place_id { get; set; }
    }

    public class Photo
    {
        public int height { get; set; }
        public string[] html_attributions { get; set; }
        public string photo_reference { get; set; }
        public int width { get; set; }
    }
    public class PlaceDetails
    {
        public result result { get; set; }
    }
    public class result
    {
        public string url { get; set; }
        public string vicinity { get; set; }
    }
}
