using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Web.Mvc;
using NinjaSoftware.Api.Core;
using System.Configuration;

namespace NinjaSoftware.Api.Mvc
{
    public class NinjaSoftwareController : Controller
    {
        /// <summary>
        /// If TryUpdateModel passes, it opens transaction and call IViewModel.Save.
        /// If IViewModel.Save passes, it commits transaction and returns true.
        /// If general exception is thrown, it rollbacks transaction and rethrows exception.
        /// If UserException or ORMConcurrencyException is thrown, it rollbacks transaction and adds ModelError with exception message.
        /// </summary>
        public bool TryUpdateAndSaveIViewModel<T>(T viewModel, DataAccessAdapterBase adapter) 
            where T : class, IViewModel
        {
            return TryUpdateAndSaveModel(viewModel, adapter, null, null);
        }

        /// <summary>
        /// If TryUpdateModel passes, it opens transaction and try to save IEntity2.
        /// If IEntity2 save passes, it commits transaction and returns true.
        /// If general exception is thrown, it rollbacks transaction and rethrows exception.
        /// If UserException or ORMConcurrencyException is thrown, it rollbacks transaction and adds ModelError with exception message.
        /// </summary>
        public bool TryUpdateAndSaveIEntity2<T>(T entity, DataAccessAdapterBase adapter, bool refetchAfterSave, bool recurse)
            where T : class, IEntity2
        {
            return TryUpdateAndSaveModel(entity, adapter, refetchAfterSave, recurse);
        }

        /// <summary>
        /// Generic method for saving IViewModel and IEntity2.
        /// Method is expecting 'ORMConcurrencyExceptionMessage' key in AppSettings.
        /// </summary>
        private bool TryUpdateAndSaveModel<T>(T model, DataAccessAdapterBase adapter, bool? refetchAfterSave, bool? recurse)
            where T : class
        {
            if (TryUpdateModel(model))
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
                        string exceptionMessage = string.Format("SaveModel don't support type {0}.", model.GetType().Name);
                        throw new NotImplementedException(exceptionMessage);
                    }

                    adapter.Commit();

                    return true;
                }
                catch (UserException ex)
                {
                    adapter.Rollback();
                    ModelState.AddModelError(ex.GetType().Name, ex.Message);

                    return false;
                }
                catch (ORMConcurrencyException ex)
                {
                    adapter.Rollback();

                    string errorMessage = ConfigurationManager.AppSettings["ORMConcurrencyExceptionMessage"];
                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage = "Other user have changed data. Reload data and enter changes.";
                    }

                    ModelState.AddModelError(ex.GetType().Name, errorMessage);

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
    }
}
