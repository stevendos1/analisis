// Spendnt.API/Controllers/RecordatoriosGastoController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spendnt.API.Data;
using Spendnt.Shared.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spendnt.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RecordatoriosGastoController : ControllerBase
    {
        private readonly DataContext _context;

        public RecordatoriosGastoController(DataContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecordatorioGasto>>> Get()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return Ok(await _context.RecordatoriosGasto
                                 .Where(r => r.UserId == userId)
                                 .OrderBy(r => r.FechaProgramada)
                                 .ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RecordatorioGasto>> Get(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var recordatorio = await _context.RecordatoriosGasto
                                           .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recordatorio == null) return NotFound();
            return Ok(recordatorio);
        }

        [HttpPost]
        public async Task<ActionResult<RecordatorioGasto>> Post(RecordatorioGasto recordatorio)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            recordatorio.UserId = userId;

            _context.Add(recordatorio);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = recordatorio.Id }, recordatorio);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, RecordatorioGasto recordatorio)
        {
            if (id != recordatorio.Id)
            {
                return BadRequest("El ID del recordatorio en la ruta no coincide con el del cuerpo.");
            }
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existingRecordatorio = await _context.RecordatoriosGasto
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (existingRecordatorio == null)
            {
                return NotFound("Recordatorio no encontrado o no pertenece al usuario.");
            }
            recordatorio.UserId = userId;
            _context.Entry(recordatorio).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.RecordatoriosGasto.AnyAsync(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var recordatorio = await _context.RecordatoriosGasto
                                           .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (recordatorio == null) return NotFound();

            _context.Remove(recordatorio);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}