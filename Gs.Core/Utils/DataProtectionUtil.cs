using Gs.Core.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace Gs.Core.Utils
{
    public static class DataProtectionUtil
    {
        private static IDataProtector _dataProtector => ServiceExtension.ServiceProvider.GetDataProtector("Asp.NetCore", "XingChengWuXian", "NiaoWo");
        public static string Protect(string plaintext)
        {
            return _dataProtector.Protect(plaintext);//SecurityUtil.AesEncrypt(plaintext,"NiaoWo");
        }
        public static string UnProtect(string protectedData)
        {
            return _dataProtector.Unprotect(protectedData);//SecurityUtil.AesDecrypt(protectedData,"NiaoWo");
        }


    }
}