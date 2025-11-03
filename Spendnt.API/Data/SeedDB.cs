// Spendnt.API/Data/SeedDB.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spendnt.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spendnt.API.Data
{
    public class SeedDB
    {
        private readonly DataContext _context;

        public SeedDB(DataContext context)
        {
            _context = context;
        }

        public async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            await _context.Database.EnsureCreatedAsync();

            await CheckRolesAsync(roleManager);
            var testUser = await CheckUserAsync(userManager, "testuser@example.com", "Test", "User", "Password123!");

            if (testUser != null)
            {
                await CheckCategoriasAsync();
                await CheckSaldosConIngresosEgresosAsync(testUser.Id);
                await CheckHistorialAsync(testUser.Id);
                await CheckRecordatoriosGastoAsync(testUser.Id);
                await CheckMetasAhorroAsync(testUser.Id);
            }
        }

        private async Task CheckRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
        }

        private async Task<User?> CheckUserAsync(UserManager<User> userManager, string email, string firstName, string lastName, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    Console.WriteLine($"Error creando usuario semilla {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return null;
                }
            }
            return user;
        }

        private async Task CheckCategoriasAsync()
        {
            if (!_context.Categorias.Any())
            {
                var categorias = new List<Categoria>
                {
                    new Categoria { Nombre = "Alimentación", Descripcion = "Gastos en supermercado, restaurantes, etc." },
                    new Categoria { Nombre = "Transporte", Descripcion = "Gastos de combustible, transporte público, mantenimiento vehículo." },
                    new Categoria { Nombre = "Vivienda", Descripcion = "Alquiler, hipoteca, servicios (luz, agua, gas)." },
                    new Categoria { Nombre = "Ocio", Descripcion = "Cine, conciertos, salidas, hobbies." },
                    new Categoria { Nombre = "Salud", Descripcion = "Médico, farmacia, seguros de salud." },
                    new Categoria { Nombre = "Educación", Descripcion = "Cursos, libros, matrículas." },
                    new Categoria { Nombre = "Ropa y Accesorios", Descripcion = "Compra de vestimenta y complementos." },
                    new Categoria { Nombre = "Regalos y Donaciones", Descripcion = "Obsequios y contribuciones benéficas." },
                    new Categoria { Nombre = "Otros Gastos", Descripcion = "Gastos varios no clasificados." },
                    new Categoria { Nombre = "Sueldo", Descripcion = "Ingreso principal por trabajo." },
                    new Categoria { Nombre = "Ingresos Freelance", Descripcion = "Ingresos por trabajos independientes." },
                    new Categoria { Nombre = "Inversiones", Descripcion = "Rendimientos de inversiones." },
                    new Categoria { Nombre = "Regalos Recibidos", Descripcion = "Dinero recibido como obsequio." },
                    new Categoria { Nombre = "Otros Ingresos", Descripcion = "Ingresos varios no clasificados." }
                };
                _context.Categorias.AddRange(categorias);
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckSaldosConIngresosEgresosAsync(string userId)
        {
            if (!await _context.Saldo.AnyAsync(s => s.UserId == userId))
            {
                var catSueldo = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Sueldo");
                var catFreelance = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Ingresos Freelance");
                var catOtrosIngresos = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Otros Ingresos");
                var catAlimentacion = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Alimentación");
                var catTransporte = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Transporte");
                var catOcio = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Ocio");

                if (catSueldo == null || catFreelance == null || catOtrosIngresos == null ||
                    catAlimentacion == null || catTransporte == null || catOcio == null)
                {
                    Console.WriteLine("ADVERTENCIA: Faltan categorías esenciales para sembrar Saldo. Se omitirá para el usuario: " + userId);
                    return;
                }

                var saldoPrincipal = new Saldo { UserId = userId };
                _context.Saldo.Add(saldoPrincipal);
                await _context.SaveChangesAsync();

                var ingresos = new List<Ingresos>
                {
                    new Ingresos { Ingreso = 2200.75M, Fecha = DateTime.UtcNow.AddDays(-25), CategoriaId = catSueldo.Id, SaldoId = saldoPrincipal.Id },
                    new Ingresos { Ingreso = 350.00M, Fecha = DateTime.UtcNow.AddDays(-15), CategoriaId = catFreelance.Id, SaldoId = saldoPrincipal.Id },
                    new Ingresos { Ingreso = 80.50M, Fecha = DateTime.UtcNow.AddDays(-5), CategoriaId = catOtrosIngresos.Id, SaldoId = saldoPrincipal.Id }
                };
                _context.Ingresos.AddRange(ingresos);

                var egresos = new List<Egresos>
                {
                    new Egresos { Egreso = 450.20M, Fecha = DateTime.UtcNow.AddDays(-20), CategoriaId = catAlimentacion.Id, SaldoId = saldoPrincipal.Id },
                    new Egresos { Egreso = 75.00M, Fecha = DateTime.UtcNow.AddDays(-10), CategoriaId = catTransporte.Id, SaldoId = saldoPrincipal.Id },
                    new Egresos { Egreso = 120.99M, Fecha = DateTime.UtcNow.AddDays(-3), CategoriaId = catOcio.Id, SaldoId = saldoPrincipal.Id }
                };
                _context.Egresos.AddRange(egresos);

                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckHistorialAsync(string userId)
        {
            var saldoUsuario = await _context.Saldo.FirstOrDefaultAsync(s => s.UserId == userId);
            if (saldoUsuario == null)
            {
                Console.WriteLine("ADVERTENCIA: No se encontró saldo para el usuario " + userId + " al sembrar historial.");
                return;
            }

            if (!_context.Historiales.Any(h => h.SaldoId == saldoUsuario.Id))
            {
                var catSueldo = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Sueldo");
                var catAlimentacion = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Alimentación");
                var catOcio = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Ocio");

                if (catSueldo != null && catAlimentacion != null && catOcio != null)
                {
                    var historiales = new List<Historial>
                    {
                        new Historial { Fecha = DateTime.UtcNow.AddMonths(-1).AddDays(5), Monto = 2150.00M, Tipo = "Ingreso", Descripcion = "Nómina mes anterior", CategoriaId = catSueldo.Id, SaldoId = saldoUsuario.Id },
                        new Historial { Fecha = DateTime.UtcNow.AddMonths(-1).AddDays(7), Monto = 88.40M, Tipo = "Egreso", Descripcion = "Compra semanal supermercado", CategoriaId = catAlimentacion.Id, SaldoId = saldoUsuario.Id },
                        new Historial { Fecha = DateTime.UtcNow.AddMonths(-1).AddDays(15), Monto = 50.00M, Tipo = "Egreso", Descripcion = "Cena con amigos", CategoriaId = catOcio.Id, SaldoId = saldoUsuario.Id }
                    };
                    _context.Historiales.AddRange(historiales);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task CheckRecordatoriosGastoAsync(string userId)
        {
            if (!_context.RecordatoriosGasto.Any(r => r.UserId == userId))
            {
                var recordatorios = new List<RecordatorioGasto>
                {
                    new RecordatorioGasto { Titulo = "Pagar Alquiler", MontoEstimado = 750.00M, FechaProgramada = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1), Notas = "Transferencia mensual", UserId = userId },
                    new RecordatorioGasto { Titulo = "Seguro Coche", MontoEstimado = 320.00M, FechaProgramada = DateTime.UtcNow.AddMonths(2).AddDays(10), Notas = "Revisar renovación", UserId = userId }
                };
                _context.RecordatoriosGasto.AddRange(recordatorios);
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckMetasAhorroAsync(string userId)
        {
            if (!_context.MetasAhorro.Any(m => m.UserId == userId))
            {
                var metas = new List<MetaAhorro>
                {
                    new MetaAhorro { Nombre = "Vacaciones Usuario", Descripcion = "Viaje personal", MontoObjetivo = 1500.00M, MontoActual = 350.00M, FechaObjetivo = new DateTime(2025, 7, 1), FechaCreacion = DateTime.UtcNow.AddMonths(-2), EstaCompletada = false, UserId = userId },
                    new MetaAhorro { Nombre = "Fondo Emergencia Usuario", Descripcion = "Para imprevistos personales", MontoObjetivo = 2000.00M, MontoActual = 1000.00M, FechaCreacion = DateTime.UtcNow.AddYears(-1), EstaCompletada = false, UserId = userId }
                };
                _context.MetasAhorro.AddRange(metas);
                await _context.SaveChangesAsync();
            }
        }
    }
}