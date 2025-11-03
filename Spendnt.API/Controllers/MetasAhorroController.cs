// Spendnt.API/Controllers/MetasAhorroController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spendnt.API.Data;
using Spendnt.Shared.DTOs; 
using Spendnt.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spendnt.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MetasAhorroController : ControllerBase
    {
        private readonly DataContext _context;

        public MetasAhorroController(DataContext context)
        {
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetaAhorro>>> GetMetasAhorro()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            return await _context.MetasAhorro
                                 .Where(m => m.UserId == userId)
                                 .OrderByDescending(m => m.FechaCreacion)
                                 .ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MetaAhorro>> GetMetaAhorro(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var metaAhorro = await _context.MetasAhorro
                                         .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (metaAhorro == null)
            {
                return NotFound($"No se encontró la meta de ahorro con ID {id} para el usuario actual.");
            }
            return metaAhorro;
        }

        [HttpPost]
        public async Task<ActionResult<MetaAhorro>> PostMetaAhorro(MetaAhorroCreateDto metaDto) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No se pudo identificar al usuario para crear la meta.");
            }

            var metaAhorro = new MetaAhorro
            {
                Nombre = metaDto.Nombre,
                Descripcion = metaDto.Descripcion,
                MontoObjetivo = metaDto.MontoObjetivo,
                MontoActual = metaDto.MontoActual, 
                FechaObjetivo = metaDto.FechaObjetivo,
                EstaCompletada = metaDto.EstaCompletada,
                UserId = userId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.MetasAhorro.Add(metaAhorro);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log ex.InnerException para más detalles
                Console.WriteLine($"Error en PostMetaAhorro: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al guardar la meta de ahorro.");
            }
            return CreatedAtAction(nameof(GetMetaAhorro), new { id = metaAhorro.Id }, metaAhorro);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutMetaAhorro(int id, MetaAhorro metaAhorroUpdateData) 
        {
            if (id != metaAhorroUpdateData.Id)
            {
                return BadRequest("El ID en la ruta no coincide con el ID en el cuerpo de la solicitud.");
            }

            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var existingMeta = await _context.MetasAhorro.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (existingMeta == null)
            {
                return NotFound($"No se encontró la meta de ahorro con ID {id} para el usuario actual, o no tienes permiso para modificarla.");
            }

           
            existingMeta.Nombre = metaAhorroUpdateData.Nombre;
            existingMeta.Descripcion = metaAhorroUpdateData.Descripcion;
            existingMeta.MontoObjetivo = metaAhorroUpdateData.MontoObjetivo;
            existingMeta.MontoActual = metaAhorroUpdateData.MontoActual;
            existingMeta.FechaObjetivo = metaAhorroUpdateData.FechaObjetivo;
            existingMeta.EstaCompletada = metaAhorroUpdateData.EstaCompletada;
            

            _context.Entry(existingMeta).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.MetasAhorro.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error en PutMetaAhorro: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al actualizar la meta de ahorro.");
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMetaAhorro(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var metaAhorro = await _context.MetasAhorro.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (metaAhorro == null)
            {
                return NotFound($"No se encontró la meta de ahorro con ID {id} para el usuario actual, o no tienes permiso para eliminarla.");
            }

            _context.MetasAhorro.Remove(metaAhorro);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id:int}/contribuir")]
        public async Task<IActionResult> ContribuirMetaAhorro(int id, [FromBody] decimal monto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var metaAhorro = await _context.MetasAhorro.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (metaAhorro == null)
            {
                return NotFound("Meta de ahorro no encontrada o no pertenece al usuario.");
            }

            metaAhorro.MontoActual += monto;
            if (metaAhorro.MontoActual < 0) metaAhorro.MontoActual = 0;
            metaAhorro.EstaCompletada = metaAhorro.MontoActual >= metaAhorro.MontoObjetivo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Hubo un problema de concurrencia al actualizar la meta. Inténtalo de nuevo.");
            }
            return Ok(metaAhorro);
        }
    }
}