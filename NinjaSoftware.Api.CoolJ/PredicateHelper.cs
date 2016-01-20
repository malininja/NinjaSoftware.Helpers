using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;
using Newtonsoft.Json;
using System.Reflection;

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

        public static PredicateExpression CreatePredicateFromJqGridFilterString(string jqGridFilterString, 
        	Type entityType,
			Func<string, Type> getEntityFieldTypeFunction,
			string ormEntityHelperClassesNamespace)
        {
            JgGridFilter jqGridFilter = JsonConvert.DeserializeObject<JgGridFilter>(jqGridFilterString);

            if (jqGridFilter != null && jqGridFilter.rules.Count() > 0)
            {
                return jqGridFilter.KreirajPredicate(entityType, getEntityFieldTypeFunction, ormEntityHelperClassesNamespace);
            }
            else
            {
                return null;
            }
        }
    }

    public class JgGridFilter
    {
        public string groupOp { get; set; }
        public IEnumerable<JqGridFilterItem> rules { get; set; }

        /// <summary>
        /// Preko refleksije izvlači EntityField2 za filtriranje iz rules.
        /// Ako je rules.Count == 0 vraća NULL.
        /// </summary>
        /// <param name="entityFieldsClass">Ako se filtriraju lijekovi onda LijekFields.</param>
        /// <returns>PredicateExpression</returns>
		public PredicateExpression KreirajPredicate(Type entityFieldsClass, 
			Func<string, Type> getEntityFieldTypeFunction,
			string ormEntityHelperClassesNamespace)
        {
            PredicateExpression toReturn = null;

            if (rules.Count() > 0)
            {
                toReturn = new PredicateExpression();

                foreach (JqGridFilterItem item in rules)
                {
                    Predicate predicateToAdd = JgGridFilter.KreirajPredikatIzJqGridFilterItem(entityFieldsClass, item, getEntityFieldTypeFunction, ormEntityHelperClassesNamespace);

                    if (groupOp.ToUpper() == "AND")
                    {
                        toReturn.AddWithAnd(predicateToAdd);
                    }
                    else if (groupOp.ToUpper() == "OR")
                    {
                        toReturn.AddWithOr(predicateToAdd);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Preko refleksije izvlači EntityField2 za filtriranje iz rules.
        /// </summary>
        /// <param name="entityFieldsClass">Ako se filtriraju lijekovi onda LijekFields.</param>
        public static Predicate KreirajPredikatIzJqGridFilterItem(Type entityFieldsClass, 
			JqGridFilterItem jqGridFilterItem, 
			Func<string, Type> getEntityFieldTypeFunction,
		    string ormEntityHelperClassesNamespace)
        {
            string filterFieldName;
            string[] entityNames = jqGridFilterItem.field.Split('.');

            // Ako je zatočkano, npr. vraćeni field je "Dobavljac.Naziv",
            // tada entityFieldClass nije onaj poslani nego DobavljacEntity.
            if (entityNames.Count() > 1)
            {
				if (string.IsNullOrWhiteSpace (ormEntityHelperClassesNamespace)) 
				{
					throw new Exception ("OrmEntitiesHelperClassesNamespace is not set in config file");
				}

                filterFieldName = entityNames[entityNames.Count() - 1];
				string typeName = string.Format ("{0}.{1}Fields", ormEntityHelperClassesNamespace, entityNames [entityNames.Count () - 2]);
				entityFieldsClass = getEntityFieldTypeFunction (typeName); // Type.GetType(typeName);

				if (entityFieldsClass == null) 
				{
					throw new Exception (string.Format ("Type '{0}' does not exist", typeName)); 
				}
            }
            else
            {
                filterFieldName = jqGridFilterItem.field;
            }

            EntityField2 filterField = (EntityField2)entityFieldsClass.GetProperty(filterFieldName, BindingFlags.Public | BindingFlags.Static).GetValue(null, null);

            FieldLikePredicate toReturn = new FieldLikePredicate(filterField, null, string.Format("{0}%", jqGridFilterItem.data.ToUpper()));
            toReturn.CaseSensitiveCollation = true;

            return toReturn;
        }
    }

    public class JqGridFilterItem
    {
        public string field { get; set; }
        public string op { get; set; }
        public string data { get; set; }
    }
}
