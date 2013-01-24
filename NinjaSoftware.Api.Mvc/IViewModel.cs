using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace NinjaSoftware.Api.Mvc
{
    public interface IViewModel
    {
        /// <summary>
        /// Logic for data persitance and validation.
        /// </summary>
        void Save(DataAccessAdapterBase adapter);

        /// <summary>
        /// Fetch data that is needed only for View, and is not needed for Save
        /// </summary>
        void LoadViewSpecificData(DataAccessAdapterBase adapter);
    }
}
