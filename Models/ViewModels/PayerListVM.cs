using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasySplitProject.Models.ViewModels
{
    public class PayerListVM
    {
        public double PayAmount { get; set; }

        public int MemberId { get; set; }

        public string PayerName { get; set; }

        public double ReceivedAmount { get; set; }

        public string PayerImageUrl { get; set; }

    }
}