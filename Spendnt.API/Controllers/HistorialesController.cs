// Spendnt.API/Controllers/HistorialesController.cs
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
    public class HistorialesController : ControllerBase
    {
        private readonly DataContext _context;

        public HistorialesController(DataContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Historial>>> GetHistoriales()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldoUsuarioId = await _context.Saldo
                                        .Where(s => s.UserId == userId)
                                        .Select(s => s.Id)
                                        .FirstOrDefaultAsync();
            if (saldoUsuarioId == 0)
            {
                return Ok(new List<Historial>());
            }

            return await _context.Historiales
                                 .Where(h => h.SaldoId == saldoUsuarioId)
                                 .Include(h => h.Categoria)
                                 .OrderByDescending(h => h.Fecha)
                                 .ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Historial>> GetHistorial(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var historial = await _context.Historiales
                                        .Include(h => h.Categoria)
                                        .FirstOrDefaultAsync(h => h.Id == id && h.Saldo.UserId == userId);

            if (historial == null)
            {
                return NotFound();
            }
            return historial;
        }

        [HttpPost]
        public async Task<ActionResult<Historial>> PostHistorial(Historial historial)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldoUsuario = await _context.Saldo.FirstOrDefaultAsync(s => s.UserId == userId);
            if (saldoUsuario == null)
            {
                return BadRequest("No se encontró un saldo principal para el usuario.");
            }
            historial.SaldoId = saldoUsuario.Id;

            _context.Historiales.Add(historial);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHistorial), new { id = historial.Id }, historial);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutHistorial(int id, Historial historial)
        {
            if (id != historial.Id)
            {
                return BadRequest();
            }
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existingHistorial = await _context.Historiales
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(h => h.Id == id && h.Saldo.UserId == userId);
            if (existingHistorial == null)
            {
                return NotFound("Historial no encontrado o no pertenece al usuario.");
            }
            historial.SaldoId = existingHistorial.SaldoId;
            _context.Entry(historial).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Historiales.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHistorial(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var historial = await _context.Historiales
                                        .FirstOrDefaultAsync(h => h.Id == id && h.Saldo.UserId == userId);
            if (historial == null)
            {
                return NotFound();
            }
            _context.Historiales.Remove(historial);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}