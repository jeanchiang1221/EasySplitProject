using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models.ViewModels
{
    public class MemberVM
    {
        /// <summary>
        /// 所屬群組編號
        /// </summary>
        public int GroupId { get; set; }


        /// <summary>
        /// 成員名稱
        /// </summary>
        [MaxLength(50)]
        public string Name { get; set; }






    }
}