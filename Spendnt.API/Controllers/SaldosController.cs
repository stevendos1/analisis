// Spendnt.API/Controllers/SaldoController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spendnt.API.Data;
using Spendnt.Shared.Entities;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spendnt.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/Saldo")]
    public class SaldoController : ControllerBase
    {
        private readonly DataContext _context;

        public SaldoController(DataContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private void CalcularYAsignarTotales(Saldo saldo)
        {
            if (saldo != null)
            {
                saldo.TotalIngresos = saldo.Ingresos?.Sum(i => i.Ingreso) ?? 0;
                saldo.TotalEgresos = saldo.Egresos?.Sum(e => e.Egreso) ?? 0;
                saldo.TotalSaldoCalculado = saldo.TotalIngresos - saldo.TotalEgresos;
            }
        }

        [HttpGet("actual")]
        public async Task<ActionResult<Saldo>> GetCurrentSaldo()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldo = await _context.Saldo
                                .Where(s => s.UserId == userId)
                                .Include(s => s.Ingresos)
                                .Include(s => s.Egresos)
                                .FirstOrDefaultAsync();

            if (saldo == null)
            {
                return NotFound("No se encontró un registro de saldo principal para el usuario actual.");
            }
            CalcularYAsignarTotales(saldo);
            return Ok(saldo);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Saldo>> GetSaldoById(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldo = await _context.Saldo
                                .Where(s => s.UserId == userId && s.Id == id)
                                .Include(s => s.Ingresos)
                                .Include(s => s.Egresos)
                                .FirstOrDefaultAsync();

            if (saldo == null)
            {
                return NotFound();
            }
            CalcularYAsignarTotales(saldo);
            return Ok(saldo);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutSaldo(int id, Saldo saldoActualizado)
        {
            if (id != saldoActualizado.Id)
            {
                return BadRequest("El ID del saldo en la ruta no coincide con el del cuerpo.");
            }
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saldoExistente = await _context.Saldo
                                        .Include(s => s.Ingresos)
                                        .Include(s => s.Egresos)
                                        .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (saldoExistente == null)
            {
                return NotFound("El saldo a actualizar no fue encontrado o no pertenece al usuario.");
            }
            saldoExistente.TotalSaldo = saldoActualizado.TotalSaldo;
            CalcularYAsignarTotales(saldoExistente);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Saldo.AnyAsync(e => e.Id == id)) return NotFound();
                else throw;
            }
            return Ok(saldoExistente);
        }
    }
}