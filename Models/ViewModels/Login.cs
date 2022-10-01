using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models.ViewModels
{
    public class Login
    {
        /// <summary>
        /// 信箱帳號
        /// </summary>
        [Required(ErrorMessage = "信箱必填")]
        [EmailAddress(ErrorMessage = "Email格式有誤")]
        public string Account { get; set; }
        /// <summary>
        /// 註冊密碼
        /// </summary>
        [Required(ErrorMessage = "密碼必填")]
        [RegularExpression(@"^.*(?=.{6,})(?=.*\d)(?=.*[a-zA-Z]).*$", ErrorMessage = "密碼須包含6位以上英文數字符號")]
        public string Password { get; set; }


    }
}