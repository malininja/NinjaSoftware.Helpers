using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace NinjaSoftware.Api.CoolJ
{
    public static class IEntity2Extensions
    {
        /// <summary>
        /// Sets Field.CurrentValue from entityWhichUpdates to entityToUpdate.
        /// Doesn't change primary key field.
        /// </summary>
        public static void UpdateDataFromOtherObject(this IEntity2 entityToUpdate,
            IEntity2 entityWhichUpdates,
            string[] includeFields,
            string[] excludeFields)
        {
            foreach (IEntityField2 field in entityToUpdate.Fields)
            {
                if ((null == includeFields || includeFields.Contains(field.Name)) &&
                    (null == includeFields || !excludeFields.Contains(field.Name)))
                {
                    if (!field.IsPrimaryKey &&
                        !object.Equals(field.CurrentValue, entityWhichUpdates.Fields[field.Name].CurrentValue))
                    {
                        field.CurrentValue = entityWhichUpdates.Fields[field.Name].CurrentValue;
                        field.IsChanged = true;
                        entityToUpdate.IsDirty = true;
                    }
                }
            }
        }

        public static string GetPrimaryKeyName(this IEntity2 entity)
        {
            string toReturn = null;

            foreach (IEntityField2 field in entity.Fields)
            {
                if (field.IsPrimaryKey)
                {
                    toReturn = field.Name;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(toReturn))
            {
                throw new ORMException("Entity must have a primary key.");
            }

            return toReturn;
        }
    }
}
