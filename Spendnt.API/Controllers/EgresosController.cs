// Spendnt.API/Controllers/EgresosController.cs
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
    [Route("/api/Egresos")]
    public class EgresosController : ControllerBase
    {
        private readonly DataContext _context;

        public EgresosController(DataContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Egresos>>> Get()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldoUsuarioId = await _context.Saldo
                                        .Where(s => s.UserId == userId)
                                        .Select(s => s.Id)
                                        .FirstOrDefaultAsync();
            if (saldoUsuarioId == 0)
            {
                return Ok(new List<Egresos>());
            }

            return Ok(await _context.Egresos
                                 .Where(e => e.SaldoId == saldoUsuarioId)
                                 .Include(e => e.Categoria)
                                 .OrderByDescending(e => e.Fecha)
                                 .ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Egresos>> Get(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var egreso = await _context.Egresos
                                        .Include(e => e.Categoria)
                                        .FirstOrDefaultAsync(e => e.Id == id && e.Saldo.UserId == userId);

            if (egreso == null)
            {
                return NotFound();
            }
            return Ok(egreso);
        }

        [HttpPost]
        public async Task<ActionResult<Egresos>> Post(Egresos egreso)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldoPrincipal = await _context.Saldo.FirstOrDefaultAsync(s => s.UserId == userId);
            if (saldoPrincipal == null)
            {
                return BadRequest("Error crítico: No se encontró un saldo principal para el usuario actual para asociar el egreso.");
            }
            egreso.SaldoId = saldoPrincipal.Id;

            _context.Egresos.Add(egreso);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al guardar el egreso.");
            }
            return CreatedAtAction(nameof(Get), new { id = egreso.Id }, egreso);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Egresos egreso)
        {
            if (id != egreso.Id)
            {
                return BadRequest("El ID del egreso en la ruta no coincide con el del cuerpo de la solicitud.");
            }
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existingEgreso = await _context.Egresos
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(e => e.Id == id && e.Saldo.UserId == userId);
            if (existingEgreso == null)
            {
                return NotFound("Egreso no encontrado o no pertenece al usuario.");
            }
            egreso.SaldoId = existingEgreso.SaldoId;
            _context.Entry(egreso).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Egresos.AnyAsync(e => e.Id == id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al actualizar el egreso.");
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var egreso = await _context.Egresos
                                      .FirstOrDefaultAsync(e => e.Id == id && e.Saldo.UserId == userId);
            if (egreso == null)
            {
                return NotFound();
            }
            _context.Egresos.Remove(egreso);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}