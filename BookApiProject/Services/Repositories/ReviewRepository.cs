using System;
using System.Collections.Generic;
using System.Linq;
using BookApiProject.Models;
using BookApiProject.Services.Interfaces;

namespace BookApiProject.Services.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly BookDbContext _bookDbContext;

        public ReviewRepository(BookDbContext bookDbContext)
        {
            _bookDbContext = bookDbContext;
        }
        public ICollection<Review> GetReviews()
        {
            return _bookDbContext.Reviews.OrderBy(r=>r.Rating).ToList();
        }

        public Review GetReview(int reviewId)
        {
            return _bookDbContext.Reviews.FirstOrDefault(r => r.Id == reviewId);
        }

        public ICollection<Review> GetReviewsOfABook(int bookId)
        {
            return _bookDbContext.Reviews.Where(r => r.Book.Id == bookId).ToList();
        }

        public Book GetBookOfAReview(int reviewId)
        {
            return _bookDbContext.Reviews.Where(r => r.Id == reviewId).Select(r => r.Book).FirstOrDefault();
        }

        public bool ReviewExists(int reviewId)
        {
            return _bookDbContext.Reviews.Any(r => r.Id == reviewId);
        }

        public bool CreateReview(Review review)
        {
            _bookDbContext.Add(review);
            return Save();
        }

        public bool UpdateReview(Review review)
        {
            _bookDbContext.Update(review);
            return Save();
        }

        public bool DeleteReview(Review review)
        {
            _bookDbContext.Remove(review);
            return Save();
        }

        public bool DeleteReviews(List<Review> reviews)
        {
            _bookDbContext.RemoveRange(reviews);
            return Save();
        }

        public bool Save()
        {
            var savedChanges = _bookDbContext.SaveChanges();
            return savedChanges >= 0;
        }
    }
}
