using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static EasySplitProject.Models.EnumDB;

namespace EasySplitProject.Models.ViewModels
{
    public class AddExpenseVM
    {

        /// <summary>
        /// 所屬群組編號
        /// </summary>
        public int GroupId { get; set; }


        /// <summary>
        /// 費用種類
        /// </summary>
        public ExpenseType ExpenseType { get; set; }

        /// <summary>
        /// 品項
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]
        public string Item { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "金額")]
        public double Cost { get; set; }

        /// <summary>
        /// 付款者的memberId
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "付款者的資訊")]

        public AddPayerExpenseVM[] AddPayerExpenseVMs { get; set; }

        /// <summary>
        /// 分款者的memberId
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]

        public AddOwnerExpenseVM[] AddOwnerExpenseVMs { get; set; }

        public AddExpenseAlbumVM[] AddExpenseAlbumVMs { get; set; }

        /// <summary>
        /// 建立時間(若沒有填，會預設為當下)
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatDate { get; set; }

        /// <summary>
        /// 註記
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]
        public string Memo { get; set; }
    }


    public class AddPayerExpenseVM
    {
        public int MemberId { get; set; }
        public double PayAmount { get; set; }

    }

    public class AddOwnerExpenseVM
    {
        public int MemberId { get; set; }
        public double OwnAmount { get; set; }

    }


    public class AddExpenseAlbumVM
    {

        /// <summary>
        /// 相片名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 所屬費用編號
        /// </summary>
        public int ExpenseId { get; set; }

    }

}