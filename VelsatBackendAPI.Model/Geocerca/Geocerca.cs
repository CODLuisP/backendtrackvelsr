using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model.Geocerca
{
    public class Geocerca
    {
        public int Idgeocerca { get; set; }
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public double? Radio { get; set; }
        public string? Puntoorigen { get; set; }
        public string? Segundopunto { get; set; }
        public string? Tercerpunto { get; set; }
        public string? Puntofinal { get; set; }

    }
}
