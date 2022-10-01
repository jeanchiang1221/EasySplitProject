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
{  /// <summary>
   /// 結清的檢視/新增/刪除
   /// </summary>
    [OpenApiTag("Settlement", Description = "結清的檢視/新增/刪除")]
    public class SettlementController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        /// <summary>
        /// 5.1.1 檢視所有待結算金額(尚未結清的費用)
        /// </summary>
        /// <param name="groupId">群組的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Settlement/GetAllSettlement/{groupId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetAllSettlement(int groupId)
        {
            //先檢視群組是否有費用
            var hasExpense = db.Expenses.Where(x => x.GroupId == groupId).FirstOrDefault();
            if (hasExpense == null)
            {
                return Ok(new
                {
                    status = true,
                    message = "群組不存在或目前群組尚未有任何費用!"
                });
            }

            //檢視是否有該群組
            //確認使用者屬於輸入groupId的群組，
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var userInGroup = db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).FirstOrDefault();
            if (userInGroup == null)
            {
                return Ok(new { status = false, message = "非屬於登入會員的群組" });
            }

            //判斷群組內有沒有成員
            var hasMember = db.Members.Where(x => x.GroupId == groupId).FirstOrDefault();
            if (hasMember == null)
            {
                return Ok(new { status = true, message = "群組尚無任何成員" });
            }


            //先取得群組所有成員的id
            var members = db.Members.Where(x => x.GroupIdFK.Id == groupId).Select(x => x.Id).ToArray();

            List<PayerListVM> payerList = new List<PayerListVM>();
            List<OwnerListVM> ownerList = new List<OwnerListVM>();

            ArrayList settlementList = new ArrayList();
    
            List<NotInvolvedListVM> notInvolvedList = new List<NotInvolvedListVM>();
            
            foreach (var memberId in members)
            {

                var payAmount = db.Payers.Where(x => x.MemberId == memberId).Select(x => x.PayAmount).ToArray();
                double allPaidMoney = 0;
                foreach (var paidMoney in payAmount)
                {
                    allPaidMoney += paidMoney;
                }

                var payerReceived = db.Settlements.Where(x => x.PayerMemberId == memberId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allPayerReceivedMoney = 0;
                foreach (var ownerPaytoPayerAmount in payerReceived)
                {
                    allPayerReceivedMoney += ownerPaytoPayerAmount;
                }

                //要扣掉已經收款的款項
                allPaidMoney -= allPayerReceivedMoney;


                var ownAmount = db.Owners.Where(x => x.MemberId == memberId).Select(x => x.OwnAmount).ToArray();
                double allOwnedMoney = 0;
                foreach (var ownedMoney in ownAmount)
                {
                    allOwnedMoney += ownedMoney;
                }
                var ownerPaid = db.Settlements.Where(x => x.OwnerMemberId == memberId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allOwnerPaidMoney = 0;
                foreach (var ownerPaytoPayerAmount in ownerPaid)
                {
                    allOwnerPaidMoney += ownerPaytoPayerAmount;
                }
                //要扣掉已經給過的款項
                allOwnedMoney -= allOwnerPaidMoney;

                double resultBalance = allPaidMoney - allOwnedMoney;

                var memberName = db.Members.Where(x => x.Id == memberId).Select(x => x.Name).FirstOrDefault();
                var memberImage= db.Members.Where(x => x.Id == memberId).Select(x => x.UserIdFK.Image).FirstOrDefault();
               

                if (memberImage == null)
                {
                    memberImage = "defaultprofile.jpg";
                }
             
                if (resultBalance > 0)
                {
                    var resultdata = new PayerListVM
                    {
                       
                        MemberId = memberId,
                        PayerName = memberName,
                        PayAmount = resultBalance,
                        PayerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + memberImage
                    };
                    payerList.Add(resultdata);
                }
                else if (resultBalance < 0)
                {
                   // resultBalance = Math.Abs(resultBalance);
                    var resultdata = new OwnerListVM
                    {
                        MemberId = memberId,
                        OwnerName= memberName,
                        OwnAmount = resultBalance*-1,
                        OwnerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + memberImage
                    };
                    ownerList.Add(resultdata);
                }
                else
                {
                    //如果resultBalance=就是已經結清or沒有參與費用的成員
                    var resultdata = new NotInvolvedListVM  
                    {
                        MemberId = memberId,
                        Name= memberName,
                        MemberImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + memberImage
                    };
                    notInvolvedList.Add(resultdata);
                }
            }

            StringBuilder builder = new StringBuilder();

            if(payerList.Count()==0&& ownerList.Count == 0)
            {
                return Ok(new
                {
                    status = true,
                    message = "目前群組尚未有任何待結清的費用!"
                });
            }

            payerList = payerList.OrderByDescending(x => x.PayAmount).ToList();
            ownerList = ownerList.OrderByDescending(x => x.OwnAmount).ToList();

            //如果最大的欠款金額(ownerAmount)超過付款者的金額(payerAmount)，就用由小到大的排序
            if (payerList[0].PayAmount < ownerList[0].OwnAmount)
            {
                payerList = payerList.OrderBy(x => x.PayAmount).ToList();
                ownerList = ownerList.OrderBy(x => x.OwnAmount).ToList();
            }

            for (int n = 0; n < payerList.Count; n++)
            {
                int payerMemberId = payerList[n].MemberId;
                double payAmountresult = payerList[n].PayAmount;
                double receivedAmount = payerList[n].ReceivedAmount;

                if (payAmountresult - receivedAmount > 0)
                {
                    for (int i = 0; i < ownerList.Count; i++)
                    {
                        int ownerMemberId = ownerList[i].MemberId;
                        double ownAmountresult = ownerList[i].OwnAmount;
                        double gaveAmount = ownerList[i].GaveAmount;

                        var payerName = db.Members.Where(x => x.Id == payerMemberId).Select(x => x.Name).FirstOrDefault();
                        var payerImage = db.Members.Where(x => x.Id == payerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();
                        var ownerName = db.Members.Where(x => x.Id == ownerMemberId).Select(x => x.Name).FirstOrDefault();
                        var ownerImage = db.Members.Where(x => x.Id == ownerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();

                        if (payerImage == null && ownerImage == null)
                        {
                            payerImage = "defaultprofile.jpg";
                            ownerImage = "defaultprofile.jpg";
                        }
                        else if(payerImage == null)
                        {
                            payerImage = "defaultprofile.jpg";
                        }
                        else if (ownerImage == null)
                        {
                            ownerImage = "defaultprofile.jpg";
                        };
                        
                        if(ownAmountresult - gaveAmount == 0) { }

                        else if (ownAmountresult - gaveAmount > 0)
                        {
                            double notEnough = payerList[n].PayAmount - payerList[n].ReceivedAmount;
                            
                         

                            if (notEnough == 0) {}//已經沒有不夠的錢了，就不用再做任何動作

                            else if (notEnough >= ownAmountresult)
                            {
                                receivedAmount += (ownAmountresult - gaveAmount);
                                double ownerShouldPay = (ownAmountresult - gaveAmount);
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += (ownAmountresult - gaveAmount);                              
                                ownerList[i].GaveAmount = gaveAmount;

                                var data = new
                                {
                                    ownerMemberId = ownerMemberId,
                                    owenerName = ownerName,
                                    ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                    ownAmountresult = ownerShouldPay,
                                    payerMemberId = payerMemberId,
                                    payerName = payerName,
                                    payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                    status = $"{ownerName}需支付{ownerShouldPay}元給{payerName}"
                                };

                                settlementList.Add(data);

                            }

                            else if (notEnough < ownAmountresult )
                            {
                                double ownAmountresultPart1 = notEnough;
                                receivedAmount += ownAmountresultPart1;
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += ownAmountresultPart1;
                                ownerList[i].GaveAmount = gaveAmount;


                                var data = new
                                {
                                    ownerMemberId = ownerMemberId,
                                    owenerName = ownerName,
                                    ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                    ownAmountresult = ownAmountresultPart1,
                                    payerMemberId = payerMemberId,
                                    payerName = payerName,
                                    payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                    status = $"{ownerName}需支付{ownAmountresultPart1}元給{payerName}"
                                };

                                settlementList.Add(data);

                             

                            }
                        }
                     
                    }
                }
            }


            return Ok(new
            {
                status = true,
                message = "檢視所有待結算金額(尚未結清的費用)成功!",
                settlementList = settlementList,
                payerList = payerList,
                ownerList = ownerList,
                notInvolvedList = notInvolvedList
            });

        }

        /// <summary>
        /// 5.1.2 檢視個人待結算金額(尚未結清的費用)
        /// </summary>
        /// <param name="memberId">成員的memberId</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Settlement/GetSettlement/{memberId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetSettlement(int memberId)
        {
           //先找到member所屬的group
            var groupId = db.Members.Where(x => x.Id == memberId).Select(x => x.GroupIdFK.Id).FirstOrDefault();
            
            //取得群組所有成員的id
            var members = db.Members.Where(x => x.GroupIdFK.Id == groupId).Select(x => x.Id).ToArray();



            List<PayerListVM> payerList = new List<PayerListVM>();
            List<OwnerListVM> ownerList = new List<OwnerListVM>();
            ArrayList settlement = new ArrayList();//個人的待結清狀況

            List<NotInvolvedListVM> notInvolvedList = new List<NotInvolvedListVM>();
            ArrayList notInvolved = new ArrayList();//個人的待結清狀況
           

            foreach (var mId in members)
            {
                var PayerReceived = db.Settlements.Where(x => x.PayerMemberId == mId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allPayerReceivedMoney = 0;
                foreach (var ownerPaytoPayerAmount in PayerReceived)
                {
                    allPayerReceivedMoney += ownerPaytoPayerAmount;
                }

                var payAmount = db.Payers.Where(x => x.MemberId == mId).Select(x => x.PayAmount).ToArray();
                double allPaidMoney = 0;
                foreach (var paidMoney in payAmount)
                {
                    allPaidMoney += paidMoney;
                }
                //要扣掉已經收款的款項
                allPaidMoney -= allPayerReceivedMoney;

                var OwnerPaid = db.Settlements.Where(x => x.OwnerMemberId == mId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allOwnerPaidMoney = 0;
                foreach (var ownerPaytoPayerAmount in OwnerPaid)
                {
                    allOwnerPaidMoney += ownerPaytoPayerAmount;
                }


                var ownAmount = db.Owners.Where(x => x.MemberId == mId).Select(x => x.OwnAmount).ToArray();
                double allOwnedMoney = 0;
                foreach (var ownedMoney in ownAmount)
                {
                    allOwnedMoney += ownedMoney;
                }

                allOwnedMoney -= allOwnerPaidMoney;

                double resultBalance = allPaidMoney - allOwnedMoney;

                var memberName = db.Members.Where(x => x.Id == memberId).Select(x => x.Name).FirstOrDefault();

                if (resultBalance > 0)
                {
                    var resultdata = new PayerListVM
                    {
                        PayAmount = resultBalance,
                        MemberId = mId
                    };
                    payerList.Add(resultdata);
                }
                else if (resultBalance < 0)
                {
                    resultBalance = Math.Abs(resultBalance);
                    var resultdata = new OwnerListVM
                    {
                        OwnAmount = resultBalance,
                        MemberId = mId
                    };
                    ownerList.Add(resultdata);
                }
                else
                {
                    //如果resultBalance=就是已經結清or沒有參與費用的成員
                    var resultdata = new NotInvolvedListVM
                    {
                        MemberId = mId,
                        Name= memberName
                    };
                    notInvolvedList.Add(resultdata);
                }
            }

            payerList = payerList.OrderByDescending(x => x.PayAmount).ToList();
            ownerList = ownerList.OrderByDescending(x => x.OwnAmount).ToList();

            //如果最大的欠款金額(ownerAmount)超過付款者的金額(payerAmount)，就用由小到大的排序
            if (payerList[0].PayAmount < ownerList[0].OwnAmount)
            {
                payerList = payerList.OrderBy(x => x.PayAmount).ToList();
                ownerList = ownerList.OrderBy(x => x.OwnAmount).ToList();
            }

            for (int n = 0; n < payerList.Count; n++)
            {
                int payerMemberId = payerList[n].MemberId;
                double payAmountresult = payerList[n].PayAmount;
                double receivedAmount = payerList[n].ReceivedAmount;

                if (payAmountresult - receivedAmount > 0)
                {
                    for (int i = 0; i < ownerList.Count; i++)
                    {
                        int ownerMemberId = ownerList[i].MemberId;
                        double ownAmountresult = ownerList[i].OwnAmount;
                        double gaveAmount = ownerList[i].GaveAmount;

                        var payerName = db.Members.Where(x => x.Id == payerMemberId).Select(x => x.Name).FirstOrDefault();
                        var payerImage = db.Members.Where(x => x.Id == payerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();
                        var ownerName = db.Members.Where(x => x.Id == ownerMemberId).Select(x => x.Name).FirstOrDefault();
                        var ownerImage = db.Members.Where(x => x.Id == ownerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();

                        if (payerImage == null && ownerImage == null)
                        {
                            payerImage = "defaultprofile.jpg";
                            ownerImage = "defaultprofile.jpg";
                        }
                        else if (payerImage == null)
                        {
                            payerImage = "defaultprofile.jpg";
                        }
                        else if (ownerImage == null)
                        {
                            ownerImage = "defaultprofile.jpg";
                        };

                        if (ownAmountresult - gaveAmount > 0)
                        {
                            double notEnough = payerList[n].PayAmount - payerList[n].ReceivedAmount;

                            if (notEnough == 0) { }//已經沒有不夠的錢了，就不用再做任何動作
                            else if (notEnough >= ownAmountresult)
                            {
                                receivedAmount += (ownAmountresult - gaveAmount);
                                double ownerShouldPay = (ownAmountresult - gaveAmount);
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += (ownAmountresult - gaveAmount);
                                ownerList[i].GaveAmount = gaveAmount;

                                if (ownerMemberId == memberId)
                                {
                                    var data = new
                                    {
                                        ownerMemberId = ownerMemberId,
                                        owenerName = ownerName,
                                        ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                        ownAmountresult = ownerShouldPay,
                                        payerMemberId = payerMemberId,
                                        payerName = payerName,
                                        payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                        status = $"{ownerName}需支付{ownerShouldPay}元給{payerName}"
                                    };
                                    settlement.Add(data);
                                }
                                else if (payerMemberId == memberId) {

                                    var data = new
                                    {
                                        ownerMemberId = ownerMemberId,
                                        owenerName = ownerName,
                                        ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                        ownAmountresult = ownerShouldPay,
                                        payerMemberId = payerMemberId,
                                        payerName = payerName,
                                        payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                        status = $"{payerName}須向{ownerName}取回{ ownerShouldPay}元"
                                    };

                                    settlement.Add(data);
                                }

                               
                            }
                            else if (notEnough < ownAmountresult)
                            {
                                double ownAmountresultPart1 = notEnough;
                                receivedAmount += ownAmountresultPart1;
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += ownAmountresultPart1;
                                ownerList[i].GaveAmount = gaveAmount;

                                if (ownerMemberId == memberId)
                                {
                                    var data = new
                                    {
                                        ownerMemberId = ownerMemberId,
                                        owenerName = ownerName,
                                        ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                        ownAmountresult = ownAmountresultPart1,
                                        payerMemberId = payerMemberId,
                                        payerName = payerName,
                                        payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                        status = $"{ownerName}需支付{ownAmountresultPart1}元給{payerName}"
                                    };

                                    settlement.Add(data);
                                }
                                else if (payerMemberId == memberId)
                                {

                                    var data = new
                                    {
                                        ownerMemberId = ownerMemberId,
                                        owenerName = ownerName,
                                        ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                        ownAmountresult = ownAmountresultPart1,
                                        payerMemberId = payerMemberId,
                                        payerName = payerName,
                                        payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                        status = $"{payerName}須向{ownerName}取回{ownAmountresultPart1}元"
                                    };

                                    settlement.Add(data);
                                }
                            }
                        }
                    }
                }
            }

    
            notInvolvedList.ToArray();

            foreach (var i in notInvolvedList)
            {
                if (i.MemberId == memberId)
                {
                    var resultdata = new 
                    {
                        MemberId = i.MemberId,
                        Name = i.Name,
                        MemberImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + i.MemberImageUrl,
                        status= $"{i.Name}無待結清費用"
                    };
                    notInvolved.Add(resultdata);

                }
            }


            return Ok(new
            {
                status = true,
                message = "檢視個人待結算金額(尚未結清的費用)成功!",
                settlement = settlement,
                notInvolved = notInvolved
            });

        }

        /// <summary>
        /// 5.1.3 檢視個人(登入會員本人)待結算金額(尚未結清的費用)
        /// </summary>
       /// <param name="groupId">群組的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Settlement/GetSelfSettlement/{groupId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetSelfSettlement(int groupId)
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var userName = JwtAuthUtil.GetUserName(Request.Headers.Authorization.Parameter);

            var userMemberId = db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).Select(x => x.Id).FirstOrDefault();

            //確認使用者屬於輸入groupId的群組，
            var userInGroup = db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).FirstOrDefault();
            if (userInGroup == null)
            {
                return Ok(new { status = false, message = "非屬於登入會員的群組" });
            }

            //先判斷有沒有群組有沒有產生過費用
            var hasExpense = db.Expenses.Where(x => x.GroupId == groupId).FirstOrDefault();
            if (hasExpense == null)
            {
                return Ok(new { status = true, message = "群組尚無任何費用紀錄" });
            }

            //判斷群組內有沒有成員
            var hasMember = db.Members.Where(x => x.GroupId == groupId).FirstOrDefault();
            if (hasMember == null)
            {
                return Ok(new { status = true, message = "群組尚無任何成員" });
            }


            //取得群組所有成員的id
            var members = db.Members.Where(x => x.GroupIdFK.Id == groupId).Select(x => x.Id).ToArray();


            List<PayerListVM> payerList = new List<PayerListVM>();
            List<OwnerListVM> ownerList = new List<OwnerListVM>();
            ArrayList settlement = new ArrayList();//個人的待結清狀況

            List<NotInvolvedListVM> notInvolvedList = new List<NotInvolvedListVM>();
            ArrayList notInvolved = new ArrayList();//個人的待結清狀況


            foreach (var mId in members)
            {
                var PayerReceived = db.Settlements.Where(x => x.PayerMemberId == mId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allPayerReceivedMoney = 0;
                foreach (var ownerPaytoPayerAmount in PayerReceived)
                {
                    allPayerReceivedMoney += ownerPaytoPayerAmount;
                }

                var payAmount = db.Payers.Where(x => x.MemberId == mId).Select(x => x.PayAmount).ToArray();
                double allPaidMoney = 0;
                foreach (var paidMoney in payAmount)
                {
                    allPaidMoney += paidMoney;
                }
                //要扣掉已經收款的款項
                allPaidMoney -= allPayerReceivedMoney;

                var OwnerPaid = db.Settlements.Where(x => x.OwnerMemberId == mId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allOwnerPaidMoney = 0;
                foreach (var ownerPaytoPayerAmount in OwnerPaid)
                {
                    allOwnerPaidMoney += ownerPaytoPayerAmount;
                }


                var ownAmount = db.Owners.Where(x => x.MemberId == mId).Select(x => x.OwnAmount).ToArray();
                double allOwnedMoney = 0;
                foreach (var ownedMoney in ownAmount)
                {
                    allOwnedMoney += ownedMoney;
                }

                allOwnedMoney -= allOwnerPaidMoney;

                double resultBalance = allPaidMoney - allOwnedMoney;

                var memberName = db.Members.Where(x => x.Id == userMemberId).Select(x => x.Name).FirstOrDefault();

                if (resultBalance > 0)
                {
                    var resultdata = new PayerListVM
                    {
                        PayAmount = resultBalance,
                        MemberId = mId
                    };
                    payerList.Add(resultdata);
                }
                else if (resultBalance < 0)
                {
                    resultBalance = Math.Abs(resultBalance);
                    var resultdata = new OwnerListVM
                    {
                        OwnAmount = resultBalance,
                        MemberId = mId
                    };
                    ownerList.Add(resultdata);
                }
                else
                {
                    //如果resultBalance=就是已經結清or沒有參與費用的成員
                    var resultdata = new NotInvolvedListVM
                    {
                        MemberId = mId,
                        Name = memberName
                    };
                    notInvolvedList.Add(resultdata);
                }
            }

            payerList = payerList.OrderByDescending(x => x.PayAmount).ToList();
            ownerList = ownerList.OrderByDescending(x => x.OwnAmount).ToList();

            //如果最大的欠款金額(ownerAmount)超過付款者的金額(payerAmount)，就用由小到大的排序
            if (payerList[0].PayAmount < ownerList[0].OwnAmount)
            {
                payerList = payerList.OrderBy(x => x.PayAmount).ToList();
                ownerList = ownerList.OrderBy(x => x.OwnAmount).ToList();
            }

            for (int n = 0; n < payerList.Count; n++)
            {
                int payerMemberId = payerList[n].MemberId;
                double payAmountresult = payerList[n].PayAmount;
                double receivedAmount = payerList[n].ReceivedAmount;

                if (payAmountresult - receivedAmount > 0)
                {
                    for (int i = 0; i < ownerList.Count; i++)
                    {
                        int ownerMemberId = ownerList[i].MemberId;
                        double ownAmountresult = ownerList[i].OwnAmount;
                        double gaveAmount = ownerList[i].GaveAmount;

                        var payerName = db.Members.Where(x => x.Id == payerMemberId).Select(x => x.Name).FirstOrDefault();
                        var payerImage = db.Members.Where(x => x.Id == payerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();
                        var ownerName = db.Members.Where(x => x.Id == ownerMemberId).Select(x => x.Name).FirstOrDefault();
                        var ownerImage = db.Members.Where(x => x.Id == ownerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();

                        if (payerImage == null && ownerImage == null)
                        {
                            payerImage = "defaultprofile.jpg";
                            ownerImage = "defaultprofile.jpg";
                        }
                        else if (payerImage == null)
                        {
                            payerImage = "defaultprofile.jpg";
                        }
                        else if (ownerImage == null)
                        {
                            ownerImage = "defaultprofile.jpg";
                        };

                        if (ownAmountresult - gaveAmount > 0)
                        {
                            double notEnough = payerList[n].PayAmount - payerList[n].ReceivedAmount;

                            if (notEnough == 0) { }//已經沒有不夠的錢了，就不用再做任何動作
                            else if (notEnough >= ownAmountresult)
                            {
                                receivedAmount += (ownAmountresult - gaveAmount);
                                double ownerShouldPay = (ownAmountresult - gaveAmount);
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += (ownAmountresult - gaveAmount);
                                ownerList[i].GaveAmount = gaveAmount;

                                if (ownerMemberId == userMemberId)
                                {
                                    var data = new
                                    {
                                        ownerMemberId = ownerMemberId,
                                        owenerName = ownerName,
                                        ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                        ownAmountresult = ownerShouldPay,
                                        payerMemberId = payerMemberId,
                                        payerName = payerName,
                                        payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                        status = $"你需支付{ownerShouldPay}元給{payerName}"
                                    };
                                    settlement.Add(data);
                                }
                                else if (payerMemberId == userMemberId)
                                {

                                    var data = new
                                    {
                                        ownerMemberId = ownerMemberId,
                                        owenerName = ownerName,
                                        ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                        ownAmountresult = ownerShouldPay,
                                        payerMemberId = payerMemberId,
                                        payerName = payerName,
                                        payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                        status = $"你須向{ownerName}取回{ ownerShouldPay}元"
                                    };

                                    settlement.Add(data);
                                }


                            }
                            else if (notEnough < ownAmountresult)
                            {
                                double ownAmountresultPart1 = notEnough;
                                receivedAmount += ownAmountresultPart1;
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += ownAmountresultPart1;
                                ownerList[i].GaveAmount = gaveAmount;

                                if (ownerMemberId == userMemberId)
                                {
                                    var data = new
                                    {
                                        ownerMemberId = ownerMemberId,
                                        owenerName = ownerName,
                                        ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                        ownAmountresult = ownAmountresultPart1,
                                        payerMemberId = payerMemberId,
                                        payerName = payerName,
                                        payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                        status = $"你需支付{ownAmountresultPart1}元給{payerName}"
                                    };

                                    settlement.Add(data);
                                }
                                else if (payerMemberId == userMemberId)
                                {

                                    var data = new
                                    {
                                        ownerMemberId = ownerMemberId,
                                        owenerName = ownerName,
                                        ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                                        ownAmountresult = ownAmountresultPart1,
                                        payerMemberId = payerMemberId,
                                        payerName = payerName,
                                        payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                                        status = $"你須向{ownerName}取回{ownAmountresultPart1}元"
                                    };

                                    settlement.Add(data);
                                }
                            }
                        }
                    }
                }
            }


            notInvolvedList.ToArray();

            foreach (var i in notInvolvedList)
            {
                if (i.MemberId == userMemberId)
                {
                    var resultdata = new
                    {
                        MemberId = i.MemberId,
                        Name = i.Name,
                        MemberImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + i.MemberImageUrl,
                        status = "你無待結清費用"
                    };
                    notInvolved.Add(resultdata);

                }
            }


         
            return Ok(new
            {
                status = true,
                message = "檢視個人待結算金額(尚未結清的費用)成功!",
                userMemberId= userMemberId,
                settlement = settlement,
                notInvolved = notInvolved
            });

        }


        /// <summary>
        /// 5.1.4 檢視收款方式的種類
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Settlement/GetPaymentType")]
        [JwtAuthFilter]
        public IHttpActionResult GetPaymentType()
        {
            var paymentMethodArray = Enum.GetValues(typeof(PaymentMethodList)).Cast<PaymentMethodList>().Select(x=> new {id=x, paymentMethod=x.ToString() }).ToList();

            return Ok(new { paymentType = paymentMethodArray });
        }


        /// <summary>
        /// 5.2.1進行結算
        /// </summary>
        /// <param name="userData">還款資訊</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Settlement/SettleUp")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> SettleUp(SettleUpVM userData )
        {
           
            try
            {
                Settlement userInput = new Settlement
                {
                    GroupId = userData.GroupId,
                    OwnerMemberId = userData.OwnerMemberId,
                    OwnerPaytoPayerAmount = userData.OwnerPaytoPayerAmount,
                    PayerMemberId =userData.PayerMemberId,
                    PaymentMethod =userData.PaymentMethod,
                    CreatDate = userData.CreatDate,
                    Memo=userData.Memo,
                    Image= userData.FileName
                };
                // 加入資料並儲存
                db.Settlements.Add(userInput);
               // db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }


            //通知功能

            //登入會員的ID
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));
            var userName = JwtAuthUtil.GetUserName(Request.Headers.Authorization.Parameter);
            
            var userMemberId = db.Members.Where(x =>x.GroupId==userData.GroupId && x.UserId == userId).Select(x => x.Id).FirstOrDefault();
            var payerName=db.Members.Where(x=>x.Id==userData.PayerMemberId).Select(x => x.Name).FirstOrDefault();
            var payerUserId = db.Members.Where(x => x.Id == userData.PayerMemberId).Select(x => x.UserId).FirstOrDefault();
            var ownerName = db.Members.Where(x => x.Id == userData.OwnerMemberId).Select(x => x.Name).FirstOrDefault();
            var ownerUserId = db.Members.Where(x => x.Id == userData.OwnerMemberId).Select(x => x.UserId).FirstOrDefault();


            if (userData.OwnerMemberId== userMemberId || userData.PayerMemberId == userMemberId)
            {
                Notification userInput = new Notification
                {
                    CreatDate = DateTime.Now,
                    UserId = ownerUserId,
                    Information = $"您已支付{payerName}${userData.OwnerPaytoPayerAmount}元"
                };
                db.Notifications.Add(userInput);

                Notification userInputSecond = new Notification
                {
                    CreatDate = DateTime.Now,
                    UserId = payerUserId,
                    Information = $"{ownerName}已支付您${userData.OwnerPaytoPayerAmount}元"
                };
                db.Notifications.Add(userInputSecond);

            }
         //   else if (userData.PayerMemberId == userMemberId)
           // {
                //Notification userInput = new Notification
                //{
                //    CreatDate = DateTime.Now,
                //    UserId = payerUserId,
                //    Information = $"{ownerName}已支付您${userData.OwnerPaytoPayerAmount}元"
                //};
                //db.Notifications.Add(userInput);

                //Notification userInput = new Notification
                //{
                //    CreatDate = DateTime.Now,
                //    UserId = ownerUserId,
                //    Information = $"您已支付{payerName}${userData.OwnerPaytoPayerAmount}元"
                //};
                //db.Notifications.Add(userInput);

          //  }
            else
            {
                //自己看得到別人的已結清費用狀況
                Notification userInput = new Notification
                {
                    CreatDate = DateTime.Now,
                    UserId = userId,
                    Information = $"{ownerName}已支付{payerName}${userData.OwnerPaytoPayerAmount}元"
                };
                db.Notifications.Add(userInput);

            }

            db.SaveChanges();




            return Ok(new { status = true, message = "結清成功!" });
        }

        /// <summary>
        /// 5.2.2進行結算 (上傳結算的圖片)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Settlement/SettleUpImage")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> SettleUpImage()
        {
            // 檢查請求是否包含 multipart/form-data.
            string fileName;
            if (!Request.Content.IsMimeMultipartContent())
            {
                //  throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                fileName = "default.jpg";

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
                string root = HttpContext.Current.Server.MapPath("~/upload/Settlement");
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory("~/upload/Settlement");
                }

                // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }
            }


            return Ok(new { status = true, message = "上傳圖片成功!", fileName=fileName });
        }

        /// <summary>
        /// 5.3取得所有結算紀錄(已結完) 
        /// </summary>
        /// <param name="groupId">群組的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Settlement/GetAllSettled/{groupId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetAllSettled(int groupId)
        {
             var setlled = db.Settlements.Where(x=>x.GroupId==groupId).Select(x=>new {
                 settledId=x.Id,
                 ownerMemberId = x.OwnerMemberId,
                 payerMemberId=x.PayerMemberId,
                 ownerPaytoPayerAmount=x.OwnerPaytoPayerAmount,
                 creatDate=x.CreatDate
             }).ToList();

            ArrayList settledArrayList = new ArrayList();

            foreach(var i in setlled)
            {
                var payerName = db.Members.Where(x => x.Id == i.payerMemberId).Select(x => x.Name).FirstOrDefault();
                var payerImage = db.Members.Where(x => x.Id == i.payerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();
                var ownerName = db.Members.Where(x => x.Id == i.ownerMemberId).Select(x => x.Name).FirstOrDefault();
                var ownerImage = db.Members.Where(x => x.Id == i.ownerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();

                if (payerImage == null && ownerImage == null)
                {
                    payerImage = "defaultprofile.jpg";
                    ownerImage = "defaultprofile.jpg";
                }
                else if (payerImage == null)
                {
                    payerImage = "defaultprofile.jpg";
                }
                else if (ownerImage == null)
                {
                    ownerImage = "defaultprofile.jpg";
                };

                var data = new
                {
                    settledId=i.settledId,
                    ownerMemberId = i.ownerMemberId,
                    ownerName = ownerName,
                    ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                    ownerPaytoPayerAmount = i.ownerPaytoPayerAmount,
                    payerMemberId = i.payerMemberId,
                    payerName = payerName,
                    payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                    status = $"{payerName}已向{ownerName}取回{i.ownerPaytoPayerAmount}元",
                    creatDate=i.creatDate
                };

                settledArrayList.Add(data);
            }
            return Ok(new { status=true,message= "取得所有結算紀錄成功", settledArrayList = settledArrayList });
        }


        /// <summary>
        /// 5.4取得單筆結算紀錄明細(已結完)
        /// </summary>
        /// <param name="settledId">已結清的費用的id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Settlement/GetSettledDetail/{settledId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetSettledDetail(int settledId)
        {
            var setlled = db.Settlements.Where(x => x.Id == settledId).ToList();

            ArrayList setllementDetail = new ArrayList();

            foreach (var i in setlled)
            {
                var payerName = db.Members.Where(x => x.Id == i.PayerMemberId).Select(x => x.Name).FirstOrDefault();
                var payerImage = db.Members.Where(x => x.Id == i.PayerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();
                var ownerName = db.Members.Where(x => x.Id == i.OwnerMemberId).Select(x => x.Name).FirstOrDefault();
                var ownerImage = db.Members.Where(x => x.Id == i.OwnerMemberId).Select(x => x.UserIdFK.Image).FirstOrDefault();

                if (payerImage == null && ownerImage == null)
                {
                    payerImage = "defaultprofile.jpg";
                    ownerImage = "defaultprofile.jpg";
                }
                else if (payerImage == null)
                {
                    payerImage = "defaultprofile.jpg";
                }
                else if (ownerImage == null)
                {
                    ownerImage = "defaultprofile.jpg";
                };

                var data = new
                {
                    settledId = i.Id,
                    ownerMemberId = i.OwnerMemberId,
                    owenerName = ownerName,
                    ownerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + ownerImage,
                    ownerPaytoPayerAmount = i.OwnerPaytoPayerAmount,
                    payerMemberId = i.PayerMemberId,
                    payerName = payerName,
                    payerImageUrl = "https://" + Request.RequestUri.Host + "/upload/UserAvatar/" + payerImage,
                    status = $"{payerName}已向{ownerName}取回{i.OwnerPaytoPayerAmount}元",
                    creatDate = i.CreatDate,
                    paymentMethod=i.PaymentMethod.ToString(),
                    memo=i.Memo,
                    imageUrl = "https://" + Request.RequestUri.Host + "/upload/Expense/" + i.Image
                };

                setllementDetail.Add(data);
            }
            return Ok(new { status = true, message = "取得結算記錄明細成功", setllementDetail = setllementDetail });
        }





        /// <summary>
        /// 5.5刪除單筆結算紀錄
        /// </summary>
        /// <param name="settledId">已結清的費用的id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Settlement/DeleteSettlemet/{settledId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeleteSettlemet(int settledId)
        {

            if (settledId <= 0)
                return BadRequest("非有效id");

            //群組的成員才可看到群組內的所有費用
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());
            //查詢費用所屬的群組
            var group = Convert.ToInt32(db.Settlements.Where(x => x.Id == settledId).Select(x => x.GroupId).FirstOrDefault());
            //判斷user是否為群組的member
            var groupMember = db.Members.Where(x => x.GroupId == group && x.UserId == userId).FirstOrDefault();

            if (groupMember == null)
            {
                return Ok(new { status = false, message = "非群組成員無法編輯其他群組費用!" });
            }

            var deleteSettlement = db.Settlements.Where(x => x.Id == settledId).FirstOrDefault();


            if (deleteSettlement != null)
            {
                db.Settlements.Remove(deleteSettlement);
            }
           


            //通知功能
            var userName = JwtAuthUtil.GetUserName(Request.Headers.Authorization.Parameter);
            var groupName = db.Settlements.Where(x => x.Id == settledId).Select(x => x.GroupIdFK.Name).FirstOrDefault();

            var groupId = db.Settlements.Where(x => x.Id == settledId).Select(x => x.GroupId).FirstOrDefault();
            var userMemberId = db.Members.Where(x => x.GroupId == groupId && x.UserId == userId).Select(x => x.Id).FirstOrDefault();
             

            //其他群組成員也會收到通知
            var groupMemberId= db.Settlements.Where(x=>x.Id==settledId).Select(x=>x.GroupIdFK.Id).FirstOrDefault();
            var groupMemberNotification = db.Members.Where(x => x.GroupId == groupMemberId).ToList();

            var payerName = db.Members.Where(x => x.Id == deleteSettlement.PayerMemberId).Select(x => x.Name).FirstOrDefault();
            var ownerName = db.Members.Where(x => x.Id == deleteSettlement.OwnerMemberId).Select(x => x.Name).FirstOrDefault();

            foreach (var i in groupMemberNotification)
            {
                if (i.Id != userMemberId)
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = i.UserId,
                        Information = $"{userName}於群組「{groupName}」刪除了一筆{ownerName}給{payerName}的結算費用{deleteSettlement.OwnerPaytoPayerAmount}元"
                    };
                    db.Notifications.Add(userInput);
                }
                else if (i.Id == userMemberId)
                {
                    Notification userInput = new Notification
                    {
                        CreatDate = DateTime.Now,
                        UserId = userId,
                        Information = $"您於群組「{groupName}」刪除了一筆{ownerName}給{payerName}的結算費用{deleteSettlement.OwnerPaytoPayerAmount}元"
                    };
                    db.Notifications.Add(userInput);
                }

            };
            db.SaveChanges();



            return Ok(new { status = true, message = "刪除單筆結清費用成功!" });
        }



        /// <summary>
        /// 5.6取得還款提醒資訊
        /// </summary>
        /// <param name="ownerId">欠款者的member id</param> 
        /// <returns></returns>
        [HttpGet]
        [Route("api/Settlement/GetReminder/{ownerId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetReminder(int ownerId)
        {
            //檢視使用者本人是不是該筆結算費用的付款者本人
            //var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter).ToString());

            //var payerInSettlement = Convert.ToInt32(db.Settlements.Where(x => x.Id == settledId).Select(x => x.PayerMemberId).FirstOrDefault());

            //var payerMemberId = Convert.ToInt32(db.Payers.Where(x => x.MemberId == payerInSettlement).FirstOrDefault());

            //var payerUserId = db.Members.Where(x => x.Id == payerMemberId).Select(x => x.UserId).FirstOrDefault();

            //如果付款者是登入的使用者本人，才可以發送提醒

            //if (payerUserId == userId)
            //{
            //    var showSettlmentReminder = db.Settlements.Where(x => x.Id == settledId).FirstOrDefault();
            //    return Ok(new { status = true, message = "取得還款提醒資訊成功!", showSettlmentReminder = showSettlmentReminder });
            //}
            //return Ok(new { status = true, message = "取得還款提醒資訊失敗!()" });



            //先取得群組所有成員的id
            var memberGroup = Convert.ToInt32(db.Members.Where(x => x.Id == ownerId).Select(x => x.GroupIdFK.Id).FirstOrDefault());
            var members = db.Members.Where(x => x.GroupIdFK.Id == memberGroup).Select(x => x.Id).ToArray();

            List<PayerListVM> payerList = new List<PayerListVM>();
            List<OwnerListVM> ownerList = new List<OwnerListVM>();
            List<SettlementListVM> settlementList = new List<SettlementListVM>();
            ArrayList settlement = new ArrayList();//個人的待結清狀況

            List<NotInvolvedListVM> notInvolvedList = new List<NotInvolvedListVM>();
            List<NotInvolvedListVM> notInvolved = new List<NotInvolvedListVM>();//個人的待結清狀況

            foreach (var mId in members)
            {
                var PayerReceived = db.Settlements.Where(x => x.PayerMemberId == mId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allPayerReceivedMoney = 0;
                foreach (var ownerPaytoPayerAmount in PayerReceived)
                {
                    allPayerReceivedMoney += ownerPaytoPayerAmount;
                }

                var payAmount = db.Payers.Where(x => x.MemberId == mId).Select(x => x.PayAmount).ToArray();
                double allPaidMoney = 0;
                foreach (var paidMoney in payAmount)
                {
                    allPaidMoney += paidMoney;
                }
                //要扣掉已經收款的款項
                allPaidMoney -= allPayerReceivedMoney;

                var OwnerPaid = db.Settlements.Where(x => x.OwnerMemberId == mId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allOwnerPaidMoney = 0;
                foreach (var ownerPaytoPayerAmount in OwnerPaid)
                {
                    allOwnerPaidMoney += ownerPaytoPayerAmount;
                }


                var ownAmount = db.Owners.Where(x => x.MemberId == mId).Select(x => x.OwnAmount).ToArray();
                double allOwnedMoney = 0;
                foreach (var ownedMoney in ownAmount)
                {
                    allOwnedMoney += ownedMoney;
                }

                allOwnedMoney -= allOwnerPaidMoney;

                double resultBalance = allPaidMoney - allOwnedMoney;


                if (resultBalance > 0)
                {
                    var resultdata = new PayerListVM
                    {
                        PayAmount = resultBalance,
                        MemberId = mId
                    };
                    payerList.Add(resultdata);
                }
                else if (resultBalance < 0)
                {
                    resultBalance = Math.Abs(resultBalance);
                    var resultdata = new OwnerListVM
                    {
                        OwnAmount = resultBalance,
                        MemberId = mId
                    };
                    ownerList.Add(resultdata);
                }
                else
                {
                    //如果resultBalance=就是已經結清or沒有參與費用的成員
                    var resultdata = new NotInvolvedListVM
                    {
                        MemberId = mId
                    };
                    notInvolvedList.Add(resultdata);
                }
            }

            payerList = payerList.OrderByDescending(x => x.PayAmount).ToList();
            ownerList = ownerList.OrderByDescending(x => x.OwnAmount).ToList();

            //如果最大的欠款金額(ownerAmount)超過付款者的金額(payerAmount)，就用由小到大的排序
            if (payerList[0].PayAmount < ownerList[0].OwnAmount)
            {
                payerList = payerList.OrderBy(x => x.PayAmount).ToList();
                ownerList = ownerList.OrderBy(x => x.OwnAmount).ToList();
            }

            for (int n = 0; n < payerList.Count; n++)
            {
                int payerMemberId = payerList[n].MemberId;
                double payAmountresult = payerList[n].PayAmount;
                double receivedAmount = payerList[n].ReceivedAmount;

                if (payAmountresult - receivedAmount > 0)
                {
                    for (int i = 0; i < ownerList.Count; i++)
                    {
                        int ownerMemberId = ownerList[i].MemberId;
                        double ownAmountresult = ownerList[i].OwnAmount;
                        double gaveAmount = ownerList[i].GaveAmount;

                        if (ownAmountresult - gaveAmount > 0)
                        {
                            double notEnough = payerList[n].PayAmount - payerList[n].ReceivedAmount;

                            if (notEnough == 0) { }//已經沒有不夠的錢了，就不用再做任何動作
                            else if (notEnough >= ownAmountresult)
                            {
                                receivedAmount += ownAmountresult;
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += ownAmountresult;
                                ownerList[i].GaveAmount = gaveAmount;


                                var resultdata = new SettlementListVM
                                {
                                    OwnerMemberId = ownerMemberId,
                                    OwnAmountresult = ownAmountresult,
                                    PayerMemberId = payerMemberId
                                };
                                settlementList.Add(resultdata);
                            }
                            else if (notEnough < ownAmountresult)
                            {
                                double ownAmountresultPart1 = notEnough;

                                receivedAmount += ownAmountresultPart1;
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += ownAmountresultPart1;
                                ownerList[i].GaveAmount = gaveAmount;


                                var resultdata = new SettlementListVM
                                {
                                    OwnerMemberId = ownerMemberId,
                                    OwnAmountresult = ownAmountresultPart1,
                                    PayerMemberId = payerMemberId
                                };
                                settlementList.Add(resultdata);
                            }
                        }
                    }
                }
            }

            settlementList.ToArray();
            notInvolvedList.ToArray();

            foreach (var i in settlementList)
            {
                if (i.OwnerMemberId == ownerId)
                {
                    var payerName = db.Members.Where(x => x.Id == i.PayerMemberId).Select(x => x.Name).FirstOrDefault();
                    var ownerName = db.Members.Where(x => x.Id == i.OwnerMemberId).Select(x => x.Name).FirstOrDefault();

                    var data = new
                    {
                        ownerMemberId = i.OwnerMemberId,
                        owenerName = ownerName,
                        ownAmountresult = i.OwnAmountresult,
                        payerMemberId = i.PayerMemberId,
                        payerName = payerName,
                        status = $"{ownerName}需支付{i.OwnAmountresult}元給{payerName}"
                    };

                    settlement.Add(data);
                }

            }

            if (settlement.Count!=0)
            {
                return Ok(new
                {
                    status = true,
                    message = "取得還款提醒資訊成功!",
                    settlementReminder = settlement,
                });
            }
            else
            {
                return Ok(new
                {
                    status = false,
                    message = "無須提醒還款(因為不是欠款者)!",
                });
            }

        }



        /// <summary>
        /// 5.7發送還款提醒
        /// </summary>
        /// <param name="userData">發送還款資訊</param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Settlement/SendReminder")]
        [JwtAuthFilter]
        public IHttpActionResult SendReminder(SettleReminderVM userData)
        {

            //先取得群組所有成員的id
            var memberGroup = Convert.ToInt32(db.Members.Where(x => x.Id == userData.OwnerId).Select(x => x.GroupIdFK.Id).FirstOrDefault());
            var members = db.Members.Where(x => x.GroupIdFK.Id == memberGroup).Select(x => x.Id).ToArray();

            List<PayerListVM> payerList = new List<PayerListVM>();
            List<OwnerListVM> ownerList = new List<OwnerListVM>();
            List<SettlementListVM> settlementList = new List<SettlementListVM>();
            ArrayList settlement = new ArrayList();//個人的待結清狀況

            List<NotInvolvedListVM> notInvolvedList = new List<NotInvolvedListVM>();
            List<NotInvolvedListVM> notInvolved = new List<NotInvolvedListVM>();//個人的待結清狀況

            foreach (var mId in members)
            {
                var PayerReceived = db.Settlements.Where(x => x.PayerMemberId == mId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allPayerReceivedMoney = 0;
                foreach (var ownerPaytoPayerAmount in PayerReceived)
                {
                    allPayerReceivedMoney += ownerPaytoPayerAmount;
                }

                var payAmount = db.Payers.Where(x => x.MemberId == mId).Select(x => x.PayAmount).ToArray();
                double allPaidMoney = 0;
                foreach (var paidMoney in payAmount)
                {
                    allPaidMoney += paidMoney;
                }
                //要扣掉已經收款的款項
                allPaidMoney -= allPayerReceivedMoney;

                var OwnerPaid = db.Settlements.Where(x => x.OwnerMemberId == mId).Select(x => x.OwnerPaytoPayerAmount).ToArray();
                double allOwnerPaidMoney = 0;
                foreach (var ownerPaytoPayerAmount in OwnerPaid)
                {
                    allOwnerPaidMoney += ownerPaytoPayerAmount;
                }


                var ownAmount = db.Owners.Where(x => x.MemberId == mId).Select(x => x.OwnAmount).ToArray();
                double allOwnedMoney = 0;
                foreach (var ownedMoney in ownAmount)
                {
                    allOwnedMoney += ownedMoney;
                }

                allOwnedMoney -= allOwnerPaidMoney;

                double resultBalance = allPaidMoney - allOwnedMoney;


                if (resultBalance > 0)
                {
                    var resultdata = new PayerListVM
                    {
                        PayAmount = resultBalance,
                        MemberId = mId
                    };
                    payerList.Add(resultdata);
                }
                else if (resultBalance < 0)
                {
                    resultBalance = Math.Abs(resultBalance);
                    var resultdata = new OwnerListVM
                    {
                        OwnAmount = resultBalance,
                        MemberId = mId
                    };
                    ownerList.Add(resultdata);
                }
                else
                {
                    //如果resultBalance=就是已經結清or沒有參與費用的成員
                    var resultdata = new NotInvolvedListVM
                    {
                        MemberId = mId
                    };
                    notInvolvedList.Add(resultdata);
                }
            }

            payerList = payerList.OrderByDescending(x => x.PayAmount).ToList();
            ownerList = ownerList.OrderByDescending(x => x.OwnAmount).ToList();

            //如果最大的欠款金額(ownerAmount)超過付款者的金額(payerAmount)，就用由小到大的排序
            if (payerList[0].PayAmount < ownerList[0].OwnAmount)
            {
                payerList = payerList.OrderBy(x => x.PayAmount).ToList();
                ownerList = ownerList.OrderBy(x => x.OwnAmount).ToList();
            }

            for (int n = 0; n < payerList.Count; n++)
            {
                int payerMemberId = payerList[n].MemberId;
                double payAmountresult = payerList[n].PayAmount;
                double receivedAmount = payerList[n].ReceivedAmount;

                if (payAmountresult - receivedAmount > 0)
                {
                    for (int i = 0; i < ownerList.Count; i++)
                    {
                        int ownerMemberId = ownerList[i].MemberId;
                        double ownAmountresult = ownerList[i].OwnAmount;
                        double gaveAmount = ownerList[i].GaveAmount;

                        if (ownAmountresult - gaveAmount > 0)
                        {
                            double notEnough = payerList[n].PayAmount - payerList[n].ReceivedAmount;

                            if (notEnough == 0) { }//已經沒有不夠的錢了，就不用再做任何動作
                            else if (notEnough >= ownAmountresult)
                            {
                                receivedAmount += ownAmountresult;
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += ownAmountresult;
                                ownerList[i].GaveAmount = gaveAmount;


                                var resultdata = new SettlementListVM
                                {
                                    OwnerMemberId = ownerMemberId,
                                    OwnAmountresult = ownAmountresult,
                                    PayerMemberId = payerMemberId
                                };
                                settlementList.Add(resultdata);
                            }
                            else if (notEnough < ownAmountresult)
                            {
                                double ownAmountresultPart1 = notEnough;

                                receivedAmount += ownAmountresultPart1;
                                payerList[n].ReceivedAmount = receivedAmount;
                                gaveAmount += ownAmountresultPart1;
                                ownerList[i].GaveAmount = gaveAmount;


                                var resultdata = new SettlementListVM
                                {
                                    OwnerMemberId = ownerMemberId,
                                    OwnAmountresult = ownAmountresultPart1,
                                    PayerMemberId = payerMemberId
                                };
                                settlementList.Add(resultdata);
                            }
                        }
                    }
                }
            }

            settlementList.ToArray();
            notInvolvedList.ToArray();

            foreach (var i in settlementList)
            {
                if (i.OwnerMemberId == userData.OwnerId)
                {
                    var payerName = db.Members.Where(x => x.Id == i.PayerMemberId).Select(x => x.Name).FirstOrDefault();
                    var ownerName = db.Members.Where(x => x.Id == i.OwnerMemberId).Select(x => x.Name).FirstOrDefault();

                    var data = new
                    {
                        ownerMemberId = i.OwnerMemberId,
                        owenerName = ownerName,
                        ownAmountresult = i.OwnAmountresult,
                        payerMemberId = i.PayerMemberId,
                        payerName = payerName,
                        status = $"{ownerName}需支付{i.OwnAmountresult}元給{payerName}"
                    };

                    settlement.Add(data);

                    var ownerUserId=db.Members.Where(x => x.Id == userData.OwnerId).Select(x => x.UserId).FirstOrDefault();

                    if (i.OwnerMemberId== userData.OwnerId && i.PayerMemberId== userData.PayerId)
                    {
                        var reminderData = new Notification
                        {
                            UserId = ownerUserId,
                            CreatDate = DateTime.Now,
                            Information = $"提醒您，尚需支付{i.OwnAmountresult}元給{payerName}"
                        };

                        db.Notifications.Add(reminderData);
                        db.SaveChanges();

                        return Ok(new
                        {
                            status = true,
                            message = "發送還款提醒資訊成功!",
                            reminderData = reminderData,

                        });
                    }

                }

            }


            return Ok(new
            {
                status = false,
                message = "付款者與欠款者之間無待結清費用"

            });
        }

    }
}