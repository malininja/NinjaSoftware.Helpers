using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;

namespace NinjaSoftware.Api.Mvc
{
    public abstract class PagerBase
    {
        #region Protected members

        /// <summary>
        /// Calls abstract LoadData which sets DataSource i PageSize.
        /// Calculate and set NoOfPages.
        /// </summary>
        public void LoadData(DataAccessAdapterBase adapter,
            int? currentPage,
            int pageSize,
            string sortField,
            bool? isSortAscending)
        {
            this.PageSize = pageSize;
            this.CurrentPage = currentPage.HasValue ? currentPage.Value : 1;

            string sort = string.IsNullOrWhiteSpace(sortField) ? this.DefaultSortField : sortField;
            if (isSortAscending.HasValue)
            {
                this.IsSortDirectionAscending = isSortAscending.Value;
            }
            else
            {
                this.IsSortDirectionAscending = this.IsDefaultSortDirectionAscending;
            }

            SetDataSource(adapter, this.CurrentPage, this.PageSize, sort, this.IsSortDirectionAscending);
            NoOfPages = CalculateNoOfPages(NoOfRecords, this.PageSize);
        }

        #endregion

        #region Abstract members

        /// <summary>
        /// Set DataSource i PageSize.
        /// </summary>
        protected abstract void SetDataSource(DataAccessAdapterBase adapter,
            int pageNumber,
            int pageSize,
            string sortField,
            bool isSortAscending);

        public abstract string DefaultSortField { get; }

        public abstract bool IsDefaultSortDirectionAscending { get; }

        #endregion

        #region Public properties

        public IEnumerable<Object> DataSource { get; protected set; }
        public int NoOfPages { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; protected set; }

        /// <summary>
        /// Number of records which would be fetched if there was no paging
        /// </summary>
        public int NoOfRecords { get; protected set; }

        public string SortField { get; set; }
        public bool IsSortDirectionAscending { get; set; }

        #endregion

        #region Static methods

        /// <summary>
        /// Calculates number of pages (for grid).
        /// </summary>
        /// <param name="noOfRecords">Total number of records (not only on page).</param>
        /// <param name="pageSize">Maximum number of records on page.</param>
        public static int CalculateNoOfPages(int noOfRecords, int pageSize)
        {
            int toReturn;

            if (0 == pageSize)
            {
                toReturn = 0;
            }
            else
            {
                toReturn = noOfRecords / pageSize;

                if ((noOfRecords % pageSize) > 0)
                {
                    toReturn++;
                }
            }

            return toReturn;
        }

        #endregion

        #region ViewElementsGeneration

        public HtmlString GenerateDropDownPagingHtmlElements(string dropDownPrefixText)
        {
            StringBuilder bob = new StringBuilder();
            string isSelectedString = @"selected=""selected""";
            for (int i = 1; i <= this.NoOfPages; i++)
            {
                bob.Append(string.Format(@"<option {0} value=""{1}"">{1}</option>",
                    i == this.CurrentPage ? isSelectedString : string.Empty,
                    i));
            }

            HtmlString htmlString = new HtmlString(string.Format(
@"{0} <select id=""CurrentPage"" name=""CurrentPage"" onchange=""pagerNavigationSetGridPage(this.value)"">{1}</select> / {2}
<script type=""text/javascript"">
function pagerNavigationSetGridPage(newPageNo) {{
var sort = ninjaSoftware.url.getParameterValue(""sortField"");
var sortDir = ninjaSoftware.url.getParameterValue(""sortDirection"");
var url = this.location.pathname + ""?"";
var isFirst = true;
if ("""" != sort || """" != sortDir) {{
url = url + ""sortField="" + sort + ""&sortDirection="" + sortDir;
isFirst = false;
}}
if (!isFirst) {{
url = url + ""&"";
}}
url = url + ""pageNumber="" + newPageNo;
this.location.href = url;
}}
</script>", dropDownPrefixText, bob.ToString(), this.NoOfPages));

            return htmlString;
        }

        #endregion
    }
}