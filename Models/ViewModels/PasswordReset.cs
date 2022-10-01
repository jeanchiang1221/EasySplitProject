using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models.ViewModels
{
    /// <summary>
    /// 登入後變更密碼
    /// </summary>
    public class PasswordReset
    {
        /// <summary>
        /// 註冊密碼
        /// </summary>

        [Required(ErrorMessage = "新密碼必填")]
        [RegularExpression(@"^.*(?=.{6,})(?=.*\d)(?=.*[a-zA-Z]).*$", ErrorMessage = "密碼須包含6位以上英文數字符號")]
        public string NewPassword { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "請您再次輸入密碼！")]
        //與Password做比對，再次確認使用者輸入的密碼
        //會使用System.Web.Mvc.Compare，是因為引入System.ComponentModel.DataAnnotations時會有衝突的產生
        [Compare("NewPassword", ErrorMessage = "兩次輸入的密碼必須相符！")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// 信箱驗證 GUID
        /// </summary>
        public string Guid { get; set; }
    }
}