using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using GasolineraSimulacion.Models;

namespace GasolineraSimulacion.Services
{
    
    public class RegistroService
    {
        private readonly string _rutaArchivo;
        private List<Abastecimiento> _registros;

        private static readonly JsonSerializerOptions _opciones = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public RegistroService(string rutaArchivo = "Data/abastecimientos.json")
        {
            _rutaArchivo = rutaArchivo;
            Directory.CreateDirectory(Path.GetDirectoryName(_rutaArchivo) ?? "Data");
            Cargar();
        }

        
        public void AgregarAbastecimiento(Abastecimiento a)
        {
            a.Id = _registros.Count > 0 ? _registros.Max(r => r.Id) + 1 : 1;
            a.FechaHora = DateTime.Now;
            _registros.Add(a);
            Guardar();
        }

      
        public bool ActualizarAbastecimiento(Abastecimiento actualizado)
        {
            int idx = _registros.FindIndex(r => r.Id == actualizado.Id);
            if (idx < 0) return false;
            _registros[idx] = actualizado;
            Guardar();
            return true;
        }

        public List<Abastecimiento> ObtenerTodos() => new List<Abastecimiento>(_registros);

        public Abastecimiento ObtenerPorId(int id) =>
            _registros.FirstOrDefault(r => r.Id == id);

      

        private void Cargar()
        {
            if (!File.Exists(_rutaArchivo))
            {
                _registros = new List<Abastecimiento>();
                return;
            }
            try
            {
                string json = File.ReadAllText(_rutaArchivo);
                _registros = JsonSerializer.Deserialize<List<Abastecimiento>>(json, _opciones)
                             ?? new List<Abastecimiento>();
            }
            catch
            {
                // Si el archivo está corrupto, empezar limpio
                _registros = new List<Abastecimiento>();
            }
        }

        public void Guardar()
        {
            string json = JsonSerializer.Serialize(_registros, _opciones);
            File.WriteAllText(_rutaArchivo, json);
        }

       

        private readonly string _rutaPrecio = "Data/precio.json";

        public double ObtenerPrecioPorLitro()
        {
            if (!File.Exists(_rutaPrecio)) return 10.0; // valor por defecto
            try
            {
                string json = File.ReadAllText(_rutaPrecio);
                var obj = JsonSerializer.Deserialize<PrecioConfig>(json);
                return obj?.PrecioPorLitro ?? 10.0;
            }
            catch { return 10.0; }
        }

        public void GuardarPrecioPorLitro(double precio)
        {
            Directory.CreateDirectory("Data");
            var obj = new PrecioConfig { PrecioPorLitro = precio };
            File.WriteAllText(_rutaPrecio, JsonSerializer.Serialize(obj, _opciones));
        }

        private class PrecioConfig
        {
            public double PrecioPorLitro { get; set; }
        }
    }
}