namespace BookApiProject.Contracts.V1
{
    public static class ApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" +Version;
        
        public const string CountriesRoot = Base + "/countries";
        public const string AuthorsRoot = Base + "/authors";
        public const string BooksRoot = Base + "/books";
        public const string CategoriesRoot = Base + "/categories";
        public const string ReviewsRoot = Base + "/reviews";
        public const string ReviewersRoot = Base + "/reviewers";

    }
}
