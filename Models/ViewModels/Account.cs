using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models.ViewModels
{
    public class Account
    {
        /// <summary>
        /// 信箱帳號
        /// </summary>
        [Required(ErrorMessage = "信箱必填")]
        [EmailAddress(ErrorMessage = "Email格式有誤")]
        public string AccountMail { get; set; }
    }
}