﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Api.Extensions;
using Nop.Api.Factories;
using Nop.Api.Models.Customer;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Tax;

namespace Nop.Api.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : BaseApiController
    {
        #region Fields
        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressService _addressService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICountryService _countryService;
        private readonly IConfiguration _config;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly TaxSettings _taxSettings;

        #endregion

        #region Ctor
        public CustomerController(
            IAddressService addressService,
            AddressSettings addressSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressModelFactory addressModelFactory,
            IAuthenticationService authenticationService,
            ICountryService countryService,
            IConfiguration config,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            TaxSettings taxSettings
        ) : base(customerService)
        {
            _addressService = addressService;
            _addressSettings = addressSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _addressModelFactory = addressModelFactory;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _config = config;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _taxSettings = taxSettings;

        }

        #endregion

        #region Methods

        #region Customer Login

        /// <summary>
        /// Customer Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/customer/login
        ///     {
        ///        "username": "admin",
        ///        "password": "123456"
        ///     }
        ///
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseSuccess), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponseError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(model.Username?.Trim(), model.Password);
            switch (loginResult)
            {
            
                case CustomerLoginResults.Successful:
                    {
                        var customer = await _customerService.GetCustomerByEmailAsync(model.Username?.Trim());
                        await _customerRegistrationService.SignInCustomerAsync(customer, "/", true);
                        var tokenString = GenerateJSONWebToken(customer);
                        return Ok(new LoginResponseSuccess { Token = tokenString });
                    }
                case CustomerLoginResults.CustomerNotExist:
                    {
                        return BadRequest(new LoginResponseError { 
                            Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist") 
                        });
                    }
                case CustomerLoginResults.Deleted:
                    {
                        return BadRequest(
                            new LoginResponseError { 
                                Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted")
                            });
                    }
                case CustomerLoginResults.NotActive:
                    {
                        return BadRequest(
                            new LoginResponseError {
                                Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive") 
                            });
                    }
                case CustomerLoginResults.NotRegistered:
                    {
                        return BadRequest(
                            new LoginResponseError {
                                Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered") 
                            
                            });
                    }
                case CustomerLoginResults.WrongPassword:
                    {
                        return BadRequest(
                            new LoginResponseError
                            {
                                Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.WrongPassword") 
                            });
                    }
                case CustomerLoginResults.LockedOut:
                    return BadRequest(new LoginResponseError { 
                        Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut") 
                    });
                default:
                    {
                        return BadRequest(
                            new LoginResponseError
                            {
                                Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.UnknownError") 
                            });
                    }
            }
            
        }

        private string GenerateJSONWebToken(Customer userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim("uid", userInfo.Id.ToString())
            };
            var token = new JwtSecurityToken(
              _config["JWTSettings:Issuer"],
              _config["JWTSettings:Issuer"],
              claims: claims,
              expires: DateTime.Now.AddMinutes(int.Parse(_config["JWTSettings:DurationInMinutes"])),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion

        #region Customer My Account/Info

        [Authorize]
        [HttpGet("info")]
        [ProducesResponseType(typeof(CustomerInfoModel),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Info()
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();
            var model = new CustomerInfoModel();
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, false);

            return Ok(model);
        }

        [Authorize]
        [HttpPost("info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Info(CustomerInfoModel model)
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var oldCustomerModel = new CustomerInfoModel();

            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

            try
            {
                if (ModelState.IsValid)
                {
                    //username 
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        var userName = model.Username.Trim();
                        if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            await _customerRegistrationService.SetUsernameAsync(customer, userName);

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }
                    //email
                    var email = model.Email.Trim();
                    if (!customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                        await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = customer.VatNumber;
                        customer.VatNumber = model.VatNumber;

                        if (prevVatNumber != model.VatNumber)
                        {
                            var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                            customer.VatNumberStatusId = (int)vatNumberStatus;

                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                    model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
                    if (_customerSettings.FirstNameEnabled)
                        customer.FirstName = model.FirstName;
                    if (_customerSettings.LastNameEnabled)
                        customer.LastName = model.LastName;
                    if (_customerSettings.DateOfBirthEnabled)
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;
                    if (_customerSettings.StreetAddressEnabled)
                        customer.StreetAddress = model.StreetAddress;
                    if (_customerSettings.StreetAddress2Enabled)
                        customer.StreetAddress2 = model.StreetAddress2;
                    if (_customerSettings.ZipPostalCodeEnabled)
                        customer.ZipPostalCode = model.ZipPostalCode;
                    if (_customerSettings.CityEnabled)
                        customer.City = model.City;
                    if (_customerSettings.CountyEnabled)
                        customer.County = model.County;
                    if (_customerSettings.CountryEnabled)
                        customer.CountryId = model.CountryId;
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        customer.StateProvinceId = model.StateProvinceId;
                    if (_customerSettings.PhoneEnabled)
                        customer.Phone = model.Phone;
                    if (_customerSettings.FaxEnabled)
                        customer.Fax = model.Fax;

                    //customer.CustomCustomerAttributesXML = customerAttributesXml;
                    await _customerService.UpdateCustomerAsync(customer);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var store = await _storeContext.GetCurrentStoreAsync();
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                            }
                            else
                            {
                                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = store.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                    //GDPR
                    //if (_gdprSettings.GdprEnabled)
                    //    await LogGdprAsync(customer, oldCustomerModel, model, form);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.CustomerInfo.Updated"));

                    return RedirectToRoute("CustomerInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true, "");

            return Ok(model);
        }
        #endregion

        #region Customer My Account/Address

        [Authorize]
        [HttpGet("addresses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Addresses()
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();

            if (!await _customerService.IsRegisteredAsync(customer))
                return NotFound();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync();

            return Ok(model);
        }

        [Authorize]
        [HttpGet("addressadd")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAddressAdd()
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();

            if (!await _customerService.IsRegisteredAsync(customer))
                return NotFound();
            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
            excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
            return Ok(model);
        }

        [Authorize]
        [HttpPost("addressadd")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAddressAdd([FromBody]CustomerAddressEditModel model)
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();

            if (!await _customerService.IsRegisteredAsync(customer))
                return NotFound();
            //custom address attributes
            var customAttributes = "";

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;


                await _addressService.InsertAddressAsync(address);

                await _customerService.InsertCustomerAddressAsync(customer, address);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.CustomerAddresses.Added"));

                return Ok(200);
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            return BadRequest(model);
        }

        [Authorize]
        [HttpGet("addressedit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAddressEdit(int addressId)
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();

            if (!await _customerService.IsRegisteredAsync(customer))
                return NotFound();

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return NotFound();

            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            return Ok(model);
        }

        [Authorize]
        [HttpPost("addressedit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAddressEdit([FromBody]CustomerAddressEditModel model)
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();

            if (!await _customerService.IsRegisteredAsync(customer))
                return NotFound();

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, model.Address.Id);
            if (address == null)
                //address is not found
                return NotFound();

            //custom address attributes
            var customAttributes = "";

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.UpdateAddressAsync(address);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.CustomerAddresses.Updated"));

                return Ok(200);
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(
                model.Address,
                address: address, 
                excludeProperties: true, 
                addressSettings: _addressSettings, 
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id), 
                overrideAttributesXml: customAttributes);

            return BadRequest(model);
        }

        [Authorize]
        [HttpDelete("addressdelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAddressDelete(int addressId)
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();

            if (!await _customerService.IsRegisteredAsync(customer))
                return NotFound();

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address != null)
            {
                await _customerService.RemoveCustomerAddressAsync(customer, address);
                await _customerService.UpdateCustomerAsync(customer);
                //now delete the address record
                await _addressService.DeleteAddressAsync(address);
                return Ok();
            }

            //redirect to the address list page
            return NotFound();
        }
        #endregion

        #endregion
    }
}
