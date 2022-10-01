using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static EasySplitProject.Models.EnumDB;

namespace EasySplitProject.Models.ViewModels
{
    public class SettleUpVM
    {
        /// <summary>
        /// 所屬群組編號
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// 欠款者成員id
        /// </summary>
        public int OwnerMemberId { get; set; }

        /// <summary>
        /// 收款者成員id
        /// </summary>
        public int PayerMemberId { get; set; }

        /// <summary>
        /// 欠款者已付金額
        /// </summary>
        public double OwnerPaytoPayerAmount { get; set; }
        /// <summary>
        /// 付款方式
        /// </summary>
        public PaymentMethodList PaymentMethod { get; set; }

        /// <summary>
        /// 付款註記
        /// </summary>

        public string Memo { get; set; }


        /// <summary>
        /// 結清圖片
        /// </summary>

        public string FileName { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>

        public DateTime CreatDate { get; set; }


    }



}