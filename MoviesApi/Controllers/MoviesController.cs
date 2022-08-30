using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private new List<string> _allowedExtenstions = new List<string> {".jpg",".png"};
        private long _maxAllowedPosterSize =1048576 ;
        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm]MovieDto dto)
        {
            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower())) 
            return BadRequest("only .png and .jpg images are allowed!");    

            if(dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster 1MB !");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);

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
                Year=dto.Year
            };
            await _context.AddAsync(movie);
            _context.SaveChanges();
            return Ok(movie); 
        }

    }
}
