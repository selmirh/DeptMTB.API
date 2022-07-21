using DeptMTB.API.Models;
using IMDbApiLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeptMTB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchIMDbController : ControllerBase
    {
        private readonly ILogger<SearchMovieDataController> _logger;
        private readonly IConfiguration Configuration;

        public SearchIMDbController(ILogger<SearchMovieDataController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        private static IMDbMovieData movieData = new IMDbMovieData();

        [HttpGet]
        [Route("~/api/getIMDbData")]
        public IMDbMovieData GetMovieDetails (string searchTerm)
        {
            Search(searchTerm);

            return movieData;
        }

        private void Search(string searchTerm)
        {
            try
            {
                new SearchIMDbController(_logger, Configuration).Run(searchTerm).Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    _logger.LogInformation("Error: " + e.Message);
                }
            }
        }

        private async Task Run(string searchTerm)
        {
            string apiKey = Configuration["IMDbApiKey"];
            // IMDb API code snippet: https://imdb-api.com/api#bodyCSLib.
            var apiLib = new ApiLib(apiKey);
            var data = await apiLib.SearchMovieAsync(searchTerm);
            
            // Here I assume that IMDb search will return wanted movie as a first element of the list.
            // It always does that (as far as I can tell), and I don't see the point to collect all results
            // and show them to the user.
            // I decided to show movie details and ratings to the user, that seems like an enough information.
            if(data.ErrorMessage == "")
            {
                var movie = data.Results.FirstOrDefault();
                movieData.Id = movie.Id;
                movieData.ResultType = movie.ResultType;
                movieData.Image = movie.Image;
                movieData.Title = movie.Title;
                movieData.Description = movie.Description;
            }

            var ratings = await apiLib.RatingsAsync(movieData.Id);
            if(ratings.ErrorMessage == "")
            {
                movieData.Year = ratings.Year;
                movieData.MetaCriticRating = ratings.Metacritic;
                movieData.IMDbRating = ratings.IMDb;
                movieData.TheMovieDbRating = ratings.TheMovieDb;
                movieData.RottenTomatoesRating = ratings.RottenTomatoes;
                movieData.FilmAffinityRating = ratings.FilmAffinity;
            }
        }
    }
}
