// Spendnt.API/Controllers/TransaccionesAhorroController.cs
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
    [Route("api/metasahorro/{metaAhorroId}/transacciones")]
    public class TransaccionesAhorroController : ControllerBase
    {
        private readonly DataContext _context;

        public TransaccionesAhorroController(DataContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransaccionAhorro>>> GetTransaccionesAhorro(int metaAhorroId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var meta = await _context.MetasAhorro.FirstOrDefaultAsync(m => m.Id == metaAhorroId && m.UserId == userId);
            if (meta == null) return NotFound("Meta no encontrada o no pertenece al usuario.");

            return await _context.TransaccionesAhorro
                                 .Where(t => t.MetaAhorroId == metaAhorroId)
                                 .OrderByDescending(t => t.Fecha)
                                 .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<TransaccionAhorro>> PostTransaccionAhorro(int metaAhorroId, TransaccionAhorro transaccion)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var meta = await _context.MetasAhorro.FirstOrDefaultAsync(m => m.Id == metaAhorroId && m.UserId == userId);
            if (meta == null) return NotFound("Meta no encontrada o no pertenece al usuario.");

            if (transaccion.MetaAhorroId != 0 && transaccion.MetaAhorroId != metaAhorroId)
            {
                return BadRequest("El ID de la meta en la transacción no coincide con el de la ruta.");
            }
            transaccion.MetaAhorroId = metaAhorroId;
            transaccion.Fecha = DateTime.UtcNow;

            _context.TransaccionesAhorro.Add(transaccion);
            meta.MontoActual += transaccion.Monto;
            if (meta.MontoActual < 0) meta.MontoActual = 0;
            if (meta.MontoActual >= meta.MontoObjetivo)
            {
                meta.EstaCompletada = true;
            }
            else
            {
                meta.EstaCompletada = false;
            }
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTransaccionesAhorro), new { metaAhorroId = meta.Id }, transaccion);
        }
    }
}