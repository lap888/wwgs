using Microsoft.AspNetCore.Authorization;

namespace Gs.Core.Mvc
{
    public class MvcAuthorizeAttribute : AuthorizeAttribute
    {
        public string LoginPath
        {
            get;
            set;
        }

        public string AccessDeniedPath
        {
            get;
            set;
        }

        public MvcAuthorizeAttribute()
            : base()
        {
        }
    }
}