using EasySplitProject.Models;
using EasySplitProject.Security;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EasySplitProject.Controllers
{
    [OpenApiTag("Notification", Description = "通知的檢視")]
    public class NotificationController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        /// <summary>
        /// 6.1取得所有通知
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Notification/GetAllNotification")]
        [JwtAuthFilter]
        public IHttpActionResult GetAllNotification()
        {
            var userId = Convert.ToInt32(JwtAuthUtil.GetUserId(Request.Headers.Authorization.Parameter));

            
            var notification = db.Notifications.Where(x => x.UserId == userId).ToList();

            //如果沒有任何通知
            if (notification.Count==0)
            {
                return Ok(new
                {
                    status = true,
                    message = "目前尚未有任何通知!"
                });
            }

            return Ok(new { status=true, notification = notification });
        }


    }
}