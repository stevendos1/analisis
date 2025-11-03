using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spendnt.Shared.DTOs
{
    public class CalendarioEventoDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string Tipo { get; set; }
        public decimal? Monto { get; set; }
        public string Color { get; set; }
        public string Url { get; set; }
    }
}
