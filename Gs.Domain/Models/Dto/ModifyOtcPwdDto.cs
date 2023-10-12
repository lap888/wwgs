namespace Gs.Domain.Models.Dto
{
    public class ModifyOtcPwdDto
    {
        public string Mobile { get; set; }
        public string VCode { get; set; }
        public string NewTradePwd { get; set; }
        public string MsgId { get; set; }
    }
}