using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMoviesService _moviesService;
        private readonly IGenresService _genresService;

        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;
        public MoviesController(IMoviesService moviesService, IGenresService genresService)
        {

            _moviesService = moviesService;
            _genresService = genresService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var Movies = await _moviesService.GetAll();
                
            return Ok(Movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _moviesService.GetById(id);
            if (movie == null)
                return NotFound();


            var dto = new MovieDetailsDto
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                GenreName = movie.Genre.Name,
                Poster = movie.Poster,
                Rate = movie.Rate,
                StoryLine = movie.StoryLine,
                Title = movie.Title,
                Year = movie.Year
            };

            return Ok(dto);
        }

        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreid)
        {
            var Movies = await _moviesService.GetAll();
            return Ok(Movies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
            if (dto.Poster == null)
                return BadRequest("poster is required!");

            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("only .png and .jpg images are allowed!");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster 1MB !");

            var isValidGenre = await _genresService.IsVaildGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("Invalid Genre Id!");



            using var datastream = new MemoryStream();

            await dto.Poster.CopyToAsync(datastream);

            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Title = dto.Title,
                Poster = datastream.ToArray(),
                Rate = dto.Rate,
                StoryLine = dto.StoryLine,
                Year = dto.Year
            };
          _moviesService.Add(movie);
            return Ok(movie);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateAsync(int id , [FromForm] MovieDto dto)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return NotFound($"No Movie was found with ID {id}");

            var isValidGenre = await _genresService.IsVaildGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("Invalid Genre Id!");

            if(dto.Poster != null)
            {
                if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("only .png and .jpg images are allowed!");

                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed size for poster 1MB !");

                using var datastream = new MemoryStream();

                await dto.Poster.CopyToAsync(datastream);

                movie.Poster=datastream.ToArray();
            }

            movie.Title=dto.Title;
            movie.GenreId=dto.GenreId;
            movie.Rate=dto.Rate;
            movie.StoryLine=dto.StoryLine;
            movie.Year=dto.Year;

            _moviesService.Update(movie);
            return Ok(movie);
        }

        [HttpDelete ("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return NotFound($"No Movie was found with ID {id}");

            _moviesService.Delete(movie);
            return Ok(movie);
        }
    }
}
