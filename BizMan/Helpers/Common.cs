using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BizMan.Helpers
{
    public class Common
    {
        public static IEnumerable<SelectListItem> CreateDropDownListData(IEnumerable<SelectListItem> listItems, string selectedValue)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in listItems)
            {
                // create new select list item
                var newItem = new SelectListItem();
                newItem.Text = item.Text;
                newItem.Value = item.Value;
                newItem.Selected = string.Compare(selectedValue, item.Value, true) == 0;

                // add new item
                result.Add(newItem);
            }
            return result;
        }
    }
}