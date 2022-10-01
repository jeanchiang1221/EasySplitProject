using EasySplitProject.Models;
using EasySplitProject.Models.ViewModels;
using EasySplitProject.Security;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace EasySplitProject.Controllers
{
    /// <summary>
    /// 註冊、登入、忘記密碼、登出
    /// </summary>
    [OpenApiTag("User", Description = "使用者操作功能(註冊、登入、忘記密碼、登出)")]
    public class UserController : ApiController
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private const int expiryTime = 1; // 信箱開通碼效期

        
        /// <summary>
        /// TEST
        /// </summary>
        /// <param name="x">數字</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/User/Test")]
        public IHttpActionResult Test()
        {
       
            return Ok(new { message ="OK" });
        }

       
        /// <summary>
        /// 1.1.1Email 帳號註冊
        /// </summary>
        /// <param name="userData">註冊資料</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/User/SignUp")]
        public IHttpActionResult SignUp(SignUp userData)
        {
            // 必填欄位資料檢查
            // 帳號已註冊過
            if (!(db.Users.FirstOrDefault(x => x.Account == userData.Account) == null)) return Ok(new{ Status = false, Message = "帳號已存在" });

            try
            {
                // 生成密碼雜湊鹽
                string saltStr = HashPassword.CreateSalt();
                // 生成郵件連結驗證碼
                string mailGuid = Guid.NewGuid().ToString();
                // 生成使用者資料
                string[] accountArr = userData.Account.Split('@');

                User userInput = new User
                {
                    Account = userData.Account,
                    HashPassword = HashPassword.GenerateHashWithSalt(userData.Password, saltStr),
                    Salt = saltStr,
                    Image = "defaultprofile.jpg",
                    Name = userData.Name,
                    AccountState = false,
                    CreatDate = DateTime.Now,
                    CheckMailCode = mailGuid,
                    MailCodeCreatDate = DateTime.Now.AddDays(expiryTime) // 設定驗證碼效期1天
                };
                // 加入資料並儲存
                db.Users.Add(userInput);
                db.SaveChanges();

                string verifyLink = Mail.SetAuthMailLink(Request.RequestUri.Host, mailGuid);
                Mail.SendVerifyLinkMail(accountArr[0], userData.Account, verifyLink);
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }

            return Ok(new { status = true, message = "註冊成功，請至信箱點選開通帳號連結登入!" });
        }




        /// <summary>
        /// 1.2註冊開通
        /// </summary>
        /// <param name="mailGuid">信箱驗證的guid</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/User/AuthMail/AccountActivation/{mailGuid}")]
        public IHttpActionResult AuthMail(string mailGuid)
        {
            // 查詢指定帳號
            var userQuery = db.Users.FirstOrDefault(x => x.CheckMailCode == mailGuid);

            if (mailGuid == null) return Ok(new { status = false, message = "未填欄位" });
            // 帳號檢查
            else if (userQuery == null) return Ok(new { status = false, message = "帳號驗證碼不存在" });

            // 判斷 guid 時效及開通狀態，驗證碼期限未到期就開通帳號
            string verifyLink;
            string[] accountArr;
            if (userQuery.AccountState)
            {
                // 帳號已開通
                return Ok(new  { status = false, message = "帳號已開通，可直接登入使用" });
            }
            else if (userQuery.MailCodeCreatDate < DateTime.Now && !userQuery.AccountState)
            {
                // 驗證連結 guid 過期，生成郵件連結驗證碼重寄驗證信
                mailGuid = Guid.NewGuid().ToString();
                userQuery.CheckMailCode = mailGuid;
                userQuery.MailCodeCreatDate = DateTime.Now.AddDays(1);
                verifyLink = Mail.SetAuthMailLink(Request.RequestUri.Host, mailGuid);
                accountArr = userQuery.Account.Split('@');
                Mail.SendVerifyLinkMail(accountArr[0], userQuery.Account, verifyLink);
                return Ok(new{ status = false, message = "開通驗證連結過期，已重新發信，請至信箱收信重新驗證" });
            }

            // 開通更新
            userQuery.AccountState = true;
            db.SaveChanges();
          
            return Ok(new { status = true, message = "帳號開通成功，請重新登入!" });

        }


        /// <summary>
        /// 1.3會員登入
        /// </summary>
        /// <param name="userData">登入資料</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/User/Login")]
        public IHttpActionResult Login(Login userData)
        {
            // 查詢指定帳號
            var userQuery = db.Users.FirstOrDefault(x => x.Account == userData.Account);
            // 帳號檢查
            if (userQuery == null) return Ok(new{ status = false, message = "帳號不存在" });
           
            // 登入密碼加鹽雜湊結果
            string hashPassword = HashPassword.GenerateHashWithSalt(userData.Password, userQuery.Salt);
            // 密碼檢查
            if (!(userQuery.HashPassword.Equals(hashPassword))) return Ok(new { status = false, message = "密碼不正確" });

            // 有網址傳值 guid 且驗證碼期限未到期就開通帳號
            string verifyLink;
            string[] accountArr;
            JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
            string jwtToken = jwtAuthUtil.GenerateToken(userQuery.Id,userQuery.Account,userQuery.Name,userQuery.Image);
            if (!userQuery.AccountState && userQuery.MailCodeCreatDate > DateTime.Now)
            {
                // 權限未開通且信箱開通連結未過期
                return Ok(new { status = false, message = "未開通驗證，請至信箱收信開通驗證" });
            }
            else if (!userQuery.AccountState)
            {
                // 權限未開通且信箱開通連結已過期，生成郵件連結驗證碼重寄驗證信
                string mailGuid = Guid.NewGuid().ToString();
                userQuery.CheckMailCode = mailGuid;
                userQuery.MailCodeCreatDate = DateTime.Now.AddDays(1);
                db.SaveChanges();

                verifyLink = Mail.SetAuthMailLink(Request.RequestUri.Host, mailGuid);
                accountArr = userData.Account.Split('@');
                Mail.SendVerifyLinkMail(accountArr[0], userData.Account, verifyLink);
                return Ok(new{ status = false, message = "開通驗證連結過期，已重新發信，請至信箱收信重新驗證" });
            }

            // 一般登入
            return Ok(new { status = true, jwtToken = jwtToken, message = "登入成功" });
        }

        
        /// <summary>
        /// 1.4.1 忘記密碼發信
        /// </summary>
        /// <param name="accountData">帳號資料</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/User/ForgetPassword")]
        public IHttpActionResult SendResetPasswordMail(Account accountData)
        {
            // 查詢指定帳號
            var userQuery = db.Users.FirstOrDefault(x => x.Account == accountData.AccountMail);
            // 帳號檢查
            if (userQuery == null) return Ok(new { status = false, message = "帳號不存在" });
  
            // 生成重設密碼連結，發信
            string resetLink;
            string[] accountArr;
            string mailGuid = Guid.NewGuid().ToString();
            userQuery.CheckMailCode = mailGuid;
            userQuery.MailCodeCreatDate = DateTime.Now;
            resetLink = Mail.SetResetPasswordMailLink(Request.RequestUri.Host, mailGuid);
            accountArr = userQuery.Account.Split('@');
            Mail.SendResetLinkMail(accountArr[0], userQuery.Account, resetLink);
            db.SaveChanges();
            return Ok(new{ status = true, message = "已發送重設密碼連結至信箱，請至信箱收信點選連結重設密碼" });
        }


        /// <summary>
        /// 1.4.2 登入時更改密碼的發信
        /// </summary>
        /// <returns></returns>
        [JwtAuthFilter]
        [HttpPost]
        [Route("api/User/ResetPassword")]
        public IHttpActionResult LoginResetPassword()
        {
            // 解密 JwtToken 取出資料回傳
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            //單純刷新效期不新生成，新生成會進資料庫
            JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
            string jwtToken = jwtAuthUtil.ExpRefreshToken(userToken);
            
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));

            try
              {
                // 查詢指定帳號
                var userQuery = db.Users.FirstOrDefault(x => x.Id == userId);

                // 生成重設密碼連結，發信
                string resetLink;
                string[] accountArr;
                string mailGuid = Guid.NewGuid().ToString();
                userQuery.CheckMailCode = mailGuid;
                userQuery.MailCodeCreatDate = DateTime.Now;
                resetLink = Mail.SetResetPasswordMailLink(Request.RequestUri.Host, mailGuid);
                accountArr = userQuery.Account.Split('@');
                Mail.SendResetLinkMail(accountArr[0], userQuery.Account, resetLink);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new  { status = false, message = ex.Message });
            }

            // 刷新 JwtToken
            return Ok(new  { status = true, jwtToken = jwtToken, message = "已發送重設密碼連結至信箱，請至信箱收信點選連結重設密碼" });
        }


        /// <summary>
        /// 1.4.3 信箱連入重設密碼的頁面
        /// </summary>
        /// <param name="resetData">信箱連入重設密碼資料</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/User/AuthMail/ResetPassword")]
        public IHttpActionResult MailLinkResetPassword(PasswordReset resetData)
        {
            // 查詢指定帳號
            var userQuery = db.Users.FirstOrDefault(x => x.CheckMailCode == resetData.Guid);

            if (resetData.Guid == null) return Ok(new { status = false, message = "未填欄位" });
            else if (userQuery.MailCodeCreatDate.AddMinutes(30) < DateTime.Now) return Ok(new { status = false, message = "連結驗證碼超過30分鐘" });
            // 帳號檢查
            else if (userQuery == null) return Ok(new { status = false, message = "連結驗證碼不存在" });
            // 生成密碼雜湊鹽
            string saltStr = HashPassword.CreateSalt();
            // 登入密碼加鹽雜湊結果
            string hashPassword = HashPassword.GenerateHashWithSalt(resetData.NewPassword, saltStr);

            // 更新密碼，註銷驗證碼
            userQuery.CheckMailCode = "";
            userQuery.Salt = saltStr;
            userQuery.HashPassword = hashPassword;
            db.SaveChanges();

            // 重設成功
            return Ok(new { status = true, message = "密碼重設成功，請重新登入" });
        }


        /// <summary>
        /// 1.5.1 會員資料檢視
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/User/GetProfile")]
        [JwtAuthFilter]
        public IHttpActionResult UserInfo()
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var account = JwtAuthUtil.GetAccount(Request.Headers.Authorization.Parameter);
            var user = db.Users.FirstOrDefault(x => x.Account == account);
            var result = new
            {
                userId=userId,
                account=user.Account,
                name=user.Name,
                imageUrl= "https://"+Request.RequestUri.Host+"/upload/UserAvatar/"+ user.Image,
            };
            return Ok(new { status = true, message = "會員資料", userdata = result });

        }

        /// <summary>
        /// 1.5.2 編輯會員資料
        /// </summary>
        /// <param name="name">修改後的會員名稱</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/User/EditProfile")]
        [JwtAuthFilter]
        public IHttpActionResult EditUserInfo(string name)
        {
            var account = JwtAuthUtil.GetAccount(Request.Headers.Authorization.Parameter);
            var user = db.Users.FirstOrDefault(x => x.Account == account);
            user.Name = name;
            db.SaveChanges();
            return Ok(new { status = true, message = "會員資料成功修改" });
        }


        /// <summary>
        /// 1.5.3 編輯/上傳大頭貼照片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/User/UploadAvatar")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> EditProfilePic()
        {
            // 檢查請求是否包含 multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // 檢查資料夾是否存在，若無則建立
            string root = HttpContext.Current.Server.MapPath("~/upload/UserAvatar");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory("~/upload/UserAvatar");
            }

            var account = JwtAuthUtil.GetAccount(Request.Headers.Authorization.Parameter);
            //if (account == null)
            //{
            //    return Ok(new { status = "error", message = "請重新登入" });
            //}

            var user = db.Users.FirstOrDefault(x => x.Account == account);


            try
            {
                // 讀取 MIME 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                // 定義檔案名稱
                string fileName = user.Name + "Avatar" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                // 使用 SixLabors.ImageSharp 調整圖片尺寸 (正方形大頭貼，這個地方在跟前端確認大小)
                var image = SixLabors.ImageSharp.Image.Load<Rgba32>(outputPath);
                image.Mutate(x => x.Resize(120, 120)); // 輸入(120, 0)會保持比例出現黑邊
                image.Save(outputPath);



                user.Image = fileName;
                db.SaveChanges();

                return Ok(new
                {
                    status = true,
                    message = "頭像更新成功",
                    data = new UserEditData
                    {
                        Image = fileName
                    }
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message); // 400
            }
        }


        /// <summary>
        /// 1.6 登出
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/User/Logout")]
        public IHttpActionResult Logout()
        {

            // 刷新 JwtToken 使其失效
            JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
            string jwtToken = jwtAuthUtil.RevokeToken();

            // 登出刷新
            return Ok(new{ status = true, message = "登出成功" , jwtToken = jwtToken });
        }

        /// <summary>
        /// 藍新金流-未來展望
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //public IHttpActionResult SetChargeData()
        //{
        //    //ChargeRequest chargeData
        //    // Do Something ~ (相關資料檢查處理，成立訂單加入資料庫，並將訂單付款狀態設為未付款)

        //    // 整理金流串接資料
        //    // 加密用金鑰
        //    string hashKey = WebConfigurationManager.AppSettings["HashKey"];
        //    string hashIV = WebConfigurationManager.AppSettings["HashIV"];

        //    // 金流接收必填資料
        //    string merchantID = WebConfigurationManager.AppSettings["MerchantID"];
        //    string tradeInfo = "";
        //    string tradeSha = "";
        //    string version = WebConfigurationManager.AppSettings["Version"]; // 參考文件串接程式版本

        //    // tradeInfo 內容，導回的網址都需為 https 
        //    string respondType = "JSON"; // 回傳格式
        //    string timeStamp = ((int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
        //    string merchantOrderNo = timeStamp + "_" + "訂單ID"; // 底線後方為訂單ID，解密比對用，不可重覆(規則參考文件)
        //    string amt = "訂單金額";
        //    string itemDesc = "商品資訊";
        //    string tradeLimit = "600"; // 交易限制秒數
        //    string notifyURL = @"https://" + Request.RequestUri.Host + WebConfigurationManager.AppSettings["NotifyURL"]; // NotifyURL 填後端接收藍新付款結果的 API 位置，如 : /api/users/getpaymentdata
        //    string returnURL = "付款完成導回頁面網址" + "/" + "訂單ID";  // 前端可用 Status: SUCCESS 來判斷付款成功，網址夾帶可拿來取得活動內容
        //    string email = WebConfigurationManager.AppSettings["gmailAccount"]; // 通知付款完成用
        //    string loginType = "0"; // 0不須登入藍新金流會員

        //    // 將 model 轉換為List<KeyValuePair<string, string>>
        //    List<KeyValuePair<string, string>> tradeData = new List<KeyValuePair<string, string>>() {
        //    new KeyValuePair<string, string>("MerchantID", merchantID),
        //    new KeyValuePair<string, string>("RespondType", respondType),
        //    new KeyValuePair<string, string>("TimeStamp", timeStamp),
        //    new KeyValuePair<string, string>("Version", version),
        //    new KeyValuePair<string, string>("MerchantOrderNo", merchantOrderNo),
        //    new KeyValuePair<string, string>("Amt", amt),
        //    new KeyValuePair<string, string>("ItemDesc", itemDesc),
        //    new KeyValuePair<string, string>("TradeLimit", tradeLimit),
        //    new KeyValuePair<string, string>("NotifyURL", notifyURL),
        //    new KeyValuePair<string, string>("ReturnURL", returnURL),
        //    new KeyValuePair<string, string>("Email", email),
        //    new KeyValuePair<string, string>("LoginType", loginType)
        //};

        //    // 將 List<KeyValuePair<string, string>> 轉換為 key1=Value1&key2=Value2&key3=Value3...
        //    var tradeQueryPara = string.Join("&", tradeData.Select(x => $"{x.Key}={x.Value}"));
        //    // AES 加密
        //    tradeInfo = CryptoUtil.EncryptAESHex(tradeQueryPara, hashKey, hashIV);
        //    // SHA256 加密
        //    tradeSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");

        //    // 送出金流串接用資料，給前端送藍新用
        //    return Ok(new
        //    {
        //        Status = true,
        //        PaymentData = new
        //        {
        //            MerchantID = merchantID,
        //            TradeInfo = tradeInfo,
        //            TradeSha = tradeSha,
        //            Version = version
        //        }
        //    });
        //}
    }
}