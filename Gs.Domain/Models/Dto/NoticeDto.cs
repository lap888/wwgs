namespace Gs.Domain.Models.Dto
{
    public class NoticeDto : BaseModel
    {
        public int Id { get; set; }
        public int? Types { get; set; }
        public string Content { get; set; }
        public string ContentFwb { get; set; }
        public string Title { get; set; }
    }
}