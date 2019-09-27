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
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewerController : Controller
    {
        private readonly IReviewerRepository _iReviewerRepository;
        private readonly IReviewRepository _iReviewRepository;

        public ReviewerController(IReviewerRepository iReviewerRepository, IReviewRepository iReviewRepository)
        {
            _iReviewerRepository = iReviewerRepository;
            _iReviewRepository = iReviewRepository;
        }

        //api/reviewer
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type = typeof(IEnumerable<ReviewerDTO>))]
        public IActionResult GetReviewers()
        {
            var reviewers = _iReviewerRepository.GetReviewers();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewersDto = new List<ReviewerDTO>();

            foreach (var reviewer in reviewers)
            {
                reviewersDto.Add(new ReviewerDTO
                {
                    Id = reviewer.Id,
                    FirstName = reviewer.FirstName,
                    LastName = reviewer.LastName

                });
            }

            return Ok(reviewersDto);
        }

        //api/reviewer/{reviewerId}
        [HttpGet("{reviewerId}",Name = "GetReviewer")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(ReviewerDTO))]
        public IActionResult GetReviewer(int reviewerId)
        {
            if (!_iReviewerRepository.ReviewerExists(reviewerId))
                return NotFound();

            var reviewer = _iReviewerRepository.GetReviewer(reviewerId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewerDto = new ReviewerDTO()
            {
                Id = reviewer.Id,
                FirstName = reviewer.FirstName,
                LastName = reviewer.LastName
            };

            return Ok(reviewerDto);
        }

        //api/reviewer/{reviewerId}/reviews
        [HttpGet("{reviewerId}/reviews")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDTO>))]
        public IActionResult GetReviewsByReviewer(int reviewerId)
        {
            if (!_iReviewerRepository.ReviewerExists(reviewerId))
                return NotFound();

            var reviews = _iReviewerRepository.GetReviewsByReviewer(reviewerId);

            if (!ModelState.IsValid)
                return BadRequest();

            var reviewsDto = new List<ReviewDTO>();

            foreach (var review in reviews)
            {
                reviewsDto.Add(new ReviewDTO
                {
                    Id = review.Id,
                    Headline = review.Headline,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText
                });
            }

            return Ok(reviewsDto);
        }

        //api/reviewer/review/{reviewId}
        [HttpGet("review/{reviewId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewerDTO>))]
        public IActionResult GetReviewerOfAReview(int reviewId)
        {
            if (!_iReviewRepository.ReviewExists(reviewId))
                return NotFound();

            var reviewer = _iReviewerRepository.GetReviewerOfAReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewerDto = new ReviewerDTO()
            {
                Id = reviewer.Id,
                FirstName = reviewer.FirstName,
                LastName = reviewer.LastName
            };

            return Ok(reviewerDto);
        }

        //api/reviewer
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201, Type = typeof(Reviewer))]
        public IActionResult CreateReviewer([FromBody]Reviewer reviewer)
        {
            if (reviewer == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (!_iReviewerRepository.CreateReviewer(reviewer))
            {
                ModelState.AddModelError("","Something went wrong saving reviewer,");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetReviewer", new {reviewerId = reviewer.Id}, reviewer);
        }

        //api/reviewer/{reviewerId}
        [HttpPut("{reviewerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult UpdateReviewer(int reviewerId, [FromBody] Reviewer reviewer)
        {
            if (reviewer == null || reviewer.Id!=reviewerId)
                return BadRequest(ModelState);

            if (!_iReviewerRepository.ReviewerExists(reviewerId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_iReviewerRepository.UpdateReviewer(reviewer))
            {
                ModelState.AddModelError("","Something went wrong updating the reviewer");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //api/reviewer/{reviewerId}
        [HttpDelete("{reviewerId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult DeleteReviewer(int reviewerId)
        {
            if (!_iReviewerRepository.ReviewerExists(reviewerId))
                return NotFound();

            var reviewerToDelete = _iReviewerRepository.GetReviewer(reviewerId);
            var reviewsToDelete = _iReviewerRepository.GetReviewsByReviewer(reviewerId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            
            if (!_iReviewerRepository.DeleteReviewer(reviewerToDelete))
            {
                ModelState.AddModelError("","Something went wrong deleting the reviewer");
                return StatusCode(500, ModelState);
            }

            if (!_iReviewRepository.DeleteReviews(reviewsToDelete.ToList()))
            {
                ModelState.AddModelError("", "Something went wrong deleting the reviews by this reviewer");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}