using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Reflection;

namespace NinjaSoftware.Api.CoolJ
{
    public static class SortHelper
    {
        /// <summary>
        /// Builds sort expression for given sortField.
        /// If sortField is complex it try to find EntityFieldClass based on last part of sortField.
        /// </summary>
        /// <param name="sortField">Name of sort field.</param>
        /// <param name="sortDirection">Valid values are 'asc' i 'desc'.</param>
        public static SortExpression GetSortExpression(string sortField, string sortDirection, Type entityFieldsClass)
        {
            string[] entityNames = sortField.Split('.');

            if (entityNames.Count() > 1)
            {
                sortField = entityNames[entityNames.Count() - 1];
                entityFieldsClass = Type.GetType(string.Format("{0}{1}Fields",
                    entityFieldsClass.FullName.Replace(entityFieldsClass.Name, ""),
                    entityNames[entityNames.Count() - 2]));
            }

            EntityField2 orderEntityField = (EntityField2)entityFieldsClass.GetProperty(sortField, BindingFlags.Public | BindingFlags.Static).GetValue(null, null);

            SortOperator sortOperator;

            switch (sortDirection.ToLower())
            {
                case "asc":
                    sortOperator = SortOperator.Ascending;
                    break;
                case "desc":
                    sortOperator = SortOperator.Descending;
                    break;
                default:
                    throw new ArgumentException("Unsupported sort type.");
            }

            return new SortExpression(orderEntityField | sortOperator);
        }
    }
}
