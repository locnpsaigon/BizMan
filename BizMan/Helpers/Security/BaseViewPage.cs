using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BizMan.Helpers.Security
{
    public abstract class BaseViewPage : WebViewPage
    {
        public virtual new BizManPrinciple User
        {
            get { return base.User as BizManPrinciple; }
        }
    }
    public abstract class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        public virtual new BizManPrinciple User
        {
            get { return base.User as BizManPrinciple; }
        }
    }
}