using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spendnt.API.Data;
using Spendnt.API.Helpers;
using Spendnt.Shared.DTOs;
using Spendnt.Shared.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spendnt.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IFileStorage _fileStorage;

        public UserProfileController(UserManager<User> userManager, IFileStorage fileStorage)
        {
            _userManager = userManager;
            _fileStorage = fileStorage;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet]
        public async Task<ActionResult<UserViewModel>> GetUserProfile()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var roles = await _userManager.GetRolesAsync(user) ?? new List<string>();

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Roles = roles
            };

            return Ok(userViewModel);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserProfile(UserProfileUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return NoContent();
        }

        [HttpPost("picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "No se ha seleccionado ningún archivo." });
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                try
                {
                    await _fileStorage.DeleteFileAsync(user.ProfilePictureUrl, "profile_pictures");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar foto de perfil anterior: {ex.Message}");
                }
            }

            string fileUrl;
            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();
                var extension = Path.GetExtension(file.FileName);
                fileUrl = await _fileStorage.SaveFileAsync(content, extension, "profile_pictures");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar archivo: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Ocurrió un error al guardar la foto de perfil." });
            }

            user.ProfilePictureUrl = fileUrl;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                try
                {
                    await _fileStorage.DeleteFileAsync(fileUrl, "profile_pictures");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar foto de perfil tras fallo de actualización de usuario: {ex.Message}");
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok(new { ProfilePictureUrl = fileUrl });
        }
    }
}