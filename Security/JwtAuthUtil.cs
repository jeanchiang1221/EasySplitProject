using EasySplitProject.Models;
using Jose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace EasySplitProject.Security
{
    /// <summary>
    /// JwtToken 生成功能
    /// </summary>
    public class JwtAuthUtil
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext(); // DB 連線

        /// <summary>
        /// 生成 JwtToken
        /// </summary>
        /// <param name="id">會員id</param>
        /// <param name="account">帳號</param>
        /// <param name="name">暱稱</param>
        /// <param name="image">暱稱</param>
        /// <returns>JwtToken</returns>
        //登入成功後產生token
        public string GenerateToken(int id,string account, string name,string image)
        {
            // 自訂字串，驗證用，用來加密送出的 key (放在 Web.config 的 appSettings)
            string secretKey = WebConfigurationManager.AppSettings["TokenKey"]; // 從 appSettings 取出
                                                                               
            Dictionary<string, Object> claim = new Dictionary<string, Object>();//payload 需透過token傳遞的資料
            claim.Add("id", id);
            claim.Add("account", account);
            claim.Add("name", name);
            claim.Add("initTime", DateTime.Now.ToString());//建立時間
            claim.Add("exp", DateTime.Now.AddDays(1).ToString());//Token 時效設定一天 
            var payload = claim;
            var token = Jose.JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);//產生token
            return token;
        }

        /// <summary>
        /// 生成只刷新效期的 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string ExpRefreshToken(Dictionary<string, object> tokenData)
        {
            string secretKey = WebConfigurationManager.AppSettings["TokenKey"];
            // payload 從原本 token 傳遞的資料沿用，並刷新效期
            var payload = new Dictionary<string, object>
            {
                { "id", (int)tokenData["id"] },
                { "account", tokenData["account"].ToString() },
                { "name", tokenData["name"].ToString() },
                { "initTime", tokenData["initTime"].ToString() },
                { "exp", DateTime.Now.AddMinutes(30).ToString() } // JwtToken 時效刷新設定 30 分
            };

            //產生刷新時效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 生成無效 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        //這裡是登出的時候要用
        public string RevokeToken()
        {
            string secretKey = "RevokeToken"; // 故意用不同的 key 生成
            var payload = new Dictionary<string, object>
            {
                { "id", 0 },
                { "account", "None" },
                { "name", "None" },
                { "initTime", DateTime.Now.ToString() },
                { "exp", DateTime.Now.AddDays(-15).ToString() } // 使 JwtToken 過期 失效
            };

            // 產生失效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 查詢使用者帳號
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static string GetAccount(string Token)
        {
            string secert = WebConfigurationManager.AppSettings["TokenKey"];
            var jwtObject = Jose.JWT.Decode<Dictionary<string, Object>>(
                Token,
                Encoding.UTF8.GetBytes(secert),
                JwsAlgorithm.HS512);
            return jwtObject["account"].ToString();
        }

        /// <summary>
        /// 查詢使用者ID
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static string GetUserId(string Token)
        {
            string secert = WebConfigurationManager.AppSettings["TokenKey"];
            var jwtObject = Jose.JWT.Decode<Dictionary<string, Object>>(
                Token,
                Encoding.UTF8.GetBytes(secert),
                JwsAlgorithm.HS512);
            return jwtObject["id"].ToString();
        }

        /// <summary>
        /// 查詢使用者名稱
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static string GetUserName(string Token)
        {
            string secert = WebConfigurationManager.AppSettings["TokenKey"];
            var jwtObject = Jose.JWT.Decode<Dictionary<string, Object>>(
                Token,
                Encoding.UTF8.GetBytes(secert),
                JwsAlgorithm.HS512);
            return jwtObject["name"].ToString();
        }
    }
}