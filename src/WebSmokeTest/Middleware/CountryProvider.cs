using System;
using System.Collections.Generic;

using Middleware;

namespace SmokeTest.Middleware
{
    public class CountryProvider : ICountryProvider
    {
        public Country CountryCreate(in string name, in string code, in bool visible)
        {
            throw new NotImplementedException();
        }

        public bool CountryDelete(in Country country)
        {
            throw new NotImplementedException();
        }

        public bool CountryUpdate(in Country country)
        {
            throw new NotImplementedException();
        }

        public List<Country> GetAllCountries()
        {
            throw new NotImplementedException();
        }

        public List<Country> GetVisibleCountries()
        {
            return new List<Country>()
            {
                new Country("Default", "def", true)
            };
        }
    }
}
