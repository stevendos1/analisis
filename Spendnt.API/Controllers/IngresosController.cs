// Spendnt.API/Controllers/IngresosController.cs
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
    [Route("/api/Ingresos")]
    public class IngresosController : ControllerBase
    {
        private readonly DataContext _context;

        public IngresosController(DataContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ingresos>>> Get()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldoUsuarioId = await _context.Saldo
                                        .Where(s => s.UserId == userId)
                                        .Select(s => s.Id)
                                        .FirstOrDefaultAsync();
            if (saldoUsuarioId == 0)
            {
                return Ok(new List<Ingresos>());
            }

            return Ok(await _context.Ingresos
                                 .Where(i => i.SaldoId == saldoUsuarioId)
                                 .Include(i => i.Categoria)
                                 .OrderByDescending(i => i.Fecha)
                                 .ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Ingresos>> Get(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var ingreso = await _context.Ingresos
                                        .Include(i => i.Categoria)
                                        .FirstOrDefaultAsync(i => i.Id == id && i.Saldo.UserId == userId);

            if (ingreso == null)
            {
                return NotFound();
            }
            return Ok(ingreso);
        }

        [HttpPost]
        public async Task<ActionResult<Ingresos>> Post(Ingresos ingreso)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldoPrincipal = await _context.Saldo.FirstOrDefaultAsync(s => s.UserId == userId);
            if (saldoPrincipal == null)
            {
                return BadRequest("Error crítico: No se encontró un saldo principal para el usuario actual para asociar el ingreso.");
            }
            ingreso.SaldoId = saldoPrincipal.Id;

            _context.Ingresos.Add(ingreso);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al guardar el ingreso.");
            }
            return CreatedAtAction(nameof(Get), new { id = ingreso.Id }, ingreso);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Ingresos ingreso)
        {
            if (id != ingreso.Id)
            {
                return BadRequest("El ID del ingreso en la ruta no coincide con el del cuerpo de la solicitud.");
            }
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existingIngreso = await _context.Ingresos
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(i => i.Id == id && i.Saldo.UserId == userId);
            if (existingIngreso == null)
            {
                return NotFound("Ingreso no encontrado o no pertenece al usuario.");
            }
            ingreso.SaldoId = existingIngreso.SaldoId;
            _context.Entry(ingreso).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Ingresos.AnyAsync(e => e.Id == id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al actualizar el ingreso.");
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var ingreso = await _context.Ingresos
                                      .FirstOrDefaultAsync(i => i.Id == id && i.Saldo.UserId == userId);
            if (ingreso == null)
            {
                return NotFound();
            }
            _context.Ingresos.Remove(ingreso);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}