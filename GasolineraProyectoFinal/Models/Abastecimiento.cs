using System;

namespace GasolineraSimulacion.Models
{
    public class Abastecimiento
    {
        public int Id { get; set; }
        public int NumeroBomba { get; set; }
        public string NombreCliente { get; set; }
        public TipoPago Tipo { get; set; }

        // Solo usado en Prepago
        public double MontoPagado { get; set; }
        public double LitrosSolicitados { get; set; }

        // Se actualiza al finalizar (puede ser menor si se interrumpió)
        public double LitrosServidos { get; set; }

        // Para TanqueLleno se calcula al final; para Prepago es MontoPagado (o menos si no terminó)
        public double MontoFinal { get; set; }

        // Asignado automáticamente al crear
        public DateTime FechaHora { get; set; }

        public bool Completado { get; set; }

        // Precio por litro vigente al momento del abastecimiento
        public double PrecioPorLitro { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {FechaHora:dd/MM/yyyy HH:mm} | Bomba {NumeroBomba} | " +
                   $"{NombreCliente} | {Tipo} | {LitrosServidos:F2}L | Q{MontoFinal:F2}";
        }
    }
}