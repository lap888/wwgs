namespace Gs.Domain.Models.Dto
{
    public class OrderGameDto : BaseModel
    {
        public int Id { get; set; }
        public string Mobile { get; set; }

        public string OrderId { get; set; }

        public string status { get; set; }

        public int UserId { get; set; }
    }
}