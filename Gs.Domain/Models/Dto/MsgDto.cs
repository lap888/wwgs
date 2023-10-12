namespace Gs.Domain.Models.Dto
{
    public class MsgDto
    {
        public string Msg_Id { get; set; }
        public bool Is_Valid { get; set; }
        public ErrorDto Error { get; set; }
    }
    public class ErrorDto
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}