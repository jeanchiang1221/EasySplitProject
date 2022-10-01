using EasySplitProject.Models;
using EasySplitProject.Models.ViewModels;
using EasySplitProject.Security;
using MimeKit;
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
    /// 群組的新增、編輯、刪除
    /// </summary>
    [OpenApiTag("Group", Description = "群組的新增、編輯、刪除")]
    public class GroupController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        /// <summary>
        /// 3.1.1新增群組
        /// </summary>
        /// <param name="userData">群組建立資訊</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/AddGroup")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> AddGroup(AddGroupVM userData)
        {

            //創立群組的人的ID(登入會員的ID)
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));

            var creator= db.Users.Where(x => x.Id == userId).Select(x=>new{x.Account}).FirstOrDefault();

            // 生成群組連結驗證碼
            string groupGuid = Guid.NewGuid().ToString();


            //把群組資訊存進去

            try
            {
                Group userInput = new Group
               {
                    Name = userData.Name,
                    Image = userData.FileName,
                    CreatDate = DateTime.Now,
                    GroupGuid = groupGuid,
                    CreatorAccount = creator.Account,
                };
                // 加入資料並儲存
                db.Groups.Add(userInput);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }

            //順便把創辦人直接加到群組中，不用額外建立自己這個成員
            var groupId = Convert.ToInt32(db.Groups.Where(x => x.GroupGuid == groupGuid).Select(x=>x.Id).FirstOrDefault());

            //創立群組的人的名稱(登入會員的token)
            var userName = JwtAuthUtil.GetUserName(Request.Headers.Authorization.Parameter).ToString();

            var groupDetail = db.Groups.Where(x => x.GroupGuid == groupGuid).Select(x=>new 
            { groupId=x.Id,
              groupName=x.Name,
              groupGuid=x.GroupGuid,
              createDate=x.CreatDate,
              creatorAccount=x.CreatorAccount}).FirstOrDefault();

            try
            {
                Member userInput = new Member
                {
                    Name = userName,
                    CreatDate = DateTime.Now,
                    GroupId = groupId,
                    UserId=userId
                };
                // 加入資料並儲存
                db.Members.Add(userInput);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }


            try
            {
                Notification userInput = new Notification
                {
                    CreatDate = DateTime.Now,
                    UserId = userId,
                    Information = $"您新增了一個群組「{userData.Name}」"
                };
                // 加入資料並儲存
                db.Notifications.Add(userInput);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }


            return Ok(new { status = true, message = "新增群組成功!", groupDetail = groupDetail });
        }


        /// <summary>
        /// 3.1.2新增群組(上傳圖片)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/AddGroupCover")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> AddGroupCover()
        {
           // Photo groupCover = new Photo();

            // 檢查請求是否包含 multipart/form-data.
           string fileName;
            if (!Request.Content.IsMimeMultipartContent())
            {
                //  throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                fileName = "defaultGroupCover.jpg";

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
                string root = HttpContext.Current.Server.MapPath("~/upload/GroupCover");
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory("~/upload/GroupCover");
                }

                // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }
            }
            return Ok(new { status = true, message = "新增群組照片成功!", fileName = fileName});
        }


        /// <summary>
        /// 3.2.1取得所有群組
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Group/GetAllGroup")]
        [JwtAuthFilter]
        public IHttpActionResult ShowAllGroup()
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));

            var groupIdList = db.Members.Where(x => x.UserId == userId && x.GroupIdFK.Removed == false).OrderByDescending(x => x.CreatDate).Select(x => x.GroupId).ToList();

            ArrayList groupArrayList = new ArrayList();
            foreach (var groupId in groupIdList)
            {
                var groupList = db.Groups.Where(x => x.Id == groupId).Select(x=>new { Id=x.Id,Name=x.Name,Image=x.Image,CreateDate=x.CreatDate}).FirstOrDefault();
                var userInThisGroupMemberId = db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).Select(x=>x.Id).FirstOrDefault();
                string groupImage = "";
                if (groupList.Image == null)
                {
                    groupImage = "defaultGroupCover.jpg";
                }
                else
                {
                    groupImage = groupList.Image;
                }
                var data = new
                {
                    groupId = groupList.Id,
                    groupName = groupList.Name,
                    userInThisGroupMemberId= userInThisGroupMemberId,
                    imageUrl = "https://" + Request.RequestUri.Host + "/upload/GroupCover/" + groupImage,
                    createDate=groupList.CreateDate
                };
                groupArrayList.Add(data);
            }

            return Ok(new { status = true, message = "所有群組名稱", userId=userId,groupList = groupArrayList });
        }

        /// <summary>
        /// 3.2.2取得個別群組
        /// </summary>
        /// <param name="groupId">群組的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Group/GetAGroup/{groupId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetAGroup(int groupId)
        {
            //只可以看到自己所屬的群組
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));


            //確認使用者屬於輸入groupId的群組，
            var userInGroup = db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).FirstOrDefault();
            if (userInGroup == null)
            {
                return Ok(new { status = false, message = "非屬於登入會員的群組，不可查看群組資訊" });
            }

         
            var groupDetail = db.Users.FirstOrDefault(x => x.Id == userId).Members.Where(x => x.GroupIdFK.Id == groupId).Select(x => new
            {
                groupId = x.GroupIdFK.Id,
                groupName = x.GroupIdFK.Name,
                imageUrl = "https://" + Request.RequestUri.Host + "/upload/GroupCover/" + x.GroupIdFK.Image,
                creatDate = x.GroupIdFK.CreatDate,
                groupGuid = x.GroupIdFK.GroupGuid
            });


            return Ok(new { status = true, message = "取得個別群組資訊", groupDetail = groupDetail });
  
            
        }


        /// <summary>
        /// 3.3 發送群組邀請連結(只要是群組的成員都可以發出邀請)
        /// </summary>
        /// <param name="groupId">群組的id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/SendInviteMail/{groupId}")]
        [JwtAuthFilter]
        public IHttpActionResult SendInviteMail(int groupId)
        {
            //只要是群組的成員都可以發出邀請
            //   var userAccount =JwtAuthUtil.GetAccount(Request.Headers.Authorization.Parameter).ToString();
            //  var group = db.Groups.Where(x => x.Id == groupId && x.CreatorAccount==userAccount).FirstOrDefault();
            var group = db.Groups.Where(x => x.Id == groupId).FirstOrDefault();

            string InvitationLink = Mail.SendGroupInvitationMailLink(Request.RequestUri.Host, group.GroupGuid);
            BodyBuilder bodyBuilder = new BodyBuilder
            {
                HtmlBody =$"EasySplit-'{group.Name}'-群組邀請連結:" +
                           $"請點選以下連結加入群組!開始一起拆帳趣~!" +
                          $"{InvitationLink}"
            };

            return Ok(new { status = true, message = "產生邀請連結成功!", inviation= bodyBuilder });
        }

        /// <summary>
        /// 3.3.1加入群組 - 取得加入群組資訊
        /// </summary>
        /// <param name="groupGuid">群組的guid</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Group/AuthMail/GroupInvitation/{groupGuid}")]
        [JwtAuthFilter]
        public IHttpActionResult ShowGroupInvitation(string groupGuid)
        {
            var groupName = db.Groups.Where(x => x.GroupGuid == groupGuid).Select(x => new
            {
                groupId=x.Id,
                groupName = x.Name,
                groupCreator=x.CreatorAccount

            });
            var members = db.Members.Where(x => x.GroupIdFK.GroupGuid == groupGuid).Select(x => new
            {
                memberId=x.Id,
                memberName = x.Name

            });

            return Ok(new { status = true, message = "顯示群組資訊成功!", group=groupName, member = members });
        }

        /// <summary>
        /// 3.3.2加入群組-認領完成員後綁定成員身分
        /// </summary>
     
        /// <param name="memberId">群組的成員(Member)id</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Group/SelectMember/{memberId}")]
        [JwtAuthFilter]
        public IHttpActionResult SelectMember(int memberId)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));

            //尚未被認領身分的成員
            var memberNotYetBind = db.Members.Where(x => x.Id == memberId && x.UserId==null).FirstOrDefault();

            //已被認領的成員
            var memberAlreadyBind = db.Members.Where(x => x.Id == memberId && x.UserId != null).FirstOrDefault();

            //如果已是群組成員
            var memberIsUserItself= db.Members.Where(x => x.Id == memberId && x.UserId == userId).FirstOrDefault();

            if (memberNotYetBind != null)
            {
                memberNotYetBind.UserId = userId;
                db.SaveChanges();
                var newMember = new
                {
                    name = memberNotYetBind.Name
                };
                return Ok(new { status = true, message = "會員綁定群組資訊成功!", member = newMember });
            }

            if (memberIsUserItself != null)
            {
                var newMember = new
                {
                    name = memberIsUserItself.Name
                };
                return Ok(new { status = false, message = $"您已是群組成員，成員名稱為{newMember}" });
            }

            if (memberAlreadyBind != null)
            {
                var newMember = new
                {
                    name = memberAlreadyBind.Name
                };
                return Ok(new { status = true, message = "目前已被認領的成員名單", member = newMember });
            }

       

            return Ok(new { status = false, message = "會員綁定失敗!" });
        }


        /// <summary>
        /// 3.4.1編輯群組(群組成員皆可以編輯)
        /// </summary>
        /// <param name="userData">群組修改後的資訊</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Group/EditGroup")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> EditGroup(GroupVM userData)
        {

            //群組的成員才可編輯
             var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());
           //判斷user是否為群組的member
            var member = db.Members.Where(x => x.GroupId ==userData.Id && x.UserId==userId).FirstOrDefault();
            var group = db.Groups.Where(x => x.Id ==userData.Id).FirstOrDefault();

            if (member == null)
            {
                return Ok(new { status =false, message = "非群組成員無法編輯!" });
            }

            if (Convert.ToInt32(userData.Id) <= 0)
                return BadRequest("非有效id");


            if (group != null&& userData.FileName!="")
            {
                group.Name = userData.Name;
                group.Image = userData.FileName;
                db.SaveChanges();
            }
            else if (group != null && userData.FileName == "")
            {
                group.Name = userData.Name;
                db.SaveChanges();
            }

            return Ok(new { status = true, message = "編輯群組資訊成功!" });
        }

        /// <summary>
        /// 3.4.1.1編輯群組(更新圖片)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/EditGroupCover")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> EditGroupCover()
        {
            // 檢查請求是否包含 multipart/form-data.
            string fileName;
            if (!Request.Content.IsMimeMultipartContent())
            {
                //  throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                fileName = "";

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

            return Ok(new { status = true, message = "重新上傳群組圖片成功!"  ,fileName = fileName });
        }




        /// <summary>
        /// 3.4.2刪除群組(只有創立者才可以刪群組 and 非真的刪除只是紀錄成為刪除)
        /// </summary>
        /// <param name="groupId">群組的id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Group/DeleteGroup/{groupId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeleteGroup(int groupId)
        {
            
            if (groupId <= 0)
                return BadRequest("非有效id");
            //只有創立者才可以刪群組
            var userAccount = JwtAuthUtil.GetAccount(Request.Headers.Authorization.Parameter).ToString();
            var group = db.Groups.Where(x => x.Id == groupId && x.CreatorAccount == userAccount).FirstOrDefault();


            if (group != null)
            {
                group.Removed =true;
                db.SaveChanges();
            }

            return Ok(new { status = true, message = "刪除群組成功!" });
        }




        /// <summary>
        ///3.5.1取得群組成員
        /// </summary>
        /// <param name="groupId">群組的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Group/GetAllMember/{groupId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetAllGroupMember(int groupId)
        {
            var members = db.Members.Where(x => x.GroupIdFK.Id == groupId && x.RemovedFromGroup==false).ToArray();

            ArrayList memberData = new ArrayList();

            foreach (var i in members)
            {
                if (i.UserId != null)
                {
                    var data = new
                    {
                        memberId = i.Id,
                        memberName = i.Name,
                        userId = i.UserId,
                        imageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + i.UserIdFK.Image,
                        email=i.UserIdFK.Account
                    };
                    memberData.Add(data);
                }
                else//還沒有綁定會員的member就放預設圖片
                {
                    var data = new
                    {
                        memberId = i.Id,
                        memberName = i.Name,
                        userId = i.UserId,
                        imageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/defaultProfile.jpg",
                        email = ""
                    };
                     memberData.Add(data);
                }
              
            }

            return Ok(new { status = true, message = "顯示群組成員成功!", memberData = memberData });
        }


        /// <summary>
        /// 3.5.2新增成員
        /// </summary>
        /// <param name="userData">新增成員的資料</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/AddMember")]
        [JwtAuthFilter]
        public IHttpActionResult AddGroupMember(MemberVM userData)
        {
            try
            {
                Member userInput = new Member
                {
                    Name = userData.Name,
                    CreatDate = DateTime.Now,
                    GroupId = userData.GroupId
                };
                // 加入資料並儲存
                db.Members.Add(userInput);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }

            
            //通知功能
            
            //登入會員的ID
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var userName = JwtAuthUtil.GetUserName(Request.Headers.Authorization.Parameter);
            var groupName = db.Groups.Where(x => x.Id == userData.GroupId).Select(x=>x.Name).FirstOrDefault();

            //其他群組成員也會收到通知
            var groupMember = db.Members.Where(x => x.GroupId == userData.GroupId).ToList();


            foreach (var i in groupMember)
            {
                if (i.UserId != userId)
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = i.UserId,
                        Information = $"{userName}於群組「{groupName}」新增了一個成員「{userData.Name}」"
                    };
                    db.Notifications.Add(userInput);
                }
                else
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = userId,
                        Information = $"您於群組「{groupName}」新增了一個成員「{userData.Name}」"
                    };
                    db.Notifications.Add(userInput);
                }

            };
            db.SaveChanges();




            return Ok(new { status = true, message = "新增群組成員成功!" });
        }

        /// <summary>
        /// 3.5.3編輯成員角色(可以換角色)
        /// </summary>
        /// <param name="memberId">要變更成為的成員的id</param>
      
        /// <returns></returns>
        [HttpPut]
        [Route("api/Group/ChangeMemberRole/{memberId}")]
        [JwtAuthFilter]
        public IHttpActionResult ChangeGroupMember(int memberId)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());

            //確認使用者屬於輸入memberId的群組，不可以輸入到其他群組的memberId
            var memberInGroup = db.Members.Where(x => x.Id == memberId).Select(x => x.GroupId).FirstOrDefault();
            var userInGroup = db.Members.Where(x => x.GroupId == memberInGroup && x.UserId == userId).FirstOrDefault();
            if (userInGroup==null)  
            {
                return Ok(new { satus = false, message = "輸入會員編號非屬於此群組，無法變更!" });
            }


            //先找到原本的user他認領的成員，並把原本的userId拿掉
            var userOriginalMember = db.Members.Where(x => x.UserId == userId && x.GroupId== memberInGroup).FirstOrDefault();
            if (userOriginalMember != null)
            {
                userOriginalMember.UserId = null;
                db.SaveChanges();
            }


            //要變更的member，寫入userId
            var member = db.Members.Where(x => x.Id == memberId).FirstOrDefault();

            if (member.UserId != null)
            {
                return Ok(new { satus = false, message = "要變更的成員已綁定會員，無法變更!" });
            }


            if (member != null && member.UserId==null)
            {
                member.UserId =userId ;
                db.SaveChanges();
            }

            if (member == null)
            {
                return Ok(new { satus = false, message = "輸入會員編號非正確，無法變更!" });
            }
       


            return Ok(new { status = true, message = "變更成員角色成功!" });
        }


        /// <summary>
        /// 3.5.4編輯成員資料
        /// </summary>
        /// <param name="userData">修改後的成員資料</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Group/EditMember")]
        [JwtAuthFilter]
        public IHttpActionResult EditGroupMember(MemberEditVM userData)
        {

            //判斷user所在的群組
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());   
            var groupIdList = db.Members.Where(x => x.UserId == userId).Select(x => x.GroupId).ToList();
            
        
            //找到要變更的member
            var member = db.Members.Where(x => x.Id == userData.MemberId).FirstOrDefault();

            //判斷是群組的成員才可編輯
            int userInGroup = 0;
            foreach(var g in groupIdList)
            {
                if (member.GroupId == g)
                {
                    userInGroup++;
                }
            }

            if (userInGroup==0)
            {
                return Ok(new { status = false, message = "非群組成員無法編輯!" });
            }

            //找到要變更的member
            if (member != null)
            {
                
                member.Name = userData.EditedName;
                db.SaveChanges();
                return Ok(new { status = true, message = "變更成員資料成功!" });
            }


            return Ok(new { status = false, message = "變更成員資料失敗! 沒有該會員資料" });
        }


        /// <summary>
        /// 3.5.4刪除成員(只有群組內的會員才可以刪會員 and 費用要結清才可以刪)
        /// </summary>
        /// <param name="memberId">成員的id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Group/DeleteMember/{memberId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeleteGroupMember(int memberId)
        {

            if (memberId <= 0)
                return BadRequest("非有效id");

            //var hasMember=db.Members.Where(x=)

            //判斷user所在的群組
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());
            var groupId = Convert.ToInt32(db.Members.Where(x =>x.Id==memberId && x.UserId == userId).Select(x => x.GroupId).FirstOrDefault());

            //判斷群組是否只剩下一位成員
            var CheckLastMember = db.Members.Where(x => x.GroupId == groupId&& x.RemovedFromGroup==false).ToList();
            if (CheckLastMember.Count() == 1)
            {
                return Ok(new { status = false, message = "群組只剩你一個人了不要再刪了嗚嗚!" });
            }


            //找到要變更的member
            var member = db.Members.Where(x => x.Id == memberId).FirstOrDefault();


            //如果從來沒有參與過任何費用，可以直接刪掉
            var neverBeenPayer = db.Payers.Where(x => x.MemberId == memberId).FirstOrDefault();
            var neverBeenOwner = db.Owners.Where(x => x.MemberId == memberId).FirstOrDefault();
            if (neverBeenPayer == null&& neverBeenOwner == null)
            {
                db.Members.Remove(member);
                db.SaveChanges();
                return Ok(new { status = true, message = "刪除成員成功!" });
            }



            //檢視費用是否已經結清，結清才可以刪除成員

            var paidMoneyList = db.Payers.Where(x => x.MemberId == memberId).Select(x => x.PayAmount).ToArray();
            var ownedMoneyList = db.Owners.Where(x => x.MemberId == memberId).Select(x => x.OwnAmount).ToArray();
            double paidMoney = 0;
            double ownedMoney = 0;
            foreach(var i in paidMoneyList)
            {
                paidMoney += i;
            }
            foreach (var i in ownedMoneyList)
            {
                ownedMoney += i;
            }

            double balanceMoney = paidMoney - ownedMoney;

            double balancSettledMoney = 0;
            if (balanceMoney > 0)
            {
             
                var payerSettled = db.Settlements.Where(x => x.PayerMemberId == memberId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                foreach(var i in payerSettled)
                {
                    balancSettledMoney += i;
                }

                if (balanceMoney == balancSettledMoney)
                {
                    //因為參與過費用會留下費用紀錄，非真的刪除會員，而是紀錄被移除成員
                    member.RemovedFromGroup = true;
                    db.SaveChanges();
                    return Ok(new { status = true, message = "刪除成員成功!" });
                }
                else
                {
                    return Ok(new { status = false, message = "成員尚有未結清費用，無法刪除!" });
                }


            }else if(balanceMoney<0)
            {
                var ownerSettled = db.Settlements.Where(x => x.OwnerMemberId == memberId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                foreach (var i in ownerSettled)
                {
                    balancSettledMoney += i;
                }

                if (balanceMoney*-1 == balancSettledMoney)
                {
                    //因為參與過費用會留下費用紀錄，非真的刪除會員，而是紀錄被移除成員
                    member.RemovedFromGroup = true;
                    db.SaveChanges();
                    return Ok(new { status = true, message = "刪除成員成功!" });
                }
                else
                {
                    return Ok(new { status = false, message = "成員尚有未結清費用，無法刪除!" });
                }
            }
            else
            {
                //因為參與過費用會留下費用紀錄，非真的刪除會員，而是紀錄被移除成員
                member.RemovedFromGroup = true;
                db.SaveChanges();
                return Ok(new { status = true, message = "刪除成員成功!" });
            }

        }

        /// <summary>
        ///3.6 取得成員的會員資料
        /// </summary>
        /// <param name="memberId">成員的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Group/GetMemberInfo/{memberId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetMemberInfo(int memberId)
        {
            //找到綁定成員的會員Id

            var memberUserId = Convert.ToInt32(db.Members.Where(x => x.Id == memberId).Select(x => x.UserId).FirstOrDefault());

            if (memberUserId == 0)
            {
                return Ok(new { status = true, message = "該成員尚未綁定會員id，無法查看會員資訊!" });
            };

            var memberDetail = db.Users.Where(x => x.Id== memberUserId).Select(x => new
            {//這邊打錯打成userID了
                memberId = x.Id,
                memberName = x.Name,
                memberAcount=x.Account,
                memberImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + x.Image,

            });
        
            var paymentBank = db.PaymentBanks.Where(x => x.UserId == memberUserId).Select(x=> new { 
                bankAccountName=x.AccountName,
                bankName=x.Bank,
                bankCode=x.BankCode,
                bankAccount=x.Account
            });

            var paymentCash = db.PaymentCashs.Where(x => x.UserId == memberUserId).Select(x => new {
                phone = x.Phone,
                method = x.Method,
            });

            var paymentLine = db.PaymentLines.Where(x => x.UserId == memberUserId).Select(x => new {
                name=x.Name,
                lineId=x.LineID,
                phone = x.Phone,
                qrCodeUrl = "https://" + Request.RequestUri.Host + "/upload/Payment/" + x.QRCode,
            });

            return Ok(new { status = true, message = "顯示群組成員的會員資訊及收款成功!", memberDetail = memberDetail, paymentBank=paymentBank, paymentCash=paymentCash, paymentLine= paymentLine });
        }


    }
}

