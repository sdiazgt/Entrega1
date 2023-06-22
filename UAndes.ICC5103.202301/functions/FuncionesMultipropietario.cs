using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using UAndes.ICC5103._202301.Models;

namespace UAndes.ICC5103._202301.functions
{
    public class FuncionesMultipropietario
    {
        private InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();

        public void CrearMultipropietarios(List<Multipropietario> multipropietarios)
        {
            foreach (Multipropietario multipropietario in multipropietarios)
            {
                float porcentaje = float.Parse(multipropietario.PorcentajeDerechoPropietario);
                multipropietario.PorcentajeDerechoPropietario = porcentaje.ToString("F2");
                db.Multipropietario.Add(multipropietario);
                db.SaveChanges();
            }
        }

        public List<Multipropietario> CrearObjetoMultipropietario(List<List<string>> adquirientes, Enajenacion enajenacion)
        {
            List<string> stringsAVerificar = new List<string> { "YES", "NO" };
            string cero = "0.00";
            List<Multipropietario> multipropietarios = new List<Multipropietario>();

            int anoMinimo = 2019;
            foreach (List<string> adquiriente in adquirientes)
            {
                Multipropietario multipropietario = new Multipropietario();
                multipropietario.Comuna = enajenacion.Comuna;
                multipropietario.Manzana = enajenacion.Manzana;
                multipropietario.RolPredial = enajenacion.RolPredial;
                multipropietario.RutPropietario = adquiriente[0];
                multipropietario.PorcentajeDerechoPropietario = adquiriente[1];
                multipropietario.Foja = enajenacion.Foja;
                multipropietario.NumeroInscripcion = enajenacion.NumeroInscripcion;
                multipropietario.FechaInscripcion = enajenacion.FechaInscripcion;
                DateTime Fecha = enajenacion.FechaInscripcion;
                multipropietario.AnoInscripcion = Fecha.Year;
                if ((int)Fecha.Year <= anoMinimo)
                {
                    multipropietario.AnoVigenciaInicial = anoMinimo;
                }
                else { multipropietario.AnoVigenciaInicial = Fecha.Year; }

                if (adquiriente[2] == stringsAVerificar[0])
                {
                    multipropietario.PorcentajeDerechoPropietario = cero;
                }

                multipropietarios.Add(multipropietario);
            }
            return multipropietarios;
        }

        public List<Multipropietario> CrearObjetoMultipropietarioVacio(List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            string cero = "0.00";
            List<Multipropietario> multipropietarios = new List<Multipropietario>();

            foreach (List<string> enajenante in enajenantes)
            {
                int anoActual = enajenacion.FechaInscripcion.Year;
                Multipropietario multipropietario = new Multipropietario();
                multipropietario.Comuna = enajenacion.Comuna;
                multipropietario.Manzana = enajenacion.Manzana;
                multipropietario.RolPredial = enajenacion.RolPredial;
                multipropietario.RutPropietario = enajenante[0];
                multipropietario.PorcentajeDerechoPropietario = cero;
                multipropietario.Foja = null;
                multipropietario.NumeroInscripcion = null;
                multipropietario.FechaInscripcion = null;
                multipropietario.AnoInscripcion = null;
                multipropietario.AnoVigenciaInicial = anoActual;

                multipropietarios.Add(multipropietario);
            }
            return multipropietarios;
        }

        private List<Multipropietario> CrearObjetoMultipropetarioVigente(List<Multipropietario> multipropietariosVigentes, Enajenacion enajenacion)
        {
            List<Multipropietario> multipropietarios = new List<Multipropietario>();

            foreach (Multipropietario multipropietariosVigente in multipropietariosVigentes)
            {
                string rut = multipropietariosVigente.RutPropietario;
                string porcentaje = multipropietariosVigente.PorcentajeDerechoPropietario;
                string NumeroInscripcion = multipropietariosVigente.NumeroInscripcion;
                string foja = multipropietariosVigente.Foja;
                DateTime FechaInscripcion = (DateTime)multipropietariosVigente.FechaInscripcion;
                int ano = FechaInscripcion.Year;

                Multipropietario multipropietario = new Multipropietario();
                multipropietario.Comuna = enajenacion.Comuna;
                multipropietario.Manzana = enajenacion.Manzana;
                multipropietario.RolPredial = enajenacion.RolPredial;
                multipropietario.RutPropietario = rut;
                multipropietario.PorcentajeDerechoPropietario = porcentaje;
                multipropietario.Foja = foja;
                multipropietario.NumeroInscripcion = NumeroInscripcion;
                multipropietario.FechaInscripcion = FechaInscripcion;
                multipropietario.AnoInscripcion = ano;

                multipropietarios.Add(multipropietario);

            }
            return multipropietarios;
        }

        private List<Multipropietario> ObtenerMultipropietarioVigentes(Enajenacion enajenacion)
        {
            var multipropietariosVigentes = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaFinal == null)
                    .ToList();

            return CrearObjetoMultipropetarioVigente(multipropietariosVigentes, enajenacion);
        }

        public List<Multipropietario> CambiarFechaInicialMultipropietario(List<Multipropietario> multipropietarios, Enajenacion enajenacion)
        {
            int ano = enajenacion.FechaInscripcion.Year;
            foreach (Multipropietario multipropietario in multipropietarios)
            {
                multipropietario.AnoVigenciaInicial = ano;
            }
            return multipropietarios;
        }

        public void CerrarVigenciaMultipropietario(Enajenacion enajenacion)
        {
            int anoMinimo = 2019;

            var adquirientesACambiarVigencia = db.Multipropietario
                .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                .Where(Data4 => Data4.AnoVigenciaFinal == null)
                .ToList();

            if (adquirientesACambiarVigencia.Count > 0)
            {
                foreach (Multipropietario multipropietario in adquirientesACambiarVigencia)
                {
                    if ((int)multipropietario.AnoVigenciaInicial <= anoMinimo)
                    {
                        multipropietario.AnoVigenciaFinal = anoMinimo;
                    }
                    else { multipropietario.AnoVigenciaFinal = (int)enajenacion.FechaInscripcion.Year - 1; }
                }
            }
            db.SaveChanges();
        }

        public bool EsMultipropietarioVigente(string rut, Enajenacion enajenacion)
        {
            var multipropietariosVigentes = db.Multipropietario
                .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                .Where(Data4 => Data4.AnoVigenciaFinal == null)
                .Where(Data4 => Data4.RutPropietario == rut)
                .ToList();
            if (multipropietariosVigentes.Count >= 1)
            {
                return true;
            }
            return false;
        }

        public List<Multipropietario> FormularioAProcesar(List<List<string>> adquirientes, Enajenacion enajenacion)
        {
            List<Multipropietario> multipropietariosVigentes = ObtenerMultipropietarioVigentes(enajenacion);
            List<Multipropietario> multipropietariosNuevos = CrearObjetoMultipropietario(adquirientes, enajenacion);

            List<Multipropietario> multipropietariosParaProcesar = multipropietariosVigentes.Concat(multipropietariosNuevos).ToList();
            return new List<Multipropietario>(multipropietariosParaProcesar);
        }
    }
}