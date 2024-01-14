using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using Nop.Api.Models.Customer;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Tax;
using Nop.Services.Helpers;
using Nop.Services.Messages;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Directory;
using Nop.Services.Authentication.External;
using Nop.Services.Gdpr;
using Nop.Core.Domain.Common;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Api.Models.Common;

namespace Nop.Api.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields
        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;
        #endregion

        #region Ctor

        public CustomerModelFactory(
            AddressSettings addressSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings, 
            ExternalAuthenticationSettings externalAuthenticationSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressModelFactory addressModelFactory,
            IAuthenticationPluginManager authenticationPluginManager,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            TaxSettings taxSettings
        )
        {
            _addressSettings = addressSettings;
            _customerSettings = customerSettings;
            _countryService = countryService;
            _customerService = customerService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _dateTimeSettings = dateTimeSettings;
            _externalAuthenticationService = externalAuthenticationService;
            _gdprService = gdprService;
            _addressModelFactory = addressModelFactory;
            _gdprSettings = gdprSettings;
            _forumSettings = forumSettings;
            _authenticationPluginManager = authenticationPluginManager;
            _dateTimeHelper = dateTimeHelper;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _taxSettings = taxSettings;

        }



        #endregion

        #region Utilities

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the customer address list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer address list model
        /// </returns>
        public virtual async Task<CustomerAddressListModel> PrepareCustomerAddressListModelAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var addresses = await (await _customerService.GetAddressesByCustomerIdAsync(customer.Id))
                //enabled for the current store
                .WhereAwait(async a => a.CountryId == null || await _storeMappingService.AuthorizeAsync(await _countryService.GetCountryByAddressAsync(a)))
                .ToListAsync();

            var model = new CustomerAddressListModel();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                    address: address,
                excludeProperties: false,
                    addressSettings: _addressSettings,
                    loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
                model.Addresses.Add(addressModel);
            }
            return model;
        }

        /// <summary>
        /// Prepare the customer info model
        /// </summary>
        /// <param name="model">Customer info model</param>
        /// <param name="customer">Customer</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomCustomerAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer info model
        /// </returns>
        public async Task<CustomerInfoModel> PrepareCustomerInfoModelAsync(CustomerInfoModel model, Customer customer, 
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == (await _dateTimeHelper.GetCurrentTimeZoneAsync()).Id) });

            var store = await _storeContext.GetCurrentStoreAsync();
            if (!excludeProperties)
            {
                model.VatNumber = customer.VatNumber;
                model.FirstName = customer.FirstName;
                model.LastName = customer.LastName;
                model.Gender = customer.Gender;
                var dateOfBirth = customer.DateOfBirth;
                if (dateOfBirth.HasValue)
                {
                    var currentCalendar = CultureInfo.CurrentCulture.Calendar;

                    model.DateOfBirthDay = currentCalendar.GetDayOfMonth(dateOfBirth.Value);
                    model.DateOfBirthMonth = currentCalendar.GetMonth(dateOfBirth.Value);
                    model.DateOfBirthYear = currentCalendar.GetYear(dateOfBirth.Value);
                }
                model.Company = customer.Company;
                model.StreetAddress = customer.StreetAddress;
                model.StreetAddress2 = customer.StreetAddress2;
                model.ZipPostalCode = customer.ZipPostalCode;
                model.City = customer.City;
                model.County = customer.County;
                model.CountryId = customer.CountryId;
                model.StateProvinceId = customer.StateProvinceId;
                model.Phone = customer.Phone;
                model.Fax = customer.Fax;

                //newsletter
                var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                model.Newsletter = newsletter != null && newsletter.Active;

                model.Signature = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SignatureAttribute);

                model.Email = customer.Email;
                model.Username = customer.Username;
            }
            else
            {
                if (_customerSettings.UsernamesEnabled && !_customerSettings.AllowUsersToChangeUsernames)
                    model.Username = customer.Username;
            }

            if (_customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation)
                model.EmailToRevalidate = customer.EmailToRevalidate;

            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
                foreach (var c in await _countryService.GetAllCountriesAsync(currentLanguage.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = (await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.CountryId, currentLanguage.Id)).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetLocalizedAsync(s, x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                            Value = "0"
                        });
                    }

                }
            }

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = await _localizationService.GetLocalizedEnumAsync(customer.VatNumberStatus);
            model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            model.LastNameEnabled = _customerSettings.LastNameEnabled;
            model.FirstNameRequired = _customerSettings.FirstNameRequired;
            model.LastNameRequired = _customerSettings.LastNameRequired;
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountyEnabled = _customerSettings.CountyEnabled;
            model.CountyRequired = _customerSettings.CountyRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.CountryRequired = _customerSettings.CountryRequired;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.SignatureEnabled = _forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled;

            //external authentication
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            model.AllowCustomersToRemoveAssociations = _externalAuthenticationSettings.AllowCustomersToRemoveAssociations;
            model.NumberOfExternalAuthenticationProviders = (await _authenticationPluginManager
                .LoadActivePluginsAsync(currentCustomer, store.Id))
                .Count;
            foreach (var record in await _externalAuthenticationService.GetCustomerExternalAuthenticationRecordsAsync(customer))
            {
                var authMethod = await _authenticationPluginManager
                    .LoadPluginBySystemNameAsync(record.ProviderSystemName, currentCustomer, store.Id);
                if (!_authenticationPluginManager.IsPluginActive(authMethod))
                    continue;

                model.AssociatedExternalAuthRecords.Add(new CustomerInfoModel.AssociatedExternalAuthModel
                {
                    Id = record.Id,
                    Email = record.Email,
                    ExternalIdentifier = !string.IsNullOrEmpty(record.ExternalDisplayIdentifier)
                        ? record.ExternalDisplayIdentifier : record.ExternalIdentifier,
                    AuthMethodName = await _localizationService.GetLocalizedFriendlyNameAsync(authMethod, currentLanguage.Id)
                });
            }

            //custom customer attributes
            //var customAttributes = await PrepareCustomCustomerAttributesAsync(customer, overrideCustomCustomerAttributesXml);
            //foreach (var attribute in customAttributes)
            //    model.CustomerAttributes.Add(attribute);

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var accepted = await _gdprService.IsConsentAcceptedAsync(consent.Id, currentCustomer.Id);
                    //model.GdprConsents.Add(await PrepareGdprConsentModelAsync(consent, accepted.HasValue && accepted.Value));
                }
            }

            return model;
        }

        #endregion
    }
}
