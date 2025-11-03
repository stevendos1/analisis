// Spendnt.API/Controllers/CalendarioEventosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spendnt.API.Data;
using Spendnt.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spendnt.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/calendarioeventos")]
    public class CalendarioEventosController : ControllerBase
    {
        private readonly DataContext _context;

        public CalendarioEventosController(DataContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarioEventoDto>>> GetEventos(
            [FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventos = new List<CalendarioEventoDto>();
            var saldoUsuarioId = await _context.Saldo
                                        .Where(s => s.UserId == userId)
                                        .Select(s => s.Id)
                                        .FirstOrDefaultAsync();

            if (saldoUsuarioId == 0)
            {
                return Ok(eventos);
            }

            var ingresos = await _context.Ingresos
                .Where(i => i.Saldo.UserId == userId && i.Fecha >= fechaInicio && i.Fecha <= fechaFin)
                .Select(i => new { i.Id, i.Fecha, i.Ingreso, CategoriaNombre = i.Categoria.Nombre })
                .ToListAsync();

            eventos.AddRange(ingresos.Select(i => new CalendarioEventoDto
            {
                Id = $"ingreso-{i.Id}",
                Title = $"Ingreso: {i.CategoriaNombre ?? "Sin categoría"} ({i.Ingreso:C})",
                Start = i.Fecha,
                Tipo = "ingreso",
                Monto = i.Ingreso,
                Color = "green"
            }));

            var egresos = await _context.Egresos
                .Where(e => e.Saldo.UserId == userId && e.Fecha >= fechaInicio && e.Fecha <= fechaFin)
                .Select(e => new { e.Id, e.Fecha, e.Egreso, CategoriaNombre = e.Categoria.Nombre })
                .ToListAsync();
            eventos.AddRange(egresos.Select(e => new CalendarioEventoDto
            {
                Id = $"egreso-{e.Id}",
                Title = $"Egreso: {e.CategoriaNombre ?? "Sin categoría"} ({e.Egreso:C})",
                Start = e.Fecha,
                Tipo = "egreso",
                Monto = e.Egreso,
                Color = "red"
            }));

            var recordatorios = await _context.RecordatoriosGasto
                .Where(r => r.UserId == userId && r.FechaProgramada >= fechaInicio && r.FechaProgramada <= fechaFin)
                .ToListAsync();
            eventos.AddRange(recordatorios.Select(r => new CalendarioEventoDto
            {
                Id = $"recordatorio-{r.Id}",
                Title = $"Recordatorio: {r.Titulo} ({r.MontoEstimado:C})",
                Start = r.FechaProgramada,
                Tipo = "recordatorio",
                Monto = r.MontoEstimado,
                Color = "orange"
            }));

            var metas = await _context.MetasAhorro
                 .Where(m => m.UserId == userId && m.FechaObjetivo.HasValue && m.FechaObjetivo.Value >= fechaInicio && m.FechaObjetivo.Value <= fechaFin)
                .ToListAsync();
            eventos.AddRange(metas.Select(m => new CalendarioEventoDto
            {
                Id = $"meta-{m.Id}",
                Title = $"Meta: {m.Nombre} (Objetivo: {m.MontoObjetivo:C})",
                Start = m.FechaObjetivo.Value,
                Tipo = "meta",
                Monto = m.MontoObjetivo,
                Color = "blue"
            }));

            return Ok(eventos.OrderBy(e => e.Start));
        }
    }
}