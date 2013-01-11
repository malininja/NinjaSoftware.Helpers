using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Configuration;

namespace NinjaSoftware.CoolJ
{
    public class OptimisticConcurrencyPredicateFactory : IConcurrencyPredicateFactory
    {
        private string _concurrencyFieldName;
        private bool _doTriggerOnSave;
        private bool _doTriggerOnDelete;

        /// <summary>
        /// /// Creates concurrency predicate based on concurrencyFieldName.
        /// </summary>
        /// <param name="concurrencyFieldName">Field which will be used for concurrency check.</param>
        /// <param name="doTriggerOnSave">If true Concurrency predicated will be added on Save</param>
        /// <param name="doTriggerOnDelete">If true Concurrency predicated will be added on Delete</param>
        public OptimisticConcurrencyPredicateFactory(string concurrencyFieldName, bool doTriggerOnSave, bool doTriggerOnDelete)
        {
            _concurrencyFieldName = concurrencyFieldName;
            _doTriggerOnSave = doTriggerOnSave;
            _doTriggerOnDelete = doTriggerOnDelete;
        }

        public IPredicateExpression CreatePredicate(ConcurrencyPredicateType predicateTypeToCreate, object containingEntity)
        {
            IPredicateExpression toReturn = null;

            if ((_doTriggerOnSave && predicateTypeToCreate == ConcurrencyPredicateType.Save) ||
                (_doTriggerOnDelete && predicateTypeToCreate == ConcurrencyPredicateType.Delete))
            {
                toReturn = new PredicateExpression();
                IEntity2 entity = (IEntity2)containingEntity;
                EntityField2 concurrencyField = (EntityField2)entity.Fields[_concurrencyFieldName];
                toReturn.Add(concurrencyField == concurrencyField.CurrentValue);
            }

            return toReturn;
        }
    }
}
