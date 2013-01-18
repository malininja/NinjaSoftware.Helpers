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
        #region Constructors

        public PagerBase(int pageSize)
        {
            this.PageSize = pageSize;
        }

        #endregion

        #region Private members

        /// <summary>
        /// Calls abstract LoadData which sets DataSource i PageSize.
        /// Calculate and set NoOfPages.
        /// </summary>
        protected void LoadData(DataAccessAdapterBase adapter,
            int pageNo,
            string sortField,
            string sortDirection,
            string jqGridFilters)
        {
            this.CurrentPage = pageNo;

            string sort = string.IsNullOrWhiteSpace(sortField) ? this.DefaultSortField : sortField;
            string direction = string.IsNullOrWhiteSpace(sortDirection) ? this.DefaultSortDirection : sortDirection;

            LoadData(adapter, pageNo, this.PageSize, sort, direction);
            NoOfPages = CalculateNoOfPages(NoOfRecords, this.PageSize);
        }

        #endregion

        #region Abstract members

        /// <summary>
        /// Set DataSource i PageSize.
        /// </summary>
        protected abstract void LoadData(DataAccessAdapterBase adapter,
            int pageNumber,
            int pageSize,
            string sortField,
            string sortDirection);

        public abstract string DefaultSortField { get; }

        public abstract string DefaultSortDirection { get; }

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

        #endregion

        #region HTML generation

        //public HtmlString GeneratePagerNavigation(RequestContext requestContext,
        //    string actionName,
        //    RouteValueDictionary routeValues)
        //{
        //    UrlHelper urlHelper = new UrlHelper(requestContext);

        //    for (int i = 1; i <= NoOfPages; i++)
        //    {
        //        string fetchDataUrl = urlHelper.Action(actionName, routeValues);
            
        //    }
            


        //}

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
    }
}