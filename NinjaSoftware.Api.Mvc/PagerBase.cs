using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using Newtonsoft.Json;

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
            RelationPredicateBucket bucket,
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

            SetDataSource(adapter, bucket, this.CurrentPage, this.PageSize, sort, this.IsSortDirectionAscending);
            NoOfPages = CalculateNoOfPages(NoOfRecords, this.PageSize);
        }

        #endregion

        #region Abstract members

        /// <summary>
        /// Set DataSource i PageSize.
        /// </summary>
        protected abstract void SetDataSource(DataAccessAdapterBase adapter,
            RelationPredicateBucket bucket,
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

        #region JqGrid

        public static bool IsJqgridSortAscending(string sord)
        {
            if (string.IsNullOrWhiteSpace(sord))
            {
                return false;
            }

            return "asc" == sord.Trim().ToLowerInvariant();
        }

        public dynamic CreateJqGridRespose()
        {
            return new
            {
                page = this.CurrentPage,
                total = this.NoOfPages,
                records = this.NoOfRecords,
                rows = this.DataSource
            };
        }

        #endregion
    }
}