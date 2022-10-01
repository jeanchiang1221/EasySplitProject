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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static EasySplitProject.Models.EnumDB;

namespace EasySplitProject.Controllers
{
    /// <summary>
    /// 費用的新增、編輯、刪除
    /// </summary>
    [OpenApiTag("Expense", Description = "費用的新增、編輯、刪除")]
    public class ExpenseController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// 4.1.0 取得費用的種類
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Expense/GetExpenseType")]
        [JwtAuthFilter]
        public IHttpActionResult GetPaymentType()
        {
            var expenseTypeArray = Enum.GetValues(typeof(ExpenseType)).Cast<ExpenseType>().Select(x => new 
            { id = x,
              expenseMethod = x.ToString(),
              imageUrl = "https://" + Request.RequestUri.Host + "/upload/ExpenseType/" + Convert.ToInt32(x) + ".jpg",
            }).ToList();

            return Ok(new { expenseType = expenseTypeArray });
        }

        /// <summary>
        /// 4.1新增單筆費用
        /// </summary>
        ///<param name="expenseData">費用的資訊</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Expense/AddExpense")]
        [JwtAuthFilter]
        
        public async Task<IHttpActionResult> AddExpense(AddExpenseVM expenseData)
        {
            //群組的成員才可看到群組內的所有費用
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());
            //判斷user是否為群組的member
            var groupMember = db.Members.Where(x => x.GroupId == expenseData.GroupId && x.UserId == userId).FirstOrDefault();

            if (groupMember == null)
            {
                return Ok(new { status = false, message = "非群組成員無法新增其他群組費用!" });
            }

            Expense expenseInput = new Expense();
            Payer payerInput = new Payer();
            Owner ownerInput = new Owner();
            ExpenseAlbum expensePhotoInput = new ExpenseAlbum();

            expenseInput.GroupId = expenseData.GroupId;
            expenseInput.ExpenseType = expenseData.ExpenseType;
            expenseInput.Item = expenseData.Item;
            expenseInput.Cost = expenseData.Cost;
            expenseInput.CreatDate = expenseData.CreatDate;
            expenseInput.Memo = expenseData.Memo;
            db.Expenses.Add(expenseInput);
            db.SaveChanges();

            //新增付款人
           // double totalPayerAmount = 0;
            foreach (var i in expenseData.AddPayerExpenseVMs)
            {
                payerInput.ExpenseId = expenseInput.Id;
                payerInput.MemberId = i.MemberId;
                payerInput.PayAmount = i.PayAmount;
               // totalPayerAmount += i.PayAmount;
                db.Payers.Add(payerInput);
                db.SaveChanges();
            }

            //新增分款人
          //  double totalOwnerAmount = 0;
            foreach (var i in expenseData.AddOwnerExpenseVMs)
            {
                ownerInput.ExpenseId = expenseInput.Id;
                ownerInput.MemberId = i.MemberId;
                ownerInput.OwnAmount = i.OwnAmount;
               // totalOwnerAmount += i.OwnAmount;
                db.Owners.Add(ownerInput);
                db.SaveChanges();
            }


            //檢驗新增後的費用與付款人和分款人金額是否一致

            //if (expenseData.Cost != totalPayerAmount)
            //{
            //    return Ok(new { status = false, message = "新增的費用與付款人金額總和不一致，新增費用失敗!" });
            //}
            //else if (expenseData.Cost != totalOwnerAmount)
            //{
            //    return Ok(new { status = false, message = "新增的費用與分款人金額總和不一致，新增費用失敗!" });
            //}

            //費用圖片新增

            foreach (var i in expenseData.AddExpenseAlbumVMs)
            {
                expensePhotoInput.Name = i.Name;
                expensePhotoInput.ExpenseId = expenseInput.Id;

                // 加入資料
                db.ExpenseAlbums.Add(expensePhotoInput);
                db.SaveChanges();
            }

            var AddedExpenseId = db.Expenses.OrderByDescending(x=>x.Id).Select(x=>x.Id).FirstOrDefault();

            //通知功能
            var userName = JwtAuthUtil.GetUserName(Request.Headers.Authorization.Parameter);
            var groupName = db.Groups.Where(x => x.Id == expenseData.GroupId).Select(x => x.Name).FirstOrDefault();

            //其他群組成員也會收到通知
            var groupMemberNotification = db.Members.Where(x => x.GroupId == expenseData.GroupId).ToList();


            foreach (var i in groupMemberNotification)
            {
                if (i.UserId != userId)
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = i.UserId,
                        Information = $"{userName}於群組「{groupName}」新增了一筆費用「{expenseData.Item}」${expenseData.Cost}元"
                    };
                    db.Notifications.Add(userInput);
                    db.SaveChanges();
                }
                else
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = userId,
                        Information = $"您於群組「{groupName}」新增了一筆費用「{expenseData.Item}」${expenseData.Cost}元"
                    };
                    db.Notifications.Add(userInput);
                    db.SaveChanges();
                }

            };

            return Ok(new { status = true, message = "新增費用成功!", AddedExpenseId= AddedExpenseId });
        }

        /// <summary>
        /// 4.1.1.1新增單筆費用(上傳費用圖片)
        /// </summary>
      
        [HttpPost]
        [Route("api/Expense/AddExpensePhotos")]
        [JwtAuthFilter]

        public async Task<IHttpActionResult> AddExpenseImage()
        {
            //費用圖片新增
            // 檢查請求是否包含 multipart/form-data.
            ArrayList fileNameList = new ArrayList();
            string fileName = "";
            if (!Request.Content.IsMimeMultipartContent())
            {
                //  throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
               // fileName = "defaultExpensePhoto.jpg";

            }
            else
            {
                // 讀取 MIME 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                //單圖
                if (provider.Contents.Count == 1)
                {
                    // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                    string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                    string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                    // 定義檔案名稱
                    fileName = fileNameData + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                    // 檢查資料夾是否存在，若無則建立
                    string root = HttpContext.Current.Server.MapPath("~/upload/Expense");
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory("~/upload/Expense");
                    }

                    // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                    var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                    var outputPath = Path.Combine(root, fileName);
                    using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                    }

                    fileNameList.Add(fileName);
                }


                //多圖的情況
                else if (provider.Contents.Count >= 2)
                {
                    foreach (var file in provider.Contents)
                    {
                        string fileNameData = file.Headers.ContentDisposition.FileName.Trim('\"');
                        string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.'));
                        // 定義檔案名稱
                        fileName = fileNameData + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;
                        // 檢查資料夾是否存在，若無則建立
                        string root = HttpContext.Current.Server.MapPath("~/upload/Expense");
                        if (!Directory.Exists(root))
                        {
                            Directory.CreateDirectory("~/upload/Expense");
                        }

                        // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                        var fileBytes = await file.ReadAsByteArrayAsync();
                        var outputPath = Path.Combine(root, fileName);
                        using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                        }
                        fileNameList.Add(fileName);
                    }

                   
                }
            }

            return Ok(new { status = true, message = "新增費用圖片成功!", fileNameList = fileNameList });


        }

        /// <summary>
        /// 4.2.1取得所有費用紀錄
        /// </summary>
        /// <param name="groupId">群組的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Expense/GetAllExpense/{groupId}")]
        [JwtAuthFilter]
        public IHttpActionResult ShowAllGroupExpense(int groupId)
        {
            //群組的成員才可看到群組內的所有費用
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());
            //判斷user是否為群組的member
            var groupMember = db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).FirstOrDefault();

            if (groupMember == null)
            {
                return Ok(new { status = false, message = "非群組成員無法查詢其他群組費用!" });
            }   

            var userMemberId= db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).Select(x=>x.Id).FirstOrDefault();

            //判斷是否有費用紀錄
            var hasExpense = db.Expenses.Where(x => x.GroupId == groupId).FirstOrDefault();
            if (hasExpense == null)
            {
                return Ok(new { status = true, message = "群組尚無任何費用!" });
            }

            //費用列表
            ArrayList expenseList= new ArrayList();
        
            var expenseId = db.Expenses.Where(x => x.GroupId == groupId).Select(x => new { x.Id }).ToList();


            foreach (var i in expenseId)
            {     
                var allExpense = db.Expenses.Where(x => x.Id == i.Id).Select(x => new
                {
                    expenseId = x.Id,
                    item = x.Item,
                    cost = x.Cost,
                    creatDate = x.CreatDate,
                    memo = x.Memo,
                    expenseType = x.ExpenseType,
                    expenseTypeCh= x.ExpenseType.ToString()
                }).FirstOrDefault();

                int thisExID = Convert.ToInt32(allExpense.expenseId);

                var allPayer = db.Payers.Where(x => x.ExpenseId == thisExID).Select(x=>new { payerId = x.Id,expenseId=x.ExpenseId,payAmount=x.PayAmount,payerMemberId=x.MemberId }).ToList();
                var allOwner = db.Owners.Where(x => x.ExpenseId == thisExID).Select(x => new { ownerId = x.Id, expenseId = x.ExpenseId, ownAmount = x.OwnAmount, ownerMemberId = x.MemberId }).ToList();
                var allPhoto = db.ExpenseAlbums.Where(x => x.ExpenseId == thisExID);

                //判斷user在該筆費用上的關係
                ArrayList personalStatus = new ArrayList();

                var userInPayer = db.Payers.Where(x => x.ExpenseId == thisExID && x.MemberId == userMemberId).Select(x => x.PayAmount).FirstOrDefault();
                var userInOwner = db.Owners.Where(x => x.ExpenseId == thisExID && x.MemberId == userMemberId).Select(x => x.OwnAmount).FirstOrDefault();

                var balance = userInPayer - userInOwner;
                if (balance > 0)
                {
                    var data = new
                    {
                        statusEn="credit",
                        statusCh = "你借出",
                        balance = balance
                    };
                    personalStatus.Add(data);
                }
                else if (balance < 0)
                {
                    var data = new
                    {
                        statusEn = "debit",
                        statusCh = "你欠了",
                        balance = balance
                    };
                    personalStatus.Add(data);
                }
                else if (balance == 0)
                {
                    if (userInPayer == 0 && userInOwner == 0)
                    {
                        var data = new
                        {
                            statusEn = "notInvolved",
                            statusCh = "你無參與"
                        };
                        personalStatus.Add(data);
                    }

                    else
                    {
                        var data = new
                        {
                            statusEn = "none",
                            statusCh = "無待結費用",
                            balance = balance
                        };
                        personalStatus.Add(data);
                    }
 
                }


                ArrayList ownerList = new ArrayList();
                foreach (var o in allOwner)
                {
                    var ownerName = db.Members.Where(x => x.Id == o.ownerMemberId).Select(x => x.Name).FirstOrDefault();
                    var ownerData = new
                    {
                        Id = o.ownerId,
                        ExpenseId=o.expenseId,
                        MemberId = o.ownerMemberId,
                        ownerName = ownerName,
                        ownAmount = o.ownAmount
                    };
                    ownerList.Add(ownerData);
                }

                ArrayList payerList = new ArrayList();
                foreach (var p in allPayer)
                {
                    var payerName = db.Members.Where(x => x.Id == p.payerMemberId).Select(x => x.Name).FirstOrDefault();
                    var payerData = new
                    {
                        Id = p.payerId,
                        ExpenseId=p.expenseId,
                        MemberId=p.payerMemberId,
                        payerName= payerName,
                        payAmount=p.payAmount

                    };
                    payerList.Add(payerData);
                }


                if (allPayer.Any()&& allOwner.Any() && allPhoto.Any())
                {
                    ArrayList photoList = new ArrayList();
                    photoList.Add(allPhoto);
                    var expensedata = new
                    {
                        allExpense.expenseId,
                        allExpense.item,
                        allExpense.cost,
                        allExpense.creatDate,
                        allExpense.memo,
                        allExpense.expenseType,
                        allExpense.expenseTypeCh,
                        personalStatus,
                        payerList,
                        ownerList,
                        photoList
                    };
                    expenseList.Add(expensedata);

                }
                else if(allPayer.Any() && allOwner.Any())
                {

                    var expensedata = new
                    {
                        allExpense.expenseId,
                        allExpense.item,
                        allExpense.cost,
                        allExpense.creatDate,
                        allExpense.memo,
                        allExpense.expenseType,
                        allExpense.expenseTypeCh,
                        personalStatus,
                        payerList,
                        ownerList
                    };
                    expenseList.Add(expensedata);

                }
               
            }

            return Ok(new { status = "success", message = "檢視群組所有費用", userMemberId= userMemberId, expenseData = expenseList });
        }

        /// <summary>
        /// 4.2.2取得單筆費用
        /// </summary>
        /// <param name="expenseId">費用的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Expense/GetExpense/{expenseId}")]
        [JwtAuthFilter]
        public IHttpActionResult ShowAExpense(int expenseId)
        {
            //群組的成員才可看到群組內的所有費用
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());
            //判斷是否有該筆費用存在
            var hasExpenseId = db.Expenses.Where(x => x.Id == expenseId).FirstOrDefault();
            if (hasExpenseId == null)
            {
                return Ok(new { status = false, message = "未有該筆費用，或是該筆費用非此群組費用!" });
            }


            var groupId = Convert.ToInt32(db.Expenses.Where(x => x.Id == expenseId).Select(x => x.GroupId).FirstOrDefault());
            //判斷user是否為群組的member
            var groupMember = db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).FirstOrDefault();

            if (groupMember == null)
            {
                return Ok(new { status = false, message = "非群組成員無法查詢其他群組費用!" });
            }

            var Expense = db.Expenses.Where(x => x.Id == expenseId).Select(x => new
            {
                expenseId = x.Id,
                item = x.Item,
                cost = x.Cost,
                creatDate = x.CreatDate,
                memo = x.Memo,
                expenseType= x.ExpenseType,
                expenseTypeCh = x.ExpenseType.ToString()
            }).FirstOrDefault();

         
            ArrayList expenseList = new ArrayList();

            var allPayer = db.Payers.Where(x => x.ExpenseId == expenseId).Select(x => new { payerId = x.Id, expenseId = x.ExpenseId, payAmount = x.PayAmount, payerMemberId = x.MemberId }).ToList(); ;
            var allOwner = db.Owners.Where(x => x.ExpenseId == expenseId).Select(x => new { ownerId = x.Id, expenseId = x.ExpenseId, ownAmount = x.OwnAmount, ownerMemberId = x.MemberId }).ToList(); ;
            var allPhoto = db.ExpenseAlbums.Where(x => x.ExpenseId == expenseId).Select(x => new
            {
                imageUrl = "https://" + Request.RequestUri.Host + "/upload/Expense/" + x.Name
            });


            ArrayList ownerList = new ArrayList();
            foreach (var o in allOwner)
            {
                var ownerName = db.Members.Where(x => x.Id == o.ownerMemberId).Select(x => x.Name).FirstOrDefault();
                var ownerData = new
                {
                    Id = o.ownerId,
                    ExpenseId = o.expenseId,
                    MemberId = o.ownerMemberId,
                    ownerName = ownerName,
                    ownAmount = o.ownAmount
                };
                ownerList.Add(ownerData);
            }

            ArrayList payerList = new ArrayList();
            foreach (var p in allPayer)
            {
                var payerName = db.Members.Where(x => x.Id == p.payerMemberId).Select(x => x.Name).FirstOrDefault();
                var payerData = new
                {
                    Id = p.payerId,
                    ExpenseId = p.expenseId,
                    MemberId = p.payerMemberId,
                    payerName = payerName,
                    payAmount = p.payAmount

                };
                payerList.Add(payerData);
            }

            if (allPayer.Any() && allOwner.Any()&& allPhoto.Any())
            {

                ArrayList photoList = new ArrayList();
                photoList.Add(allPhoto);

                var expensedataWithPayer = new
                {
                    Expense.expenseId,
                    Expense.item,
                    Expense.cost,
                    Expense.creatDate,
                    Expense.memo,
                    Expense.expenseType,
                    Expense.expenseTypeCh,
                    payerList,
                    ownerList,
                    photoList
                };
                expenseList.Add(expensedataWithPayer);
            }
            else if(allPayer.Any() && allOwner.Any())
            {
               

                var expensedataWithPayer = new
                {
                    Expense.expenseId,
                    Expense.item,
                    Expense.cost,
                    Expense.creatDate,
                    Expense.memo,
                    Expense.expenseType,
                    Expense.expenseTypeCh,
                    payerList,
                    ownerList
                };
                expenseList.Add(expensedataWithPayer);
            }
          

            return Ok(new { status = "success", message = "檢示群組內的單一費用", expenseData = expenseList });
        }


        /// <summary>
        ///
        /// 4.3編輯單筆費用
        /// </summary>
        ///<param name="expenseData">費用的資訊</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Expense/EditExpense")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> EditExpense(ExpenseVM expenseData)
        {
            //群組的成員才可看到群組內的所有費用
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());
            //判斷是否有該筆費用存在
            var hasExpenseId = db.Expenses.Where(x => x.Id == expenseData.Id).FirstOrDefault();
            if (hasExpenseId == null)
            {
                return Ok(new { status = false, message = "未有該筆費用，或是該筆費用非此群組費用!" });
            }


            //判斷user是否為群組的member
            var groupMember = db.Members.Where(x => x.GroupId == expenseData.GroupId && x.UserId == userId).FirstOrDefault();

            if (groupMember == null)
            {
                return Ok(new { status = false, message = "非群組成員無法編輯其他群組費用!" });
            }

            //編輯費用
            var editExpense = db.Expenses.Where(x => x.Id == expenseData.Id).FirstOrDefault();

            if (editExpense != null)
            {
                editExpense.ExpenseType = expenseData.ExpenseType;
                editExpense.Item = expenseData.Item;
                editExpense.Cost = expenseData.Cost;
                editExpense.CreatDate = expenseData.CreatDate;
                editExpense.Memo = expenseData.Memo;
            }

            //編輯付款人
            var editPayer = db.Payers.Where(x => x.ExpenseId == expenseData.Id).ToArray();

            double totalPayerAmount = 0;
            if (expenseData.PayerExpenseVMs.Any())
            {
                //先移除掉舊的付款人
                foreach(var i in editPayer)
                {
                    db.Payers.Remove(i);
                }
                //新增新的付款人
                foreach (var i in expenseData.PayerExpenseVMs)
                {
                    Payer payerInput = new Payer();
                    payerInput.ExpenseId =expenseData.Id;
                    payerInput.MemberId = i.MemberId;
                    payerInput.PayAmount = i.PayAmount;
                    totalPayerAmount += i.PayAmount;
                    db.Payers.Add(payerInput);
                }
            }

            //編輯分款人
            var editOwner = db.Owners.Where(x => x.ExpenseId == expenseData.Id).ToArray();

            double totalOwnerAmount = 0;
            if (expenseData.OwnerExpenseVMs.Any())
            {
                //先移除掉舊的分款人
                foreach (var i in editOwner)
                {
                    db.Owners.Remove(i);
                }
                //新增新的分款人
                foreach (var i in expenseData.OwnerExpenseVMs)
                {
                    Owner ownerInput = new Owner();
                    ownerInput.ExpenseId = expenseData.Id;
                    ownerInput.MemberId = i.MemberId;
                    ownerInput.OwnAmount = i.OwnAmount;
                    totalOwnerAmount += i.OwnAmount;
                    db.Owners.Add(ownerInput);
                }
            }

            //費用圖片編輯

            foreach (var i in expenseData.ExpenseAlbumVMs)
            {
                ExpenseAlbum expensePhotoInput = new ExpenseAlbum();
                expensePhotoInput.Name = i.Name;
                expensePhotoInput.ExpenseId = expenseData.Id;

                // 加入資料
                db.ExpenseAlbums.Add(expensePhotoInput);
            }


            //檢驗編輯後的費用與付款人和分款人金額是否一致

            if (expenseData.Cost!=totalPayerAmount)
            {
                return Ok(new { status = false, message = "編輯後的費用與付款人金額總和不一致，編輯費用失敗!" });
            }
            else if(expenseData.Cost != totalOwnerAmount)
            {
                return Ok(new { status = false, message = "編輯後的費用與分款人金額總和不一致，編輯費用失敗!" });
            }

            //通知功能

            var userName = JwtAuthUtil.GetUserName(Request.Headers.Authorization.Parameter);
            var groupName = db.Groups.Where(x => x.Id == expenseData.GroupId).Select(x => x.Name).FirstOrDefault();

            //其他群組成員也會收到通知
            var groupMemberNotification = db.Members.Where(x => x.GroupId == expenseData.GroupId).ToList();


            foreach (var i in groupMemberNotification)
            {
                if (i.UserId != userId)
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = i.UserId,
                        Information = $"{userName}於群組「{groupName}」編輯了一筆費用「{expenseData.Item}」${expenseData.Cost}元"
                    };
                    db.Notifications.Add(userInput);
                }
                else
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = userId,
                        Information = $"您於群組「{groupName}」編輯了一筆費用「{expenseData.Item}」${expenseData.Cost}元"
                    };
                    db.Notifications.Add(userInput);
                }
                
            };
            //最後一起儲存
            db.SaveChanges();

            return Ok(new { status = true, message = "編輯費用成功!"});
        }

        /// <summary>
        /// 4.1.1編輯單筆費用(更新費用圖片)
        /// </summary>

        [HttpPost]
        [Route("api/Expense/EditExpenseImage")]
        [JwtAuthFilter]

        public async Task<IHttpActionResult> EditExpenseImage()
        {
            //費用圖片新增
            // 檢查請求是否包含 multipart/form-data.
            ArrayList fileNameList = new ArrayList();
            string fileName = "";
            if (!Request.Content.IsMimeMultipartContent())
            {
                //  throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                // fileName = "defaultExpensePhoto.jpg";

            }
            else
            {
                // 讀取 MIME 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                //單圖
                if (provider.Contents.Count == 1)
                {
                    // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                    string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                    string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                    // 定義檔案名稱
                    fileName = fileNameData + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                    // 檢查資料夾是否存在，若無則建立
                    string root = HttpContext.Current.Server.MapPath("~/upload/Expense");
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory("~/upload/Expense");
                    }

                    // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                    var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                    var outputPath = Path.Combine(root, fileName);
                    using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                    }

                    fileNameList.Add(fileName);
                }


                //多圖的情況
                else if (provider.Contents.Count >= 2)
                {
                    foreach (var file in provider.Contents)
                    {
                        string fileNameData = file.Headers.ContentDisposition.FileName.Trim('\"');
                        string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.'));
                        // 定義檔案名稱
                        fileName = fileNameData + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;
                        // 檢查資料夾是否存在，若無則建立
                        string root = HttpContext.Current.Server.MapPath("~/upload/Expense");
                        if (!Directory.Exists(root))
                        {
                            Directory.CreateDirectory("~/upload/Expense");
                        }

                        // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                        var fileBytes = await file.ReadAsByteArrayAsync();
                        var outputPath = Path.Combine(root, fileName);
                        using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                        }
                        fileNameList.Add(fileName);
                    }


                }
            }

            return Ok(new { status = true, message = "編輯圖片成功!", fileNameList = fileNameList });


        }


        /// <summary>
        /// 4.4刪除單筆費用
        /// </summary>
        /// <param name="expenseId">費用的id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Expense/DeleteExpense/{expenseId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeleteExpense(int expenseId)
        {

            if (expenseId <= 0)
                return BadRequest("非有效id");

            //群組的成員才可看到群組內的所有費用
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());
            //查詢費用所屬的群組
            var group = Convert.ToInt32(db.Expenses.Where(x => x.Id == expenseId).Select(x => x.GroupId).FirstOrDefault());
            //判斷user是否為群組的member
            var groupMember = db.Members.Where(x => x.GroupId ==group  && x.UserId == userId).FirstOrDefault();

            if (groupMember == null)
            {
                return Ok(new { status = false, message = "非群組成員無法編輯其他群組費用!" });
            }

            var deleteExpense = db.Expenses.Where(x => x.Id == expenseId).FirstOrDefault();
           

            if (deleteExpense != null)
            {
                db.Expenses.Remove(deleteExpense);
            }

            //把跟該費用相關的資料一併刪除
            //刪除該筆費用付款者
            var deletePayer = db.Payers.Where(x => x.Id == expenseId);
            if (deletePayer.Any())
            {
                foreach (var p in deletePayer)
                {
                    db.Payers.Remove(p);
                }
            }
            //刪除該筆費用分款者
            var deleteOwner = db.Owners.Where(x => x.Id == expenseId);
            if (deleteOwner.Any())
            {
                foreach (var o in deleteOwner)
                {
                    db.Owners.Remove(o);
                }
            }
            //刪除該筆費用照片
            var deleteExpensePhoto = db.ExpenseAlbums.Where(x => x.Id == expenseId);
            if (deleteExpensePhoto.Any())
            {
                foreach (var i in deleteExpensePhoto)
                {
                    db.ExpenseAlbums.Remove(i);
                }
            }

            //通知功能
            var userName = JwtAuthUtil.GetUserName(Request.Headers.Authorization.Parameter);
            var groupId = db.Expenses.Where(x => x.Id == expenseId).Select(x => x.GroupId).FirstOrDefault();
            var groupName = db.Groups.Where(x => x.Id == groupId).Select(x => x.Name).FirstOrDefault();
         
            //其他群組成員也會收到通知
            var groupMemberNotification = db.Members.Where(x => x.GroupId == groupId).ToList();


            foreach (var i in groupMemberNotification)
            {
                if (i.UserId != userId)
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = i.UserId,
                        Information = $"{userName}於群組「{groupName}」刪除了一筆費用「{deleteExpense.Item}」${deleteExpense.Cost}元"
                    };
                    db.Notifications.Add(userInput);
                }
                else
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = userId,
                        Information = $"您於群組「{groupName}」刪除了一筆費用「{deleteExpense.Item}」${deleteExpense.Cost}元"
                    };
                    db.Notifications.Add(userInput);
                }

            };

            //最後一起儲存
            db.SaveChanges();

            return Ok(new { status = true, message = "刪除費用成功!" });
        }
    }
}