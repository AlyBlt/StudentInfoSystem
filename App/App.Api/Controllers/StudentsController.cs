using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: Generate unique student number if duplicate
        private async Task<string> GenerateUniqueStudentNumber(string baseNumber, int? excludeId = null)
        {
            string newNumber = baseNumber;
            int counter = 1;

            while (await _context.Students.AnyAsync(s =>
                       s.StudentNumber == newNumber && (!excludeId.HasValue || s.Id != excludeId.Value)))
            {
                newNumber = $"{baseNumber}-{counter}";
                counter++;
            }

            return newNumber;
        }

        // POST--Generate unique student number without id needed
        private async Task<string> GenerateUniqueStudentNumberForPost(string baseNumber)
        {
            string newNumber = baseNumber;
            int counter = 1;

            while (await _context.Students.AnyAsync(s => s.StudentNumber == newNumber))
            {
                newNumber = $"{baseNumber}-{counter}";
                counter++;
            }

            return newNumber;
        }

        private string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Tümünü küçük yap
            input = input.ToLowerInvariant();

            // İlk harfi büyük yap
            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }

        // GET api/students
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Students.ToListAsync());
        }

        // GET api/students/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound(new { message = $"Student with the given Id: {id} was not found." });

            return Ok(student);
        }

        // POST api/students
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] StudentEntity student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ID veritabanı tarafından atanacak, kullanıcı ne girerse girsin sıfırla
            student.Id = 0;

            // Generate unique StudentNumber for Post (no ID needed)
            student.StudentNumber = await GenerateUniqueStudentNumberForPost(student.StudentNumber);

            student.FirstName = ToTitleCase(student.FirstName);
            student.LastName = ToTitleCase(student.LastName);

            _context.Students.Add(student);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = $"An error occurred while saving the student: {ex.InnerException?.Message ?? ex.Message}" });
            }

            return CreatedAtAction(nameof(Get), new { id = student.Id }, student);

        }

        // PUT api/students/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] StudentEntity student)
        {
            var existingStudent = await _context.Students.FindAsync(id);
            if (existingStudent == null)
                return NotFound(new { message = $"Student with the given Id: {id} was not found." });

            // Generate unique StudentNumber if duplicate, excluding current student
            student.StudentNumber = await GenerateUniqueStudentNumber(student.StudentNumber, id);

            // Update fields
            existingStudent.FirstName = ToTitleCase(student.FirstName);
            existingStudent.LastName = ToTitleCase(student.LastName);
            existingStudent.StudentNumber = student.StudentNumber;
            existingStudent.Grade = student.Grade;
            existingStudent.Email = student.Email;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/students/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound(new { message = $"Student with the given Id: {id} was not found." });

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}