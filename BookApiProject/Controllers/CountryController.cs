using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.DTOs;
using BookApiProject.Models;
using BookApiProject.Services;
using BookApiProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookApiProject.Controllers
{
    //specify route
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : Controller
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IAuthorRepository _iAuthorRepository;

        public CountryController(ICountryRepository countryRepository,IAuthorRepository iAuthorRepository)
        {
            _countryRepository = countryRepository;
            _iAuthorRepository = iAuthorRepository;
        }

        //api/countries
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CountryDTO>))]
        public IActionResult GetCountries()
        {
            var countries = _countryRepository.GetCountries().ToList();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countriesDTO= new List<CountryDTO>();

            foreach (var country in countries)
            {
                countriesDTO.Add(new CountryDTO
                {
                    Id = country.Id,
                    Name = country.Name
                });
            }

            return Ok(countriesDTO);
        }

        //api/countries/countryId
        [HttpGet("{countryId}",Name = "GetCountry")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(CountryDTO))]
        public IActionResult GetCountry(int countryId)
        {
            if (!_countryRepository.CountryExists(countryId))
            {
                return NotFound();
            }

            var country = _countryRepository.GetCountry(countryId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countryDTO = new CountryDTO()
            {
                Id = country.Id,
                Name = country.Name
            };

            return Ok(countryDTO);
        }

        //api/countries/authors/authorId
        [HttpGet("authors/{authorId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(CountryDTO))]
        public IActionResult GetCountryOfAnAuthor(int authorId)
        {
            if (!_iAuthorRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var country = _countryRepository.GetCountryOfAnAuthor(authorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countryDTO = new CountryDTO()
            {
                Id = country.Id,
                Name = country.Name
            };

            return Ok(countryDTO);
        }

        //api/country/{countryId}/authors
        [HttpGet("{countryId}/authors")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDTO>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetAuthorsFromACountry(int countryId)
        {
            if (!_countryRepository.CountryExists(countryId))
                return NotFound();

            var authors = _countryRepository.GetAuthorsFromACountry(countryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorsDto = new List<AuthorDTO>();

            foreach (var author in authors)
            {
                authorsDto.Add(new AuthorDTO
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });
            }

            return Ok(authorsDto);
        }

        //api/country
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(422)]
        [ProducesResponseType(201,Type = typeof(Country))]
        public IActionResult CreateCountry([FromBody]Country countryToCreate)
        {
            if (countryToCreate == null)
                return BadRequest(ModelState);

            //check if country with this name already exists
            var countryExists = _countryRepository.GetCountries().Any(c =>
                c.Name.Trim().ToUpper().Equals(countryToCreate.Name.Trim().ToUpper()));

            if (countryExists)
            {
                ModelState.AddModelError("", $"Country {countryToCreate.Name} already Exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_countryRepository.CreateCountry(countryToCreate))
            {
                ModelState.AddModelError("", $"Something went wrong saving {countryToCreate.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetCountry", new {countryId = countryToCreate.Id}, countryToCreate);
        }

        //api/country/{countryId}
        [HttpPut("{countryId}")]
        [ProducesResponseType(204)]//no content
        [ProducesResponseType(400)]//Bad request
        [ProducesResponseType(404)]//Not Found
        [ProducesResponseType(422)]//UnProcessable Entity
        [ProducesResponseType(500)]//Server error
        public IActionResult UpdateCountry(int countryId, [FromBody]Country updatedCountryInfo)
        {
            if (updatedCountryInfo == null)
                return BadRequest(ModelState);

            if (updatedCountryInfo.Id != countryId)
                return BadRequest(ModelState);

            if (!_countryRepository.CountryExists(countryId))
                return NotFound();

            if (_countryRepository.IsDuplicateCountryName(countryId, updatedCountryInfo.Name))
            {
                ModelState.AddModelError("", $"Country {updatedCountryInfo.Name} already Exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_countryRepository.UpdateCountry(updatedCountryInfo))
            {
                ModelState.AddModelError("", $"Something went wrong updating {updatedCountryInfo.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //api/country/{countryId}
        [HttpDelete("{countryId}")]
        [ProducesResponseType(204)] //no content
        [ProducesResponseType(400)] //Bad request
        [ProducesResponseType(404)] //Not Found
        [ProducesResponseType(500)] //Server error
        [ProducesResponseType(409)] //Conflict
        public IActionResult DeleteCountry(int countryId)
        {
            if (!_countryRepository.CountryExists(countryId))
                return NotFound();

            var countryToDelete = _countryRepository.GetCountry(countryId);

            //check if there are any authors from this country. If there is one then we cannot delete the country
            if (_countryRepository.GetAuthorsFromACountry(countryId).Count > 0)
            {
                ModelState.AddModelError("",
                    $"Country {countryToDelete.Name} cannot be deleted because it is used by at least one author");
                return StatusCode(409, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_countryRepository.DeleteCountry(countryToDelete))
            {
                ModelState.AddModelError("",
                    $"Something went wrong deleting {countryToDelete.Name} ");
                return StatusCode(500, ModelState);
            }

            return NoContent();

        }
    }
}
