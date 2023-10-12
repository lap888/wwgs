using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Core
{
    /// <summary>
    /// 鉴权类
    /// </summary>
    public static class AuthToken
    {
#if DEBUG
        const String TokenSecurityKey = "06e4d1fcf43513a67d976ade88f45321";
#else
        const String TokenSecurityKey = "dd4e30292b393af6a8e10f5b900851f8";
#endif
        /// <summary>
        /// 设置Token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Data"></param>
        /// <returns>Token</returns>
        public static String SetToken(Object Data)
        {
            var content = null == Data ? "{}" : Data.ToJson(false, false);
            var nonce = Guid.NewGuid().ToString("N").Substring(0, 12);
            var associated = Security.MD5(nonce + TokenSecurityKey, true);
            var body = Security.GcmEncrypt(content, TokenSecurityKey, nonce, associated);
            var foot = Security.SHA($"{nonce}{TokenSecurityKey}{content}", Security.SHAType.SHA256);
            var token = $"{Security.Base64Encrypt(nonce)}.{body}.{foot}";
            return token;
        }

        /// <summary>
        /// 获取Token内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static T GetToken<T>(String Token) where T : class, new()
        {
            if (!CheckToken(Token, out String DeBody)) { return null; }
            return DeBody.JsonTo<T>();
        }

        /// <summary>
        /// 获取TOKEN内容
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static String GetToken(String Token)
        {
            if (!CheckToken(Token, out String DeBody)) { return null; }
            return DeBody;
        }
        /// <summary>
        /// 验证Token是否合法
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static bool CheckToken(String Token)
        {
            return CheckToken(Token, out _);
        }

        /// <summary>
        /// 验证Token是否合法
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="TokenBody"></param>
        /// <returns></returns>
        private static Boolean CheckToken(String Token, out String TokenBody)
        {
            TokenBody = String.Empty;
            if (String.IsNullOrWhiteSpace(Token)) { return false; }
            try
            {
                String[] TokenArray = Token.Split('.');
                if (TokenArray.Length != 3) { return false; }
                var nonce = Security.Base64Decrypt(TokenArray[0]);
                var associated = Security.MD5(nonce + TokenSecurityKey, true);
                var content = Security.GcmDecrypt(TokenArray[1], TokenSecurityKey, nonce, associated);
                var foot = Security.SHA($"{nonce}{TokenSecurityKey}{content}", Security.SHAType.SHA256);
                if (!foot.Equals(TokenArray[2])) { return false; }
                TokenBody = content;
                return true;
            }
            catch { return false; }
        }
    }
}