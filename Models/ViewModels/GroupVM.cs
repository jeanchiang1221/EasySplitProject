using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models.ViewModels
{
    public class GroupVM
    {

        /// <summary>
        /// 群組編號
        /// </summary>

        public int Id { get; set; }


        /// <summary>
        /// 群組名稱
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "群組名稱")]
        public string Name { get; set; }


        /// <summary>
        /// 群組圖片
        /// </summary>

        public string FileName { get; set; }

    }
}