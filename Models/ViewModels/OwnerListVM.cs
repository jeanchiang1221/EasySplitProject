using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasySplitProject.Models.ViewModels
{
    public class OwnerListVM
    {
        public double OwnAmount { get; set; }

        public int MemberId { get; set; }

        public string OwnerName { get; set; }

        public double GaveAmount { get; set; }

        public string OwnerImageUrl { get; set; }
    }
}