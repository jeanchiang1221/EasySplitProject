using EasySplitProject.Models;
using EasySplitProject.Models.ViewModels;
using EasySplitProject.Security;
using NSwag.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace EasySplitProject.Controllers
{
    /// <summary>
    /// 收款資訊新增、刪除
    /// </summary>
    [OpenApiTag("Payment", Description = "收款資訊新增、刪除、編輯")]
    public class PaymentController : ApiController
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// 2.1 取得所有收款資訊
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Payment/GetAll")]
        [JwtAuthFilter]
        public IHttpActionResult UserPaymentInfo()
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var bank = db.PaymentBanks.Where(x => x.UserId == userId).Select(x=>new 
            {
                id = x.Id,
                bankName = x.Bank,
                bankAccountName=x.AccountName,
                bankCode=x.BankCode,
                bankAccount=x.Account
            });
            var cash = db.PaymentCashs.Where(x => x.UserId == userId).Select(x => new
            {
                id = x.Id,
                name = x.Name,
                phone = x.Phone,
                method = x.Method,
            });
            var line = db.PaymentLines.Where(x => x.UserId == userId).Select(x => new
            {
                id = x.Id,
                name=x.Name,
                phone = x.Phone,
                lineId = x.LineID,
                qrCodeUrl= "https://" + Request.RequestUri.Host + "/upload/Payment/" + x.QRCode
            });

            return Ok(new { status = "success", message = "所有收款資訊", bank = bank , cash =cash,line =line}) ;

        }

        /// <summary>
        /// 2.2.1新增收款資訊 - 現金面交
        /// </summary>
        /// <param name="userData">現金面交資訊</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Payment/AddCash")]
        [JwtAuthFilter]
        public IHttpActionResult AddCash(PaymentCashVM userData)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            try
            {
                PaymentCash userInput = new PaymentCash
                {
                    Name=userData.Name,
                    Phone = userData.Phone,
                    Method= userData.Method,
                    UserId=userId,
                };
                // 加入資料並儲存
                db.PaymentCashs.Add(userInput);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new { Status = false, Message = ex.Message });
            }

            return Ok(new { Status = true, Message = "新增現金收款資訊成功!" });
        }

        /// <summary>
        /// 2.2.2刪除收款資訊 - 現金面交
        /// </summary>
        /// <param name="paymentCashId">現金收款的id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Payment/DeleteCash/{paymentCashId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeletePaymentCash(int paymentCashId)
        {
           
            if (paymentCashId <= 0)
                return BadRequest("非有效id");
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var PaymentCashDelete = db.PaymentCashs.Where(x => x.UserId == userId && x.Id == paymentCashId).First();

            if (PaymentCashDelete!=null)
            {
                db.PaymentCashs.Remove(PaymentCashDelete);
                db.SaveChanges();
            }

            return Ok(new { status = true, message = "刪除現金收款資訊成功!" });
        }

        /// <summary>
        /// 2.2.3編輯收款資訊 - 現金面交
        /// </summary>
        /// <param name="paymentCashId">現金收款的id</param>
        /// <param name="userData">現金面交資訊</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Payment/EditCash/{paymentCashId}")]
        [JwtAuthFilter]
        public IHttpActionResult EditPaymentCash(int paymentCashId, PaymentCashVM userData)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var PaymentCashEdit = db.PaymentCashs.Where(x => x.UserId == userId && x.Id == paymentCashId).First();

            if (paymentCashId <= 0)
                return BadRequest("非有效id");
            if (PaymentCashEdit != null)
            {
                PaymentCashEdit.Name = userData.Name;
                PaymentCashEdit.Phone = userData.Phone;
                PaymentCashEdit.Method = userData.Method;
                db.SaveChanges();
            }

            return Ok(new { status = true, message = "編輯現金收款資訊成功!" });
        }

        /// <summary>
        /// 2.3.1新增收款資訊 - 銀行轉帳
        /// </summary>
        /// <param name="userData">銀行收款資訊</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Payment/AddBank")]
        [JwtAuthFilter]
        public IHttpActionResult AddBank(PaymentBankVM userData)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            try
            {
                PaymentBank userInput = new PaymentBank
                {
                    Bank = userData.Bank,
                    AccountName = userData.AccountName,
                    BankCode = userData.BankCode,
                    Account=userData.Account,
                    UserId=userId
                };
                // 加入資料並儲存
                db.PaymentBanks.Add(userInput);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }

            return Ok(new { status = true, message = "新增銀行收款資訊成功!" });
        }

        /// <summary>
        /// 2.3.2刪除收款資訊 - 銀行轉帳
        /// </summary>
        /// <param name="paymentBankId">銀行收款的id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Payment/DeleteBank/{paymentBankId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeletePaymentBank(int paymentBankId)
        {

            if (paymentBankId <= 0)
                return BadRequest("非有效id");
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var PaymentBankDelete = db.PaymentBanks.Where(x => x.UserId == userId && x.Id == paymentBankId).First();

            if (PaymentBankDelete != null)
            {
                db.PaymentBanks.Remove(PaymentBankDelete);
                db.SaveChanges();
            }

            return Ok(new { status = true, message = "刪除銀行收款資訊成功!" });
        }


        /// <summary>
        ///2.3.3編輯收款資訊 - 銀行轉帳
        /// </summary>
        /// <param name="paymentBankId">銀行收款的id</param>
        /// <param name="userData">銀行收款資訊</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Payment/EditBank/{paymentBankId}")]
        [JwtAuthFilter]
        public IHttpActionResult EditPaymentBank(int paymentBankId, PaymentBankVM userData)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var PaymentBankEdit = db.PaymentBanks.Where(x => x.UserId == userId && x.Id == paymentBankId).First();

            if (paymentBankId <= 0)
                return BadRequest("非有效id");
            if (PaymentBankEdit != null)
            {
                PaymentBankEdit.Bank = userData.Bank;
                PaymentBankEdit.AccountName = userData.AccountName;
                PaymentBankEdit.BankCode = userData.BankCode;
                PaymentBankEdit.Account = userData.Account;
                db.SaveChanges();
            }

            return Ok(new { status = true, message = "編輯銀行收款資訊成功!" });
        }

        /// <summary>
        /// 2.4.1新增收款資訊 - LINEPay
        /// </summary>
        /// <param name="userData">LINEPay收款資訊</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Payment/AddLinePay")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> AddLinePay(PaymentLineVM userData)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            try
            {
                PaymentLine userInput = new PaymentLine
                {
                    QRCode = userData.QRCode,
                    Name = userData.Name,
                    LineID = userData.LineID,
                    Phone = userData.Phone,
                    UserId = userId
                };
                // 加入資料並儲存
                db.PaymentLines.Add(userInput);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }

            return Ok(new { status = true, message = "新增LinePay收款資訊成功!" });
        }



        /// <summary>
        /// 2.4.1.1新增收款資訊 - LINEPay QRcode
        /// </summary>
        /// <param name="userData">LINEPay收款資訊</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Payment/AddLinePayQRcode")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> AddLinePayQRcode(PaymentLineVM userData)
        {

            // 檢查請求是否包含 multipart/form-data.
            string fileName;
            if (!Request.Content.IsMimeMultipartContent())
            {
                //  throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                fileName = "defaultLinepay.jpg";

            }
            else
            {
                // 讀取 MIME 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                // 定義檔案名稱
                fileName = fileNameData + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                // 檢查資料夾是否存在，若無則建立
                string root = HttpContext.Current.Server.MapPath("~/upload/Payment");
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory("~/upload/Payment");
                }

                // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }
            }

            return Ok(new { status = true, message = "新增LinePay QRcode成功!", qrCode=fileName });
        }

        /// <summary>
        /// 2.4.2刪除收款資訊 - LINEPay
        /// </summary>
        /// <param name="paymentLineId">Linepay收款的id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Payment/DeleteLine/{paymentLineId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeletePaymentLine(int paymentLineId)
        {

            if (paymentLineId <= 0)
                return BadRequest("非有效id");
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var PaymentLineDelete = db.PaymentLines.Where(x => x.UserId == userId && x.Id == paymentLineId).First();

            if (PaymentLineDelete != null)
            {
                db.PaymentLines.Remove(PaymentLineDelete);
                db.SaveChanges();
            }

            return Ok(new { status = true, message = "刪除Line收款資訊成功!" });
        }


        /// <summary>
        /// 2.4.3編輯收款資訊 - LINEPay
        /// </summary>
        /// <param name="paymentLineId">LinePay收款的id</param>
        /// <param name="userData">LinePay收款資訊</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Payment/EditLine/{paymentLineId}")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> EditPaymentBank(int paymentLineId, PaymentLineVM userData)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var PaymentLineEdit = db.PaymentLines.Where(x => x.UserId == userId && x.Id == paymentLineId).First();


            // 檢查請求是否包含 multipart/form-data.
            string fileName;
            if (!Request.Content.IsMimeMultipartContent())
            {
                //  throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                fileName = PaymentLineEdit.QRCode.ToString();

            }
            else
            {
                // 讀取 MIME 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                // 定義檔案名稱
                fileName = fileNameData + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                // 檢查資料夾是否存在，若無則建立
                string root = HttpContext.Current.Server.MapPath("~/Upload/GroupCover");
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory("~/UploadGroupCover");
                }

                // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }
            }




            if (paymentLineId <= 0)
                return BadRequest("非有效id");
            if (PaymentLineEdit != null)
            {
                PaymentLineEdit.QRCode = fileName;
                PaymentLineEdit.Name = userData.Name;
                PaymentLineEdit.LineID = userData.LineID;
                PaymentLineEdit.Phone = userData.Phone;

                db.SaveChanges();
            }

            return Ok(new { status = true, message = "編輯LinePay收款資訊成功!" });
        }

        //// GET api/<controller>
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<controller>/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<controller>
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<controller>/5
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<controller>/5
        //public void Delete(int id)
        //{
        //}
    }
}