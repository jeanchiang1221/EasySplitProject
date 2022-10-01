using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasySplitProject.Models.ViewModels
{
    public class SettleReminderVM
    {
        /// <summary>
        /// 欠款者memberId
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// 收款者memberId
        /// </summary>
        public int PayerId { get; set; }
    }
}