using Nop.Api.Models.Common;
using Nop.Api.Models.Order;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;

namespace Nop.Api.Factories
{
    /// <summary>
    /// Represents the order model factory
    /// </summary>
    public class OrderModelFactory : IOrderModelFactory
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRewardPointService _rewardPointService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;
        
        #endregion

        #region Ctor

        public OrderModelFactory(
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceFormatter priceFormatter,
            IRewardPointService rewardPointService,
            IStoreContext storeContext,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings
            )
        {
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceFormatter = priceFormatter;
            _rewardPointService = rewardPointService;
            _storeContext = storeContext;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the customer order list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer order list model
        /// </returns>
        public async Task<CustomerOrderListModel> PrepareCustomerOrderListModelAsync(Customer customer)
        {
            var model = new CustomerOrderListModel();
            var store = await _storeContext.GetCurrentStoreAsync();
            var orders = await _orderService.SearchOrdersAsync(storeId: store.Id,
                customerId: customer.Id);
            foreach (var order in orders)
            {
                var orderModel = new CustomerOrderListModel.OrderDetailsModel
                {
                    Id = order.Id,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusEnum = order.OrderStatus,
                    OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                    IsReturnRequestAllowed = await _orderProcessingService.IsReturnRequestAllowedAsync(order),
                    CustomOrderNumber = order.CustomOrderNumber
                };
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);

                model.Orders.Add(orderModel);
            }

            var recurringPayments = await _orderService.SearchRecurringPaymentsAsync(store.Id,
                customer.Id);
            foreach (var recurringPayment in recurringPayments)
            {
                var order = await _orderService.GetOrderByIdAsync(recurringPayment.InitialOrderId);

                var recurringPaymentModel = new CustomerOrderListModel.RecurringOrderModel
                {
                    Id = recurringPayment.Id,
                    StartDate = (await _dateTimeHelper.ConvertToUserTimeAsync(recurringPayment.StartDateUtc, DateTimeKind.Utc)).ToString(),
                    CycleInfo = $"{recurringPayment.CycleLength} {await _localizationService.GetLocalizedEnumAsync(recurringPayment.CyclePeriod)}",
                    NextPayment = await _orderProcessingService.GetNextPaymentDateAsync(recurringPayment) is DateTime nextPaymentDate ? (await _dateTimeHelper.ConvertToUserTimeAsync(nextPaymentDate, DateTimeKind.Utc)).ToString() : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = await _orderProcessingService.GetCyclesRemainingAsync(recurringPayment),
                    InitialOrderId = order.Id,
                    InitialOrderNumber = order.CustomOrderNumber,
                    CanCancel = await _orderProcessingService.CanCancelRecurringPaymentAsync(customer, recurringPayment),
                    CanRetryLastPayment = await _orderProcessingService.CanRetryLastRecurringPaymentAsync(customer, recurringPayment)
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }

            return model;
        }

        /// <summary>
        /// Prepare the customer reward points model
        /// </summary>
        /// <param name="page">Number of items page; pass null to load the first page</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer reward points model
        /// </returns>
        public async Task<CustomerRewardPointsModel> PrepareCustomerRewardPointsAsync(Customer customer,int? page)
        {
            //get reward points history
            var store = await _storeContext.GetCurrentStoreAsync();
            var pageSize = _rewardPointsSettings.PageSize;
            var rewardPoints = await _rewardPointService.GetRewardPointsHistoryAsync(customer.Id, store.Id, true, pageIndex: --page ?? 0, pageSize: pageSize);

            //prepare model
            var model = new CustomerRewardPointsModel
            {
                RewardPoints = await rewardPoints.SelectAwait(async historyEntry =>
                {
                    var activatingDate = await _dateTimeHelper.ConvertToUserTimeAsync(historyEntry.CreatedOnUtc, DateTimeKind.Utc);
                    return new CustomerRewardPointsModel.RewardPointsHistoryModel
                    {
                        Points = historyEntry.Points,
                        PointsBalance = historyEntry.PointsBalance.HasValue ? historyEntry.PointsBalance.ToString()
                            : string.Format(await _localizationService.GetResourceAsync("RewardPoints.ActivatedLater"), activatingDate),
                        Message = historyEntry.Message,
                        CreatedOn = activatingDate,
                        EndDate = !historyEntry.EndDateUtc.HasValue ? null :
                            (DateTime?)(await _dateTimeHelper.ConvertToUserTimeAsync(historyEntry.EndDateUtc.Value, DateTimeKind.Utc))
                    };
                }).ToListAsync(),

                PagerModel = new PagerModel(_localizationService)
                {
                    PageSize = rewardPoints.PageSize,
                    TotalRecords = rewardPoints.TotalCount,
                    PageIndex = rewardPoints.PageIndex,
                    ShowTotalSummary = true,
                    RouteActionName = "CustomerRewardPointsPaged",
                    UseRouteLinks = true,
                    RouteValues = new RewardPointsRouteValues { PageNumber = page ?? 0 }
                }
            };

            //current amount/balance
            var rewardPointsBalance = await _rewardPointService.GetRewardPointsBalanceAsync(customer.Id, store.Id);
            var rewardPointsAmountBase = await _orderTotalCalculationService.ConvertRewardPointsToAmountAsync(rewardPointsBalance);
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var rewardPointsAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rewardPointsAmountBase, currentCurrency);
            model.RewardPointsBalance = rewardPointsBalance;
            model.RewardPointsAmount = await _priceFormatter.FormatPriceAsync(rewardPointsAmount, true, false);

            //minimum amount/balance
            var minimumRewardPointsBalance = _rewardPointsSettings.MinimumRewardPointsToUse;
            var minimumRewardPointsAmountBase = await _orderTotalCalculationService.ConvertRewardPointsToAmountAsync(minimumRewardPointsBalance);
            var minimumRewardPointsAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minimumRewardPointsAmountBase, currentCurrency);
            model.MinimumRewardPointsBalance = minimumRewardPointsBalance;
            model.MinimumRewardPointsAmount = await _priceFormatter.FormatPriceAsync(minimumRewardPointsAmount, true, false);

            return model;
        }

        #endregion
    }
}
