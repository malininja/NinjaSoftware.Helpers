using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;
using Newtonsoft.Json;

namespace NinjaSoftware.Api.CoolJ
{
    public static class EntityCollectionExtensions
    {
        /// <summary>
        /// Updates EntityCollection entities with provided jsonData.
        /// If EntityCollection contains entites which are not included in jsonData, they won't be updated.
        /// If jsonData contains new entites (primary key is 0 or null), they will be added to EntityCollecton.
        /// </summary>
        public static void UpdateEntityCollectionFromJson<TEntity>(this EntityCollectionBase2<TEntity> entityCollection,
            string jsonData,
            EntityField2 primaryKeyField,
            string[] includeFields,
            string[] excludeFields,
            JsonSerializerSettings jsonSettings)
            where TEntity : EntityBase2, IEntity2
        {
            IEnumerable<TEntity> deserializedCollection = JsonConvert.DeserializeObject<IEnumerable<TEntity>>(jsonData, jsonSettings);

            if (deserializedCollection.Count() > 0)
            {
                foreach (TEntity entity in entityCollection)
                {
                    TEntity entityWhichUpdates = deserializedCollection.Where(p => 
                        object.Equals(p.Fields[primaryKeyField.Name].CurrentValue, entity.Fields[primaryKeyField.Name].CurrentValue)).SingleOrDefault();

                    if (null != entityWhichUpdates)
                    {
                        entity.UpdateDataFromOtherObject(entityWhichUpdates, includeFields, excludeFields);
                    }
                }

                foreach (TEntity entity in deserializedCollection.Where(e =>
                    object.Equals(e.Fields[primaryKeyField.Name].CurrentValue, 0L) || e.Fields[primaryKeyField.Name].CurrentValue == null))
                {
                    entityCollection.Add(entity);
                }
            }
        }

        public static List<TEntity> GetEntitiesNotIncludedInJson<TEntity>(this EntityCollectionBase2<TEntity> entityCollection, string jsonData)
            where TEntity : EntityBase2, IEntity2
        {
            List<TEntity> toReturn = new List<TEntity>();

            if (entityCollection.Count > 0)
            {
                IEnumerable<TEntity> deserializedCollection = JsonConvert.DeserializeObject<IEnumerable<TEntity>>(jsonData);
                string pkName = entityCollection[0].GetPrimaryKeyName();

                foreach (TEntity entity in entityCollection)
                {
                    TEntity entityWhichUpdates = deserializedCollection.Where(e => 
                        object.Equals(e.Fields[pkName].CurrentValue, entity.Fields[pkName].CurrentValue)).SingleOrDefault();

                    if (null == entityWhichUpdates)
                    {
                        toReturn.Add(entity);
                    }
                }
            }

            return toReturn;
        }
    }
}
