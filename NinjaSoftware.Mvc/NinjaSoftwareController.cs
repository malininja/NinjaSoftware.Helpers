using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Web.Mvc;
using NinjaSoftware.Core;
using System.Configuration;

namespace NinjaSoftware.Mvc
{
    public class NinjaSoftwareController : Controller
    {
        #region SaveModel

        /// <summary>
        /// If TryUpdateModel passes, it opens transaction and call IViewModel.Save.
        /// If IViewModel.Save passes, it commits transaction and returns true.
        /// If general exception is thrown, it rollbacks transaction and rethrows exception.
        /// If UserException or ORMConcurrencyException is thrown, it rollbacks transaction and adds ModelError with exception message.
        /// </summary>
        public bool PohraniViewModel(IViewModel viewModel, DataAccessAdapterBase adapter)
        {
            return SaveModel(viewModel, adapter, null, null);
        }

        /// <summary>
        /// If TryUpdateModel passes, it opens transaction and try to save IEntity2.
        /// If IEntity2 save passes, it commits transaction and returns true.
        /// If general exception is thrown, it rollbacks transaction and rethrows exception.
        /// If UserException or ORMConcurrencyException is thrown, it rollbacks transaction and adds ModelError with exception message.
        /// </summary>
        public bool PohraniCommonEntityBase(IEntity2 entity, DataAccessAdapterBase adapter, bool refetchAfterSave, bool recurse)
        {
            return SaveModel(entity, adapter, null, null);
        }

        /// <summary>
        /// Used when model is passed as interface or base class, and update properties are defined in other type.
        /// </summary>
        private bool CustomTryUpdateModel(object model, Type viewModelType)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (ValueProvider == null)
            {
                throw new ArgumentNullException("valueProvider");
            }

            Predicate<string> propertyFilter = propertyName => true;
            IModelBinder binder = Binders.GetBinder(viewModelType);

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, viewModelType),
                ModelState = ModelState,
                PropertyFilter = propertyFilter,
                ValueProvider = ValueProvider
            };
            binder.BindModel(ControllerContext, bindingContext);

            return ModelState.IsValid;
        }

        /// <summary>
        /// Generic method for saving IViewModel and IEntity2.
        /// Method is expecting 'ORMConcurrencyExceptionMessage' key in AppSettings.
        /// </summary>
        private bool SaveModel(object model, DataAccessAdapterBase adapter, bool? refetchAfterSave, bool? recurse)
        {
            Type type = model.GetType();

            if (CustomTryUpdateModel(model, type))
            {
                try
                {
                    adapter.StartTransaction(System.Data.IsolationLevel.Serializable, "SaveModel");

                    if (model is IViewModel)
                    {
                        ((IViewModel)model).Save(adapter);
                    }
                    else if (model is IEntity2)
                    {
                        adapter.SaveEntity((IEntity2)model, refetchAfterSave.Value, recurse.Value);
                    }
                    else
                    {
                        string exceptionMessage = string.Format("SaveModel doesn't support type {0}.", type.Name);
                        throw new NotImplementedException(exceptionMessage);
                    }

                    adapter.Commit();

                    return true;
                }
                catch (UserException ex)
                {
                    adapter.Rollback();
                    ModelState.AddModelError("UserException", ex.Message);

                    return false;
                }
                catch (ORMConcurrencyException)
                {
                    adapter.Rollback();

                    string errorMessage = ConfigurationManager.AppSettings["ORMConcurrencyExceptionMessage"];
                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage = "Other user have changed data. Reload date and enter changes.";
                    }

                    ModelState.AddModelError("ORMConcurrencyException", errorMessage);

                    return false;
                }
                catch (Exception)
                {
                    adapter.Rollback();
                    throw;
                }
                finally
                {
                    adapter.Dispose();
                }
            }
            else
            {
                adapter.Dispose();
                return false;
            }
        }

        #endregion
    }
}
