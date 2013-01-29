using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Configuration;

namespace NinjaSoftware.Api.Mvc
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

        public static string ToLocalizedWord(this bool value)
        {
            string language = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            if ("hr" == language)
            {
                if (value)
                {
                    return "da";
                }
                else
                {
                    return "ne";
                }
            }
            else
            {
                if (value)
                {
                    return "yes";
                }
                else
                {
                    return "no";
                }
            }
        }
    }
}
