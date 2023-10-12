namespace Gs.Domain.Models.Dto
{
    public class BannerDto : BaseModel
    {
        public int? Id { get; set; }
        public int? Types { get; set; }
        public string Pic { get; set; }
        public string JumpUrl { get; set; }
        public string Params { get; set; }
        public int Queue { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string CityCode { get; set; }
        public int Source { get; set; }
        public string ContentFwb { get; set; }
    }
}