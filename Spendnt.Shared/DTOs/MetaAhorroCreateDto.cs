// Spendnt.Shared/DTOs/MetaAhorroCreateDto.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spendnt.Shared.DTOs
{
    public class MetaAhorroCreateDto
    {
        [Required(ErrorMessage = "El nombre de la meta es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string? Descripcion { get; set; } 

        [Required(ErrorMessage = "El monto objetivo es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto objetivo debe ser mayor a cero.")]
        public decimal MontoObjetivo { get; set; }

  
        [Range(0, double.MaxValue, ErrorMessage = "El monto actual no puede ser negativo.")]
        public decimal MontoActual { get; set; } = 0;

        [DataType(DataType.Date)]
        public DateTime? FechaObjetivo { get; set; }

        public bool EstaCompletada { get; set; } = false;

     
    }
}