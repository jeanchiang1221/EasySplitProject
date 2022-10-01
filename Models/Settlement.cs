using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using static EasySplitProject.Models.EnumDB;

namespace EasySplitProject.Models
{
    public class Settlement
    {
        /// <summary>
        /// 結清編號
        /// </summary>
        [Key]
        [Display(Name = "結清編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 所屬群組編號
        /// </summary>
        [Display(Name = "屬群組編號")]
        public int GroupId { get; set; }

        [ForeignKey("GroupId")]
         [JsonIgnore]
        public virtual Group GroupIdFK { get; set; }

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
        /// 還款時間
        /// </summary>
        public DateTime CreatDate { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public PaymentMethodList PaymentMethod { get; set; }

        /// <summary>
        /// 付款註記
        /// </summary>
        [MaxLength(200)]
        public string Memo { get; set; }


        /// <summary>
        /// 付款圖片
        /// </summary>
        [MaxLength(200)]
        public string Image { get; set; }

    }


}