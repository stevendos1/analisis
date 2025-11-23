using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spendnt.Shared.Entities
{
    public class Wallet
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public bool IsLocked { get; set; } = false;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public ICollection<Ingresos> Ingresos { get; set; }
        public ICollection<Egresos> Egresos { get; set; }
        public ICollection<MetaAhorro> MetasAhorro { get; set; }
        public ICollection<Historial> Historial { get; set; }
    }
}
