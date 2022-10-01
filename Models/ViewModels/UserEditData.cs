using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models.ViewModels
{
    /// <summary>
    /// 修改會員資料
    /// </summary>
    public class UserEditData
    {
        /// <summary>
        /// 頭貼圖檔名稱
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "頭貼圖檔名稱")]
        public string Image { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "名稱")]
        public string Name { get; set; }
    }
}