using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ImageSharePassword.Data;

namespace ImageSharePassword.Web.Models
{
    public class ViewImageViewModel
    {
        public bool HasPermissionToView { get; set; }
        public Image Image { get; set; }
        public string Message { get; set; }
    }
}