using System.Collections.Generic;
using BookApiProject.Contracts.V1;
using BookApiProject.DTOs;
using BookApiProject.Models;
using BookApiProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookApiProject.Controllers.V1
{
    [Route(ApiRoutes.ReviewsRoot)]
    [ApiController]
    public class ReviewsController : Controller
    {
        private readonly IReviewRepository _iReviewRepository;
        private readonly IBookRepository _iBookRepository;
        private readonly IReviewerRepository _iReviewerRepository;

        public ReviewsController(IReviewRepository iReviewRepository, IBookRepository iBookRepository,IReviewerRepository iReviewerRepository)
        {
            _iReviewRepository = iReviewRepository;
            _iBookRepository = iBookRepository;
            _iReviewerRepository = iReviewerRepository;
        }

        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDTO>))]
        public IActionResult GetReviews()
        {
            var reviews = _iReviewRepository.GetReviews();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewsDto = new List<ReviewDTO>();
            foreach (var review in reviews)
            {
                reviewsDto.Add(new ReviewDTO()
                {
                    Id = review.Id,
                    Headline = review.Headline,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText
                });
            }

            return Ok(reviewsDto);
        }

        //api/reviews/{reviewId}
        [HttpGet("{reviewId}",Name = "GetReview")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(ReviewDTO))]
        public IActionResult GetReview(int reviewId)
        {
            if (!_iReviewRepository.ReviewExists(reviewId))
                return NotFound();

            var review = _iReviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest();

            var reviewDto = new ReviewDTO()
            {
                Id = review.Id,
                Headline = review.Headline,
                Rating = review.Rating,
                ReviewText = review.ReviewText
            };
            return Ok(reviewDto);
        }

        //api/reviews/{reviewId}/book
        [HttpGet("{reviewId}/book")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(BookDTO))]
        public IActionResult GetBookOfAReview(int reviewId)
        {
            if (!_iReviewRepository.ReviewExists(reviewId))
                return NotFound();

            var book = _iReviewRepository.GetBookOfAReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest();

            var bookDto = new BookDTO()
            {
                Id = book.Id,
                Isbn = book.Isbn,
                Title = book.Title,
                DatePublished = book.DatePublished
            };
            return Ok(bookDto);
        }

        //api/reviews/book/{bookId}
        [HttpGet("book/{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDTO>))]
        public IActionResult GetReviewsForABook(int bookId)
        {
            if (!_iBookRepository.BookExists(bookId))
                return NotFound();

            var reviews = _iReviewRepository.GetReviewsOfABook(bookId);

            if (!ModelState.IsValid)
                return BadRequest();

            var reviewsDto = new List<ReviewDTO>();
            foreach (var review in reviews)
            {
                reviewsDto.Add(new ReviewDTO()
                {
                    Id = review.Id,
                    Headline = review.Headline,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText
                });
            }

            return Ok(reviewsDto);
        }



        //api/reviews
        [HttpPost]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201, Type = typeof(Review))]
        public IActionResult CreateReview([FromBody]Review reviewToCreate)
        {
            if (reviewToCreate == null)
                return BadRequest(ModelState);

            //The review has to specify a book and a reviewer. We cannot have a review without book and reviewer

            //first check the Ids of book and reviewer which will come fromBody
            if (!_iBookRepository.BookExists(reviewToCreate.Book.Id))
            {
                ModelState.AddModelError("",$"Book with id: {reviewToCreate.Book.Id} does not exists");
            }

            if (!_iReviewerRepository.ReviewerExists(reviewToCreate.Reviewer.Id))
            {
                ModelState.AddModelError("", $"Reviewer with id: {reviewToCreate.Reviewer.Id} does not exists");
            }

            if (!ModelState.IsValid)
            {
                return StatusCode(404, ModelState);
            }
                

            //From body will only have the bookId and reviewerId. We need to pass the objects too.
            reviewToCreate.Book = _iBookRepository.GetBook(reviewToCreate.Book.Id);
            reviewToCreate.Reviewer = _iReviewerRepository.GetReviewer(reviewToCreate.Reviewer.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_iReviewRepository.CreateReview(reviewToCreate))
            {
                ModelState.AddModelError("", $"Something went wrong with saving {reviewToCreate.Headline} review.");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetReview", new {reviewId = reviewToCreate.Id}, reviewToCreate);
        }

        //api/reviews/{reviewId}
        [HttpPut("{reviewId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult UpdateReview(int reviewId, [FromBody]Review reviewToUpdate)
        {
            if (reviewToUpdate == null)
                return BadRequest(ModelState);

            if (reviewId != reviewToUpdate.Id)
                return BadRequest(ModelState);

            if (!_iReviewRepository.ReviewExists(reviewId))
                ModelState.AddModelError("","Review doesn't exists");

            //The review has to specify a book and a reviewer. We cannot have a review without book and reviewer

            //first check the Ids of book and reviewer which will come fromBody
            if (!_iBookRepository.BookExists(reviewToUpdate.Book.Id))
            {
                ModelState.AddModelError("", $"Book with id: {reviewToUpdate.Book.Id} does not exists");
            }

            if (!_iReviewerRepository.ReviewerExists(reviewToUpdate.Reviewer.Id))
            {
                ModelState.AddModelError("", $"Reviewer with id: {reviewToUpdate.Reviewer.Id} does not exists");
            }

            if (!ModelState.IsValid)
            {
                return StatusCode(404, ModelState);
            }


            //From body will only have the bookId and reviewerId. We need to pass the objects too.
            reviewToUpdate.Book = _iBookRepository.GetBook(reviewToUpdate.Book.Id);
            reviewToUpdate.Reviewer = _iReviewerRepository.GetReviewer(reviewToUpdate.Reviewer.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_iReviewRepository.UpdateReview(reviewToUpdate))
            {
                ModelState.AddModelError("", $"Something went wrong with saving {reviewToUpdate.Headline} review.");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //api/reviews/{reviewId}
        [HttpDelete("{reviewId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult DeleteReview(int reviewId)
        {
            if (!_iReviewRepository.ReviewExists(reviewId))
                return NotFound();

            var reviewToDelete = _iReviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_iReviewRepository.DeleteReview(reviewToDelete))
            {
                ModelState.AddModelError("","Something went wrong deleting the review");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}