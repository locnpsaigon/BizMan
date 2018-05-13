using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizMan.Helpers
{
    public class Configurations
    {
        
        public static int DEFAULT_PAGE_SIZE =
            int.Parse(ConfigurationManager.AppSettings["DEFAULT_PAGE_SIZE"]); // paging size configuration
    }
}