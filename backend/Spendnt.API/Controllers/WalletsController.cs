using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spendnt.API.Data;
using Spendnt.Shared.Entities;
using System.Security.Claims;

namespace Spendnt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletsController : ControllerBase
    {
        private readonly DataContext _context;

        public WalletsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Wallet>>> GetWallets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Wallets.Where(w => w.UserId == userId).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Wallet>> GetWallet(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (wallet == null)
            {
                return NotFound();
            }

            return wallet;
        }

        [HttpPost]
        public async Task<ActionResult<Wallet>> PostWallet(Wallet wallet)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            wallet.UserId = userId;

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWallet", new { id = wallet.Id }, wallet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutWallet(int id, Wallet wallet)
        {
            if (id != wallet.Id)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingWallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (existingWallet == null)
            {
                return NotFound();
            }

            existingWallet.Name = wallet.Name;
            existingWallet.IsLocked = wallet.IsLocked;
            // Balance is managed by transactions usually, but we might allow editing for initial setup or adjustment if needed.
            // However, usually balance should be read-only derived or carefully managed.
            // For now, I will let it be updated if passed, or we can choose to ignore it.
            // Requirement says "Gestion de Carteras", usually implies just CRUD of container.

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (wallet == null)
            {
                return NotFound();
            }

            _context.Wallets.Remove(wallet);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
