using System.Collections.Generic;
using System.Linq;
using BookApiProject.Models;
using BookApiProject.Services.Interfaces;

namespace BookApiProject.Services.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly BookDbContext _countryContext;

        public CountryRepository(BookDbContext countryContext)
        {
            _countryContext = countryContext;
        }
        public ICollection<Country> GetCountries()
        {
            return _countryContext.Countries.OrderBy(c=>c.Name).ToList();
        }

        public Country GetCountry(int countryId)
        {
            return _countryContext.Countries.FirstOrDefault(c => c.Id == countryId);
        }

        public Country GetCountryOfAnAuthor(int authorId)
        {
            return _countryContext.Authors.Where(a => a.Id == authorId).Select(c=>c.Country).FirstOrDefault();
        }

        public ICollection<Author> GetAuthorsFromACountry(int countryId)
        {
            return _countryContext.Authors.Where(c=>c.Id == countryId).ToList();
        }

        public bool CountryExists(int countryId)
        {
            return _countryContext.Countries.Any(c=>c.Id==countryId);
        }

        public bool IsDuplicateCountryName(int countryId, string countryName)
        {
            var country = _countryContext.Countries.FirstOrDefault(c =>
                c.Name.Trim().ToUpper().Equals(countryName.Trim().ToUpper()) && c.Id != countryId);

            return country != null;
        }

        public bool CreateCountry(Country country)
        {
            _countryContext.AddAsync(country);
            return Save();
        }

        public bool UpdateCountry(Country country)
        {
            _countryContext.Update(country);
            return Save();
        }

        public bool DeleteCountry(Country country)
        {
            _countryContext.Remove(country);
            return Save();
        }

        public bool Save()
        {
            //check if there are any changes to save
            var savedChanges = _countryContext.SaveChanges();
            return savedChanges >= 0 ? true : false;
        }

    }
}
