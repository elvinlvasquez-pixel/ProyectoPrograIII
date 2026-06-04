using System;
using System.Collections.Generic;
using System.Linq;
using GasolineraSimulacion.Models;

namespace GasolineraSimulacion.Services
{
   
    public class EstadisticasService
    {
        private readonly RegistroService _registro;

        public EstadisticasService(RegistroService registro)
        {
            _registro = registro;
        }

        // Cierre de caja diario
       
        public List<Abastecimiento> CierreDiario(DateTime fecha)
        {
            return _registro.ObtenerTodos()
                .Where(a => a.FechaHora.Date == fecha.Date)
                .OrderBy(a => a.FechaHora)
                .ToList();
        }

        
        public double TotalDiario(DateTime fecha)
        {
            return CierreDiario(fecha).Sum(a => a.MontoFinal);
        }

       
        public double LitrosDiarios(DateTime fecha)
        {
            return CierreDiario(fecha).Sum(a => a.LitrosServidos);
        }

     

        public List<Abastecimiento> InformePrepago()
        {
            return _registro.ObtenerTodos()
                .Where(a => a.Tipo == TipoPago.Prepago)
                .OrderByDescending(a => a.FechaHora)
                .ToList();
        }

        public List<Abastecimiento> InformeTanqueLleno()
        {
            return _registro.ObtenerTodos()
                .Where(a => a.Tipo == TipoPago.TanqueLleno)
                .OrderByDescending(a => a.FechaHora)
                .ToList();
        }

       
        public Dictionary<int, int> ConteoPorBomba()
        {
            var todos = _registro.ObtenerTodos();
            var resultado = new Dictionary<int, int>();
            for (int i = 1; i <= 4; i++)
                resultado[i] = todos.Count(a => a.NumeroBomba == i);
            return resultado;
        }

     
        public (int masBomba, int menosBomba) BombasMasYMenosUsadas()
        {
            var conteo = ConteoPorBomba();
            int mas = conteo.OrderByDescending(kvp => kvp.Value).First().Key;
            int menos = conteo.OrderBy(kvp => kvp.Value).First().Key;
            return (mas, menos);
        }

        

        public ResumenGeneral ObtenerResumenGeneral()
        {
            var todos = _registro.ObtenerTodos();
            var (mas, menos) = todos.Count > 0
                ? BombasMasYMenosUsadas()
                : (0, 0);

            return new ResumenGeneral
            {
                TotalAbastecimientos = todos.Count,
                TotalLitros = todos.Sum(a => a.LitrosServidos),
                TotalRecaudado = todos.Sum(a => a.MontoFinal),
                TotalPrepago = todos.Count(a => a.Tipo == TipoPago.Prepago),
                TotalTanqueLleno = todos.Count(a => a.Tipo == TipoPago.TanqueLleno),
                BombaMasUsada = mas,
                BombaMenosUsada = menos
            };
        }
    }

    public class ResumenGeneral
    {
        public int TotalAbastecimientos { get; set; }
        public double TotalLitros { get; set; }
        public double TotalRecaudado { get; set; }
        public int TotalPrepago { get; set; }
        public int TotalTanqueLleno { get; set; }
        public int BombaMasUsada { get; set; }
        public int BombaMenosUsada { get; set; }
    }
}