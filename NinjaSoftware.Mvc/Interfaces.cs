using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace NinjaSoftware.Mvc
{
    public interface IViewModel
    {
        void Save(DataAccessAdapterBase adapter);
    }
}
