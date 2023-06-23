using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UAndes.ICC5103._202301.Models;

namespace UAndes.ICC5103._202301.functions
{
    public class RecalcularFormulario
    {
        private readonly InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();
        private readonly FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario();
        private readonly CasosEnajenantesFantasmas CasosFantasma = new CasosEnajenantesFantasmas();
        private readonly CasosEnajenantes CasosNoFantasma = new CasosEnajenantes();
        private readonly CasosGenerales CasosGenerales = new CasosGenerales();
        private readonly FuncionesFormulario formulario = new FuncionesFormulario();

        private void LimpiarMultipropietariosAnteriores(string comuna, string manzana, string predio)
        {
            var formularios = db.Multipropietario
                .Where(Data1 => Data1.Comuna == comuna)
                .Where(Data2 => Data2.Manzana == manzana)
                .Where(Data3 => Data3.RolPredial == predio)
                .ToList();

            foreach (Multipropietario formulario in formularios)
            {
                db.Multipropietario.Remove(formulario);
            }
            db.SaveChanges();
        }

        private (List<List<string>>, List<List<string>>) DeserealizarJson(string adquirientes, string enajenantes)
        {
            List<List<string>> jsonAdquirientes = JsonConvert.DeserializeObject<List<List<string>>>(adquirientes);
            List<List<string>> jsonEnajenantes = JsonConvert.DeserializeObject<List<List<string>>>(enajenantes);
            return(jsonAdquirientes, jsonEnajenantes);
        }

        private void ProcesarRecalculado(List<Enajenacion> formularios)
        {
            string compraVenta = "1";
            string regularizacionDePatrimonio = "2";
            
            foreach (Enajenacion enajenacion in formularios)
            {
                var jsonDeserealizado = DeserealizarJson(enajenacion.Adquirientes, enajenacion.Enajenantes);
                List<List<string>> adquirientes = jsonDeserealizado.Item1;
                List<List<string>> enajenantes = jsonDeserealizado.Item2;

                if (enajenacion.CNE == compraVenta)
                {
                    List<Multipropietario> multipropietariosProcesados = funcionMultipropietario.FormularioAProcesar(adquirientes, enajenacion);
                    List<Multipropietario> multipropietarios = funcionMultipropietario.CambiarFechaInicialMultipropietario(multipropietariosProcesados, enajenacion);

                    if (CasosFantasma.CasoEnajenateFantasma(multipropietarios, adquirientes, enajenantes, enajenacion) == false)
                    {
                        CasosNoFantasma.CasoEnajenantes(multipropietarios, adquirientes, enajenantes, enajenacion);
                    }
                    CasosGenerales.CasosPostProcesamiento(enajenacion);
                }
                else if (enajenacion.CNE == regularizacionDePatrimonio)
                {
                    var adquirientesPorAcreditacion = formulario.ObtenerAdquirientesPorAcreditacion(adquirientes);
                    List<List<string>> adquirientesAcredidatos = adquirientesPorAcreditacion.Item1;
                    List<List<string>> adquirientesNoAcredidatos = adquirientesPorAcreditacion.Item2;

                    adquirientes = formulario.ProcesarAdquirientesPorAcreditacion(adquirientesAcredidatos, adquirientesNoAcredidatos);

                    List<Multipropietario> multipropietariosACrear = funcionMultipropietario.CrearObjetoMultipropietario(adquirientes, enajenacion);
                    funcionMultipropietario.CerrarVigenciaMultipropietario(enajenacion);
                    funcionMultipropietario.CrearMultipropietarios(multipropietariosACrear);
                }
            }
        }

        private List<Enajenacion> ObtenerFormularios(string comuna, string manzana, string predio)
        {
            var formularios = db.Enajenacion
                    .Where(Data1 => Data1.Comuna == comuna)
                    .Where(Data2 => Data2.Manzana == manzana)
                    .Where(Data3 => Data3.RolPredial == predio)
                    .ToList();

            return formularios;
        }

        private bool EsFormularioRepetido(List<Enajenacion> formularios, Enajenacion formularioARevisar)
        {
            foreach (Enajenacion formulario in formularios)
            {
                string numeroInscripcionRevisar = formularioARevisar.NumeroInscripcion;
                string numeroInscripcion = formulario.NumeroInscripcion;
                if (numeroInscripcionRevisar == numeroInscripcion)
                {
                    return true;
                }
            }
            return false;
        }

        private List<Enajenacion> FiltrarFormulariosPorPrioridad(List<Enajenacion> formularios)
        {
            List<Enajenacion> formulariosFiltrados = new List<Enajenacion>();
            foreach (Enajenacion formulario in formularios)
            {
                string numeroInscripcion = formulario.NumeroInscripcion;
                string comuna = formulario.Comuna;
                string manzana = formulario.Manzana;
                string predio = formulario.RolPredial;

                var formulariosRepetidos = db.Enajenacion
                    .Where(Data1 => Data1.Comuna == comuna)
                    .Where(Data2 => Data2.Manzana == manzana)
                    .Where(Data3 => Data3.RolPredial == predio)
                    .Where(Data4 => Data4.NumeroInscripcion == numeroInscripcion)
                    .ToList();

                if (formulariosRepetidos.Count > 1)
                {
                    formulariosRepetidos = formulariosRepetidos.OrderBy(x => x.Id).Reverse().ToList();
                    if (EsFormularioRepetido(formulariosFiltrados, formulariosRepetidos[0]) == false)
                    {
                        formulariosFiltrados.Add(formulariosRepetidos[0]);
                    }
                }
                else
                {
                    formulariosFiltrados.Add(formulario);
                }
            }
            return formulariosFiltrados;
        }

        public void RecalcularFormularios(string comuna, string manzana, string predio)
        {
            List<Enajenacion> formulariosARecalcular = ObtenerFormularios(comuna, manzana, predio);
            List<Enajenacion> formulariosARecalcularOrdenados = FiltrarFormulariosPorPrioridad(formulariosARecalcular);
            formulariosARecalcularOrdenados = formulariosARecalcularOrdenados
                .OrderBy(x => x.FechaInscripcion.Year)
                .ThenBy(x => int.Parse(x.NumeroInscripcion))
                .ToList();

            LimpiarMultipropietariosAnteriores(comuna, manzana, predio);
            ProcesarRecalculado(formulariosARecalcularOrdenados);
        }
    }
}