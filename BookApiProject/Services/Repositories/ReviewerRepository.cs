using System.Collections.Generic;
using System.Linq;
using BookApiProject.Models;
using BookApiProject.Services.Interfaces;

namespace BookApiProject.Services.Repositories
{
    public class ReviewerRepository : IReviewerRepository
    {
        private readonly BookDbContext _bookDbContext;

        public ReviewerRepository(BookDbContext bookDbContext)
        {
            _bookDbContext = bookDbContext;
        }

        public ICollection<Reviewer> GetReviewers()
        {
            return _bookDbContext.Reviewers.OrderBy(r=>r.LastName).ToList();
        }

        public Reviewer GetReviewer(int reviewerId)
        {
            return _bookDbContext.Reviewers.FirstOrDefault(r => r.Id == reviewerId);
        }

        public ICollection<Review> GetReviewsByReviewer(int reviewerId)
        {
            return _bookDbContext.Reviews.Where(r => r.Reviewer.Id == reviewerId).ToList();
        }

        public Reviewer GetReviewerOfAReview(int reviewId)
        {
            var reviewerId = _bookDbContext.Reviews.Where(r => r.Id == reviewId).Select(r => r.Reviewer.Id).FirstOrDefault();
            return _bookDbContext.Reviewers.FirstOrDefault(r => r.Id == reviewerId);
        }

        public bool ReviewerExists(int reviewerId)
        {
            return _bookDbContext.Reviewers.Any(r => r.Id == reviewerId);
        }

        public bool CreateReviewer(Reviewer reviewer)
        {
            _bookDbContext.Add(reviewer);
            return Save();
        }

        public bool UpdateReviewer(Reviewer reviewer)
        {
            _bookDbContext.Update(reviewer);
            return Save();
        }

        public bool Save()
        {
            var savedChanges = _bookDbContext.SaveChanges();
            return savedChanges >= 0;
        }

        public bool DeleteReviewer(Reviewer reviewer)
        {
            _bookDbContext.Remove(reviewer);
            return Save();
        }

        
    }
}
