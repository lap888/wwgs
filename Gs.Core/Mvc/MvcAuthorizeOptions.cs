using Microsoft.AspNetCore.Http;

namespace Gs.Core.Mvc
{
    public class MvcAuthorizeOptions
    {
        public string ReturnUrlParameter
        {
            get;
            set;
        } = "ReturnUrl";


        public PathString AccessDeniedPath
        {
            get;
            set;
        } = new PathString("/Account/AccessDenied");


        public PathString LogoutPath
        {
            get;
            set;
        } = new PathString("/Account/Logout");


        public PathString LoginPath
        {
            get;
            set;
        } = new PathString("/Account/Login");


        public string AuthenticationScheme
        {
            get;
            set;
        } = "NetCore";
    }
}