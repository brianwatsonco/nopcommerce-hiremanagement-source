using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Misc.HireManagement.Services;

public class HireShoppingCartService : ShoppingCartService
{
    private readonly IHireBookingService _hireBookingService;

    public HireShoppingCartService(CatalogSettings catalogSettings,
        IAclService aclService,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateRangeService dateRangeService,
        IDateTimeHelper dateTimeHelper,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IRepository<ShoppingCartItem> sciRepository,
        IShippingService shippingService,
        IShortTermCacheManager shortTermCacheManager,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreService storeService,
        IStoreMappingService storeMappingService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        LinkGenerator linkGenerator,
        OrderSettings orderSettings,
        ShoppingCartSettings shoppingCartSettings,
        IHireBookingService hireBookingService)
        : base(catalogSettings,
              aclService,
              checkoutAttributeParser,
              checkoutAttributeService,
              currencyService,
              customerService,
              dateRangeService,
              dateTimeHelper,
              eventPublisher,
              genericAttributeService,
              giftCardService,
              httpContextAccessor,
              localizationService,
              permissionService,
              priceCalculationService,
              priceFormatter,
              productAttributeParser,
              productAttributeService,
              productService,
              sciRepository,
              shippingService,
              shortTermCacheManager,
              staticCacheManager,
              storeContext,
              storeService,
              storeMappingService,
              urlRecordService,
              workContext,
              linkGenerator,
              orderSettings,
              shoppingCartSettings)
    {
        _hireBookingService = hireBookingService;
    }

    public override async Task<IList<string>> GetShoppingCartItemWarningsAsync(Customer customer, ShoppingCartType shoppingCartType,
        Product product, int storeId,
        string attributesXml, decimal customerEnteredPrice,
        DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
        int quantity = 1, bool addRequiredProducts = true, int shoppingCartItemId = 0,
        bool getStandardWarnings = true, bool getAttributesWarnings = true,
        bool getGiftCardWarnings = true, bool getRequiredProductWarnings = true,
        bool getRentalWarnings = true)
    {
        var warnings = await base.GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product, storeId, attributesXml,
            customerEnteredPrice, rentalStartDate, rentalEndDate, quantity, addRequiredProducts, shoppingCartItemId,
            getStandardWarnings, getAttributesWarnings, getGiftCardWarnings, getRequiredProductWarnings, getRentalWarnings);

        if (warnings.Any())
            return warnings;

        if (!product.IsRental || shoppingCartType != ShoppingCartType.ShoppingCart || !rentalStartDate.HasValue || !rentalEndDate.HasValue)
            return warnings;

        var normalizedQuantity = quantity < 1 ? 1 : quantity;
        var available = await _hireBookingService.IsSelectionAvailableAsync(product.Id, rentalStartDate.Value, rentalEndDate.Value, normalizedQuantity);
        if (!available)
            warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Front.Messages.CartConflict"));

        if (warnings.Any())
            return warnings;

        // Prevent overbooking by combining overlapping rental lines already in the cart
        var cartItems = await GetShoppingCartAsync(customer,
            shoppingCartType: ShoppingCartType.ShoppingCart,
            storeId: storeId,
            productId: product.Id);

        var overlappingQuantity = 0;
        foreach (var item in cartItems)
        {
            if (shoppingCartItemId > 0 && item.Id == shoppingCartItemId)
                continue;

            if (!item.RentalStartDateUtc.HasValue || !item.RentalEndDateUtc.HasValue)
                continue;

            if (!Intersects(rentalStartDate.Value, rentalEndDate.Value, item.RentalStartDateUtc.Value, item.RentalEndDateUtc.Value))
                continue;

            overlappingQuantity += Math.Max(item.Quantity, 1);
        }

        var capacity = Math.Max(product.StockQuantity, 1);
        if (overlappingQuantity + normalizedQuantity > capacity)
            warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Front.Messages.CartConflict"));

        return warnings;
    }

    public override async Task<(decimal unitPrice, decimal discountAmount, List<Discount> appliedDiscounts)> GetUnitPriceAsync(Product product,
        Customer customer,
        Store store,
        ShoppingCartType shoppingCartType,
        int quantity,
        string attributesXml,
        decimal customerEnteredPrice,
        DateTime? rentalStartDate,
        DateTime? rentalEndDate,
        bool includeDiscounts)
    {
        if (product?.IsRental == true && rentalStartDate.HasValue && rentalEndDate.HasValue && !string.IsNullOrEmpty(attributesXml))
        {
            var selectedDurationDays = await GetSelectedDurationDaysAsync(attributesXml);
            if (selectedDurationDays.HasValue && selectedDurationDays.Value > 0)
            {
                var normalizedDaysForPricing = selectedDurationDays.Value > 7 ? 7 : selectedDurationDays.Value;
                rentalEndDate = rentalStartDate.Value.AddDays(normalizedDaysForPricing);
            }
        }

        return await base.GetUnitPriceAsync(product,
            customer,
            store,
            shoppingCartType,
            quantity,
            attributesXml,
            customerEnteredPrice,
            rentalStartDate,
            rentalEndDate,
            includeDiscounts);
    }

    private async Task<int?> GetSelectedDurationDaysAsync(string attributesXml)
    {
        var values = await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml);
        if (values == null || values.Count == 0)
            return null;

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value?.Name))
                continue;

            var match = Regex.Match(value.Name, "(\\d+)\\s*day", RegexOptions.IgnoreCase);
            if (!match.Success)
                continue;

            if (int.TryParse(match.Groups[1].Value, out var parsedDays) && parsedDays > 0)
                return parsedDays;
        }

        return null;
    }

    private static bool Intersects(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
    {
        return aStart <= bEnd && bStart <= aEnd;
    }

    public override async Task<IList<string>> GetShoppingCartWarningsAsync(IList<ShoppingCartItem> shoppingCart,
        string checkoutAttributesXml, bool validateCheckoutAttributes)
    {
        var warnings = await base.GetShoppingCartWarningsAsync(shoppingCart, checkoutAttributesXml, validateCheckoutAttributes);

        foreach (var item in shoppingCart)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null || !product.IsRental)
                continue;

            if (!item.RentalStartDateUtc.HasValue || !item.RentalEndDateUtc.HasValue)
                continue;

            var normalizedQuantity = item.Quantity < 1 ? 1 : item.Quantity;
            var available = await _hireBookingService.IsSelectionAvailableAsync(product.Id,
                item.RentalStartDateUtc.Value,
                item.RentalEndDateUtc.Value,
                normalizedQuantity);

            if (!available)
            {
                var warning = await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Front.Messages.CartConflict");
                if (!warnings.Contains(warning))
                    warnings.Add(warning);
            }
        }

        return warnings;
    }
}
