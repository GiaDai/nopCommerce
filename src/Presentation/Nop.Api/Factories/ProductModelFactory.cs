using Nop.Api.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Helpers;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Services.Localization;
using Nop.Api.Models.Common;

namespace Nop.Api.Factories
{
    public class ProductModelFactory : IProductModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public ProductModelFactory(
            CatalogSettings catalogSettings,
            IDateTimeHelper dateTimeHelper,
            IStoreContext storeContext,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IReviewTypeService reviewTypeService,
            ILocalizationService localizationService
            )
        {
            _catalogSettings = catalogSettings;
            _dateTimeHelper = dateTimeHelper;
            _storeContext = storeContext;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _reviewTypeService = reviewTypeService;
            _localizationService = localizationService;
        }

        #endregion

        /// <summary>
        /// Prepare the customer product reviews model
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="page">Number of items page; pass null to load the first page</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer product reviews model
        /// </returns>
        public async Task<CustomerProductReviewsModel> PrepareCustomerProductReviewsModelAsync(Customer customer, int? page)
        {
            var pageSize = _catalogSettings.ProductReviewsPageSizeOnAccountPage;
            var pageIndex = 0;

            if (page > 0)
            {
                pageIndex = page.Value - 1;
            }

            var store = await _storeContext.GetCurrentStoreAsync();

            var list = await _productService.GetAllProductReviewsAsync(
                customerId: customer.Id,
            approved: null,
                storeId: _catalogSettings.ShowProductReviewsPerStore ? store.Id : 0,
                pageIndex: pageIndex,
                pageSize: pageSize);

            var productReviews = new List<CustomerProductReviewModel>();

            foreach (var review in list)
            {
                var product = await _productService.GetProductByIdAsync(review.ProductId);

                var productReviewModel = new CustomerProductReviewModel
                {
                    Title = review.Title,
                    ProductId = product.Id,
                    ProductName = await _localizationService.GetLocalizedAsync(product, p => p.Name),
                    ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                    Rating = review.Rating,
                    ReviewText = review.ReviewText,
                    ReplyText = review.ReplyText,
                    WrittenOnStr = (await _dateTimeHelper.ConvertToUserTimeAsync(review.CreatedOnUtc, DateTimeKind.Utc)).ToString("g")
                };

                if (_catalogSettings.ProductReviewsMustBeApproved)
                {
                    productReviewModel.ApprovalStatus = review.IsApproved
                        ? await _localizationService.GetResourceAsync("Account.CustomerProductReviews.ApprovalStatus.Approved")
                        : await _localizationService.GetResourceAsync("Account.CustomerProductReviews.ApprovalStatus.Pending");
                }

                foreach (var q in await _reviewTypeService.GetProductReviewReviewTypeMappingsByProductReviewIdAsync(review.Id))
                {
                    var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(q.ReviewTypeId);

                    productReviewModel.AdditionalProductReviewList.Add(new ProductReviewReviewTypeMappingModel
                    {
                        ReviewTypeId = q.ReviewTypeId,
                        ProductReviewId = review.Id,
                        Rating = q.Rating,
                        Name = await _localizationService.GetLocalizedAsync(reviewType, x => x.Name),
                    });
                }

                productReviews.Add(productReviewModel);
            }

            var pagerModel = new PagerModel(_localizationService)
            {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerProductReviewsPaged",
                UseRouteLinks = true,
                RouteValues = new CustomerProductReviewsModel.CustomerProductReviewsRouteValues { PageNumber = pageIndex }
            };

            var model = new CustomerProductReviewsModel
            {
                ProductReviews = productReviews,
                PagerModel = pagerModel
            };

            return model;
        }
    }
}
