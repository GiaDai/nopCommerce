using Nop.Api.Models.Directory;
using Nop.Core;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Api.Factories
{
    public class CountryModelFactory : ICountryModelFactory
    {
        #region Fields

        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CountryModelFactory(
            ICountryService countryService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IWorkContext workContext
        )
        {
            _countryService = countryService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task<IList<StateProvinceModel>> GetStatesByCountryIdAsync(string countryId, bool addSelectStateItem)
        {
            if (string.IsNullOrEmpty(countryId))
                throw new ArgumentNullException(nameof(countryId));

            var country = await _countryService.GetCountryByIdAsync(Convert.ToInt32(countryId));
            var states = (await _stateProvinceService
                .GetStateProvincesByCountryIdAsync(country?.Id ?? 0, (await _workContext.GetWorkingLanguageAsync()).Id))
                .ToList();
            var result = new List<StateProvinceModel>();
            foreach (var state in states)
                result.Add(new StateProvinceModel
                {
                    id = state.Id,
                    name = await _localizationService.GetLocalizedAsync(state, x => x.Name)
                });

            if (country == null)
            {
                //country is not selected ("choose country" item)
                if (addSelectStateItem)
                {
                    result.Insert(0, new StateProvinceModel
                    {
                        id = 0,
                        name = await _localizationService.GetResourceAsync("Address.SelectState")
                    });
                }
                else
                {
                    result.Insert(0, new StateProvinceModel
                    {
                        id = 0,
                        name = await _localizationService.GetResourceAsync("Address.Other")
                    });
                }
            }
            else
            {
                //some country is selected
                if (!result.Any())
                {
                    //country does not have states
                    result.Insert(0, new StateProvinceModel
                    {
                        id = 0,
                        name = await _localizationService.GetResourceAsync("Address.Other")
                    });
                }
                else
                {
                    //country has some states
                    if (addSelectStateItem)
                    {
                        result.Insert(0, new StateProvinceModel
                        {
                            id = 0,
                            name = await _localizationService.GetResourceAsync("Address.SelectState")
                        });
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
