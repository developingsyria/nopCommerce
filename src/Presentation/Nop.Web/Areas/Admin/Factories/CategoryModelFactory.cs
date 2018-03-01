﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Extensions;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Kendoui;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the category model factory implementation
    /// </summary>
    public partial class CategoryModelFactory : BaseModelFactory, ICategoryModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public CategoryModelFactory(CatalogSettings catalogSettings,
            IAclService aclService,
            ICategoryService categoryService,
            ICategoryTemplateService categoryTemplateService,
            ICustomerService customerService,
            IDiscountService discountService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IStaticCacheManager cacheManager,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IVendorService vendorService) : base(languageService,
                storeMappingService,
                storeService)
        {
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._categoryService = categoryService;
            this._categoryTemplateService = categoryTemplateService;
            this._customerService = customerService;
            this._discountService = discountService;
            this._localizationService = localizationService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._cacheManager = cacheManager;
            this._storeService = storeService;
            this._vendorService = vendorService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare available templates for the passed model
        /// </summary>
        /// <param name="model">Category model</param>
        /// <param name="category">Category</param>
        protected virtual void PrepareModelTemplates(CategoryModel model, Category category)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare available category templates
            var availableTemplates = _categoryTemplateService.GetAllCategoryTemplates();
            model.AvailableCategoryTemplates = availableTemplates
                .Select(template => new SelectListItem { Text = template.Name, Value = template.Id.ToString() }).ToList();
        }

        /// <summary>
        /// Prepare available categories for the passed model
        /// </summary>
        /// <param name="model">Category model</param>
        /// <param name="category">Category</param>
        protected virtual void PrepareModelCategories(CategoryModel model, Category category)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare available categories
            model.AvailableCategories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, showHidden: true);

            //insert special category item for the "none" value
            model.AvailableCategories.Insert(0, new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Categories.Fields.Parent.None"),
                Value = "0"
            });
        }

        /// <summary>
        /// Prepare selected and all available discounts for the passed model
        /// </summary>
        /// <param name="model">Category model</param>
        /// <param name="category">Category</param>
        /// <param name="ignoreAppliedDiscounts">Whether to ignore existing applied discounts</param>
        protected virtual void PrepareDiscountModel(CategoryModel model, Category category, bool ignoreAppliedDiscounts)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //try to get already applied discounts
            if (!ignoreAppliedDiscounts && category != null)
                model.SelectedDiscountIds = category.AppliedDiscounts.Select(discount => discount.Id).ToList();

            //prepare available discounts
            var availableDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
            model.AvailableDiscounts = availableDiscounts.Select(discount => new SelectListItem
            {
                Text = discount.Name,
                Value = discount.Id.ToString(),
                Selected = model.SelectedDiscountIds.Contains(discount.Id)
            }).ToList();
        }

        /// <summary>
        /// Prepare selected and all available customer roles for the passed model
        /// </summary>
        /// <param name="model">Category model</param>
        /// <param name="category">Category</param>
        /// <param name="ignoreAclMappings">Whether to ignore existing acl mappings</param>
        protected virtual void PrepareModelCustomerRoles(CategoryModel model, Category category, bool ignoreAclMappings)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //try to get customer role identifiers with granted access
            if (!ignoreAclMappings && category != null)
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(category).ToList();

            //prepare available customer roles
            var availableRoles = _customerService.GetAllCustomerRoles(showHidden: true);
            model.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Id.ToString(),
                Selected = model.SelectedCustomerRoleIds.Contains(role.Id)
            }).ToList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare category list model
        /// </summary>
        /// <param name="model">Category list model</param>
        /// <returns>Category list model</returns>
        public virtual CategoryListModel PrepareCategoryListModel(CategoryListModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare available stores
            var availableStores = _storeService.GetAllStores();
            model.AvailableStores = availableStores
                .Select(store => new SelectListItem { Text = store.Name, Value = store.Id.ToString() }).ToList();

            //insert special store item for the "all" value
            model.AvailableStores.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return model;
        }

        /// <summary>
        /// Prepare paged category list model for the grid
        /// </summary>
        /// <param name="listModel">Category list model</param>
        /// <param name="command">Pagination parameters</param>
        /// <returns>Grid model</returns>
        public virtual DataSourceResult PrepareCategoryListGridModel(CategoryListModel listModel, DataSourceRequest command)
        {
            if (listModel == null)
                throw new ArgumentNullException(nameof(listModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            //get categories
            var categories = _categoryService.GetAllCategories(categoryName: listModel.SearchCategoryName,
                showHidden: true,
                storeId: listModel.SearchStoreId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            //prepare grid model
            var model = new DataSourceResult
            {
                Data = categories.Select(category =>
                {
                    //fill in model values from the entity
                    var categoryModel = category.ToModel();

                    //fill in additional values (not existing in the entity)
                    categoryModel.Breadcrumb = category.GetFormattedBreadCrumb(_categoryService);

                    return categoryModel;
                }),
                Total = categories.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare category model
        /// </summary>
        /// <param name="model">Category model</param>
        /// <param name="category">Category</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Category model</returns>
        public virtual CategoryModel PrepareCategoryModel(CategoryModel model, Category category, bool excludeProperties = false)
        {
            Action<CategoryLocalizedModel, int> localizedModelConfiguration = null;

            if (category != null)
            {
                //fill in model values from the entity
                model = model ?? category.ToModel();

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = category.GetLocalized(entity => entity.Name, languageId, false, false);
                    locale.Description = category.GetLocalized(entity => entity.Description, languageId, false, false);
                    locale.MetaKeywords = category.GetLocalized(entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = category.GetLocalized(entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = category.GetLocalized(entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = category.GetSeName(languageId, false, false);
                };
            }

            //set default values for the new model
            if (category == null)
            {
                model.PageSize = _catalogSettings.DefaultCategoryPageSize;
                model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
                model.Published = true;
                model.IncludeInTopMenu = true;
                model.AllowCustomersToSelectPageSize = true;
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = PrepareLocalizedModels(localizedModelConfiguration);

            //prepare model templates
            PrepareModelTemplates(model, category);

            //prepare available model categories
            PrepareModelCategories(model, category);

            //prepare model discounts
            PrepareDiscountModel(model, category, excludeProperties);

            //prepare model customer roles
            PrepareModelCustomerRoles(model, category, excludeProperties);

            //prepare model stores
            PrepareModelStores(model, category, excludeProperties);

            return model;
        }

        /// <summary>
        /// Prepare paged category product list model for the grid
        /// </summary>
        /// <param name="command">Pagination parameters</param>
        /// <param name="category">Category</param>
        /// <returns>Grid model</returns>
        public virtual DataSourceResult PrepareCategoryProductListGridModel(DataSourceRequest command, Category category)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            //get product categories
            var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id,
                showHidden: true,
                pageIndex: command.Page - 1, pageSize: command.PageSize);

            //prepare grid model
            var model = new DataSourceResult
            {
                //fill in model values from the entity
                Data = productCategories.Select(productCategory => new CategoryModel.CategoryProductModel
                {
                    Id = productCategory.Id,
                    CategoryId = productCategory.CategoryId,
                    ProductId = productCategory.ProductId,
                    ProductName = _productService.GetProductById(productCategory.ProductId)?.Name,
                    IsFeaturedProduct = productCategory.IsFeaturedProduct,
                    DisplayOrder = productCategory.DisplayOrder
                }),
                Total = productCategories.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare add category product list model
        /// </summary>
        /// <param name="model">Add category product list model</param>
        /// <returns>Add category product list model</returns>
        public virtual CategoryModel.AddCategoryProductModel PrepareAddCategoryProductListModel(CategoryModel.AddCategoryProductModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare available categories
            model.AvailableCategories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);

            //prepare available manufacturers
            model.AvailableManufacturers = SelectListHelper.GetManufacturerList(_manufacturerService, _cacheManager, true);

            //prepare available stores
            var availableStores = _storeService.GetAllStores();
            model.AvailableStores = availableStores
                .Select(store => new SelectListItem { Text = store.Name, Value = store.Id.ToString() }).ToList();

            //prepare available vendors
            model.AvailableStores = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);

            //prepare available product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

            //insert special item for the "all" value
            var allSelectListItem = new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" };
            model.AvailableCategories.Insert(0, allSelectListItem);
            model.AvailableManufacturers.Insert(0, allSelectListItem);
            model.AvailableStores.Insert(0, allSelectListItem);
            model.AvailableVendors.Insert(0, allSelectListItem);
            model.AvailableProductTypes.Insert(0, allSelectListItem);

            return model;
        }

        /// <summary>
        /// Prepare paged add category product list model for the grid
        /// </summary>
        /// <param name="model">Add category product list model</param>
        /// <param name="command">Pagination parameters</param>
        /// <returns>Grid model</returns>
        public virtual DataSourceResult PrepareAddCategoryProductListGridModel(CategoryModel.AddCategoryProductModel listModel,
            DataSourceRequest command)
        {
            if (listModel == null)
                throw new ArgumentNullException(nameof(listModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { listModel.SearchCategoryId },
                manufacturerId: listModel.SearchManufacturerId,
                storeId: listModel.SearchStoreId,
                vendorId: listModel.SearchVendorId,
                productType: listModel.SearchProductTypeId > 0 ? (ProductType?)listModel.SearchProductTypeId : null,
                keywords: listModel.SearchProductName,
                pageIndex: command.Page - 1, pageSize: command.PageSize);

            //prepare grid model
            var model = new DataSourceResult
            {
                //fill in model values from the entity
                Data = products.Select(product => product.ToModel()),
                Total = products.TotalCount
            };

            return model;
        }

        #endregion
    }
}