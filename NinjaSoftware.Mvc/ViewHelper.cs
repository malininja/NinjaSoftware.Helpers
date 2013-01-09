using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Configuration;

namespace NinjaSoftware.Mvc
{
    public static class ViewHelper
    {
        /// <summary>
        /// Returns SelectList based on DataSource and adds first item whos value is null and text is fetched from .config file (FirstDropDownItemText item).
        /// </summary>
        public static List<SelectListItem> GenerateDropDownItemSource(IEnumerable<Object> dataSource, 
            string valuePropertyName, 
            string textPropertyName, 
            object selectedValue)
        {
            List<SelectListItem> toReturn = new List<SelectListItem>();

            string firstDropDownItemText = ConfigurationManager.AppSettings["FirstDropDownItemText"];

            toReturn.Add(new SelectListItem() { Value = "", Text = firstDropDownItemText });

            if (null != dataSource)
            {
                toReturn.AddRange(new SelectList(dataSource, valuePropertyName, textPropertyName, ConfigurationManager.AppSettings["FirstDropDownItemText"]));
            }

            return toReturn;
        }
    }
}
