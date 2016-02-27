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

		/// <summary>
		/// Creates predicate based on jgGridFilterString
		/// </summary>
		/// <returns>PredicateExpression</returns>
		/// <param name="jqGridFilterString">Serialized JSON returned from JqGrid</param>
		/// <param name="entityType">If you wanna filter PartnerEntity then typeof(PartnerFields)</param>
		/// <param name="getEntityFieldTypeFunction">Function excepts string (EntityFields type name) and returnes type. In case that you need filtering by entity linked to main entity (Partner.Country), then this function is used to get CountryFields via reflection.</param>
        public static PredicateExpression CreatePredicateFromJqGridFilterString(string jqGridFilterString, 
        	Type entityType,
			Func<string, Type> getEntityFieldTypeFunction)
        {
            JqGridFilter jqGridFilter = JsonConvert.DeserializeObject<JqGridFilter>(jqGridFilterString);

            if (jqGridFilter != null)
            {
                return jqGridFilter.KreirajPredicate(entityType, getEntityFieldTypeFunction);
            }
            else
            {
                return null;
            }
        }
    }

    public class JqGridFilter
    {
        public string groupOp { get; set; }
        public IEnumerable<JqGridFilterItem> rules { get; set; }

        /// <summary>
        /// Za svaki član this.rules kolekcije kreira jedan Predicate.
        /// Ako je rules.Count == 0 vraća NULL
        /// </summary>
        /// <returns>PredicateExpression</returns>
        /// <param name="entityFieldsType">Ako se filtrira artikl onda typeof(ArtiklFields)</param>
		/// <param name="getEntityFieldTypeFunction">Funkcija prima string (EntityFields type name) a vraća tip. Ako se desi da se filtrira po entitetu vezanom na glavni entitet (RacunGlava.Partner) tada se koristi ova funkcija za dohvat EntityFieldsType preko refleksije.</param>
		public PredicateExpression KreirajPredicate(Type entityFieldsType, 
                                            Func<string, Type> getEntityFieldTypeFunction)
        {
            PredicateExpression toReturn = null;

            if (rules.Count() > 0)
            {
                toReturn = new PredicateExpression();

                foreach (JqGridFilterItem item in rules)
                {
                    Predicate predicateToAdd = JqGridFilter.KreirajPredikatIzJqGridFilterItem(entityFieldsType, item, getEntityFieldTypeFunction);
                    
                    if (predicateToAdd != null)
                    {
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
            }

            return toReturn;
        }
       
        /// <summary>
        /// Kreira predikat za JqGridFilterItem
        /// </summary>
        /// <returns>The predikat iz jq grid filter item.</returns>
        /// <param name="entityFieldsType">Ako se filtrira artikl onda typeof(ArtiklFields)</param>
        /// <param name="jqGridFilterItem">Jedan item iz this.rules kolekcije</param>
        /// <param name="getEntityFieldTypeFunction">Funkcija prima string (EntityFields type name) a vraća tip. Ako se desi da se filtrira po entitetu vezanom na glavni entitet (RacunGlava.Partner) tada se koristi ova funkcija za dohvat EntityFieldsType preko refleksije.</param>
        public static Predicate KreirajPredikatIzJqGridFilterItem(Type entityFieldsType, 
                                                          JqGridFilterItem jqGridFilterItem, 
                                                          Func<string, Type> getEntityFieldTypeFunction)
        {
            string filterFieldName;
            string[] entityNames = jqGridFilterItem.field.Split('.');

            // Ako je zatočkano, npr. vraćeni field je "Dobavljac.Naziv",
            // tada entityFieldClass nije onaj poslani nego DobavljacEntity.
            if (entityNames.Count() > 1)
            {
                filterFieldName = entityNames[entityNames.Count() - 1];
                string typeName = string.Format("{0}Fields", entityNames[entityNames.Count() - 2]);
                entityFieldsType = getEntityFieldTypeFunction(typeName); 

                if (entityFieldsType == null)
                {
                    throw new Exception(string.Format("Type '{0}' does not exist", typeName)); 
                }
            }
            else
            {
                filterFieldName = jqGridFilterItem.field;
            }

            EntityField2 filterField = (EntityField2)entityFieldsType.GetProperty(filterFieldName, BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            
            return CreatePredicateForType(filterField, jqGridFilterItem.data);
        }
        
        private static Predicate CreatePredicateForType(EntityField2 filterField, string data)
        {
            Predicate predicate = null;
            
            if (filterField.DataType == typeof(string))
            {
                FieldLikePredicate likePredicate = new FieldLikePredicate(filterField, null, string.Format("{0}%", data.ToUpper()));
                likePredicate.CaseSensitiveCollation = true;
                
                predicate = likePredicate;
            }
            else if (filterField.DataType == typeof(bool))
            {
                bool boolData;
                
                if (bool.TryParse(data, out boolData))
                {
                    predicate = filterField == boolData;
                }
            }
            else if (filterField.DataType == typeof(byte))
            {
                byte byteData;
                
                if (byte.TryParse(data, out byteData))
                {
                    predicate = filterField == byteData;
                }
            }
            else if (filterField.DataType == typeof(short))
            {
                short shortData;
                
                if (short.TryParse(data, out shortData))
                {
                    predicate = filterField == shortData;
                }
            }
            else if (filterField.DataType == typeof(int))
            {
                int intData;
                
                if (int.TryParse(data, out intData))
                {
                    predicate = filterField == intData;
                }
            }
            else if (filterField.DataType == typeof(long))
            {
                long longData;
                
                if (long.TryParse(data, out longData))
                {
                    predicate = filterField == longData;
                }
            }
            else if (filterField.DataType == typeof(Single))
            {
                Single singleData;
                
                if (Single.TryParse(data, out singleData))
                {
                    predicate = filterField == singleData;
                }
            }
            else if (filterField.DataType == typeof(double))
            {
                double doubleData;
                
                if (double.TryParse(data, out doubleData))
                {
                    predicate = filterField == doubleData;
                }
            }
            else if (filterField.DataType == typeof(decimal))
            {
                decimal decimalData;
                
                if (decimal.TryParse(data, out decimalData))
                {
                    predicate = filterField == decimalData;
                }
            }
            
            return predicate;
        }
    }

    public class JqGridFilterItem
    {
        public string field { get; set; }
        public string op { get; set; }
        public string data { get; set; }
    }
}
