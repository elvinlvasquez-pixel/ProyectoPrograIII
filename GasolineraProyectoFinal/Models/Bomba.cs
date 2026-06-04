using GasolineraSimulacion.Models;

namespace GasolineraSimulacion.Models
{
    public class Bomba
    {
        public int Numero { get; private set; }
        public bool EnUso { get; private set; }
        public Abastecimiento AbastecimientoActual { get; private set; }

        public Bomba(int numero)
        {
            Numero = numero;
            EnUso = false;
            AbastecimientoActual = null;
        }

   
        public bool IniciarAbastecimiento(Abastecimiento abastecimiento)
        {
            if (EnUso) return false;
            EnUso = true;
            AbastecimientoActual = abastecimiento;
            return true;
        }

       
        public void FinalizarAbastecimiento()
        {
            EnUso = false;
            AbastecimientoActual = null;
        }

        public override string ToString()
        {
            return EnUso
                ? $"Bomba {Numero} - EN USO ({AbastecimientoActual?.NombreCliente})"
                : $"Bomba {Numero} - DISPONIBLE";
        }
    }
}