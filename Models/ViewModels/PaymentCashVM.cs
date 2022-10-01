using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models.ViewModels
{

    /// <summary>
    /// 現金面交
    /// </summary>
    public class PaymentCashVM
    {
        /// <summary>
        /// 姓名
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "姓名")]
        public string Name { get; set; }

        /// <summary>
        /// 手機
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "手機")]
        [RegularExpression(@"^09[0-9]{8}$")]
        [Required]
        public string Phone { get; set; }

        /// <summary>
        /// 聯絡訊息
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "聯絡訊息")]
        [Required]
        public string Method { get; set; }
    }
}