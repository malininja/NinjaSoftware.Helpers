using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace NinjaSoftware.Api.CoolJ
{
    public static class PredicateHelper
    {
        /// <summary>
        /// Returns valid entities which are valid for given moment in time.
        /// Valid entities have:
        /// 1. Begining is not NULL AND is LESS than given moment in time.
        /// 2. End is NULL OR is GREATER OR EQUAL than given moment in time.
        /// </summary>
        public static PredicateExpression FilterValidEntities(DateTime momentInTime,
            EntityField2 validFromDateTimeField,
            EntityField2 validToDateTimeField)
        {
            PredicateExpression predicateExpression = new PredicateExpression();
            predicateExpression.Add(validFromDateTimeField != DBNull.Value & validFromDateTimeField <= momentInTime);
            predicateExpression.AddWithAnd(validToDateTimeField == DBNull.Value | validToDateTimeField >= momentInTime);

            return predicateExpression;
        }

        /// <summary>
        /// Returns predicate which returns single (on none) entity for given moment in time.
        /// Vraća predikat koji filtrira jedan entitet koji je validan za dani momentInTime.
        /// </summary>
        /// <param name="setFilter">Additional filter which applies before datetime predicate.</param>
        public static PredicateExpression FilterValidEntities(DateTime momentInTime,
            EntityField2 validDateTimeField,
            IPredicateExpression setFilter)
        {
            PredicateExpression newSetFilter;

            if (null != setFilter)
            {
                newSetFilter = new PredicateExpression(setFilter);
                newSetFilter.AddWithAnd(validDateTimeField <= momentInTime);
            }
            else
            {
                newSetFilter = new PredicateExpression(validDateTimeField <= momentInTime);
            }

            PredicateExpression toReturn = new PredicateExpression();
            toReturn.Add(validDateTimeField <= momentInTime);
            toReturn.Add(new FieldCompareSetPredicate(validDateTimeField, null, validDateTimeField, null, SetOperator.GreaterEqualAll, newSetFilter));

            return toReturn;
        }

        /// <summary>
        /// Returns entites which validityDateTimeField is GREATER OR EQUAL than startDateTime,
        /// and LESS than endDateTime.
        public static PredicateExpression FilterValidEntities(DateTime? startDateTime,
            DateTime? endDateTime,
            EntityField2 validityDateTimeField)
        {
            PredicateExpression predicateExpression = new PredicateExpression();

            if (null != startDateTime)
            {
                predicateExpression.Add(validityDateTimeField >= startDateTime.Value);
            }

            if (null != endDateTime)
            {
                predicateExpression.Add(validityDateTimeField < endDateTime.Value);
            }

            return predicateExpression;
        }

        /// <summary>
        /// Returns entities which period, at least partially, overlaps with given period.
        /// </summary>
        public static PredicateExpression FilterValidEntities(DateTime? startDateTime,
            DateTime? endDateTime,
            EntityField2 validFromDateTimeField,
            EntityField2 validToDateTimeField)
        {
            PredicateExpression predicateExpression = new PredicateExpression();

            if (null != startDateTime)
            {
                predicateExpression.Add(validToDateTimeField >= startDateTime.Value | validToDateTimeField == DBNull.Value);
            }

            if (null != endDateTime)
            {
                predicateExpression.Add(validFromDateTimeField <= endDateTime.Value | validFromDateTimeField == DBNull.Value);
            }

            return predicateExpression;
        }
    }
}
