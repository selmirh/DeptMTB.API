using DeptMTB.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeptMTB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchMovieDataController : ControllerBase
    {
        private readonly ILogger<SearchMovieDataController> _logger;
        private readonly IConfiguration Configuration;
        public SearchMovieDataController(ILogger<SearchMovieDataController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        [HttpGet]
        [Route("~/api/getMovieDetails")]
        public AggregatedMovieData GetMovieDetails (string searchTerm)
        {
            AggregatedMovieData movieData = new AggregatedMovieData();
            SearchYouTubeController searchYouTube = new SearchYouTubeController(_logger, Configuration);
            SearchIMDbController searchIMDb = new SearchIMDbController(_logger, Configuration);

            List<string> trailers = searchYouTube.GetMovieTrailers(searchTerm);
            IMDbMovieData movieDetails = searchIMDb.GetMovieDetails(searchTerm);
            movieData.Trailers = trailers;
            movieData.IMDbData = movieDetails;

            return movieData;
        }

    }
}
