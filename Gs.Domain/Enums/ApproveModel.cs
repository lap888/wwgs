using System.ComponentModel;

namespace Gs.Domain.Enums
{
    public enum ApproveModel
    {
        [Description("自动认证")]
        AutoApprove = 1,
        [Description("人工认证")]
        PersonApprove = 2,
    }
}