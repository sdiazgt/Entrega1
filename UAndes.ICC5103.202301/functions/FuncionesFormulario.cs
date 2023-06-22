using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UAndes.ICC5103._202301.Models;

namespace UAndes.ICC5103._202301.functions
{
    public class FuncionesFormulario
    {
        private InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();

        public (List<List<string>>, List<List<string>>) FormatearAdquirientesYEnajenantes(List<string> adquirientes, List<string> enajenantes)
        {
            double valorComparacion1;
            double valorComparacion2;

            List<string> stringsAVerificar = new List<string> { "YES", "NO" };

            for (int i = 0; i < enajenantes.Count; i++)
            {
                valorComparacion1 = (i + 1) % 3;

                if ((int)valorComparacion1 == 0 && i != 0)
                {
                    if (enajenantes[i] != stringsAVerificar[0] && enajenantes[i] != stringsAVerificar[1])
                    {
                        enajenantes.Insert(i, stringsAVerificar[1]);
                    }
                }
            }

            for (int i = 0; i < adquirientes.Count; i++)
            {
                valorComparacion2 = (i + 1) % 3;
                if ((int)valorComparacion2 == 0 && i != 0)
                {
                    if (adquirientes[i] != stringsAVerificar[0] && adquirientes[i] != stringsAVerificar[1])
                    {
                        adquirientes.Insert(i, stringsAVerificar[1]);
                    }
                }
            }

            if ((enajenantes.Count % 3) != 0)
            {
                enajenantes.Insert(enajenantes.Count, stringsAVerificar[1]);
            }
            if ((adquirientes.Count % 3) != 0)
            {
                adquirientes.Insert(adquirientes.Count, stringsAVerificar[1]);
            }

            List<List<string>> listaEnajenantesFormateada = new List<List<string>>();
            List<List<string>> listaAdquirientesFormateada = new List<List<string>>();
            List<string> listaTemporal = new List<string>();

            foreach (string value in enajenantes)
            {
                if (value == stringsAVerificar[1] || value == stringsAVerificar[0])
                {
                    listaTemporal.Add(value);
                    listaEnajenantesFormateada.Add(listaTemporal.ToList());
                    listaTemporal.Clear();
                }
                else
                {
                    listaTemporal.Add(value);
                }
            }

            listaTemporal.Clear();
            foreach (string value in adquirientes)
            {
                if (value == stringsAVerificar[1] || value == stringsAVerificar[0])
                {
                    listaTemporal.Add(value);
                    listaAdquirientesFormateada.Add(listaTemporal.ToList());
                    listaTemporal.Clear();
                }
                else
                {
                    listaTemporal.Add(value);
                }
            }
            return (listaAdquirientesFormateada, listaEnajenantesFormateada);
        }

        public float ObtenerPorcentajeEnajenante(string rut, Enajenacion enajenacion)
        {
            var multipropietarios = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaFinal == null)
                    .Where(Data5 => Data5.RutPropietario == rut)
                    .ToList();

            return float.Parse(multipropietarios[0].PorcentajeDerechoPropietario);
        }

        public (List<List<string>>, List<List<string>>) ObtenerAdquirientesPorAcreditacion(List<List<string>> adquirientes)
        {
            List<string> stringsAVerificar = new List<string> { "YES", "NO" };
            List<List<string>> adquirientesNoAcredidatos = new List<List<string>>();
            List<List<string>> adquirientesAcredidatos = new List<List<string>>();

            foreach (List<string> adquiriente in adquirientes)
            {
                if (adquiriente[2] == stringsAVerificar[0])
                {
                    adquirientesNoAcredidatos.Add(adquiriente);
                    continue;
                }
                else
                {
                    adquirientesAcredidatos.Add(adquiriente);
                }
            }
            return (adquirientesAcredidatos, adquirientesNoAcredidatos);
        }

        public List<List<string>> ProcesarAdquirientesPorAcreditacion(List<List<string>> adquirientesAcreditados, List<List<string>> adquirientesNoAcreditados)
        {
            float porcentajeTotalAcreditado = 0;

            foreach (List<string> adquiriente in adquirientesAcreditados)
            {
                porcentajeTotalAcreditado += float.Parse(adquiriente[1]);
            }

            if (adquirientesNoAcreditados.Count > 0)
            {
                float porcentajeRestante = 100 - porcentajeTotalAcreditado;
                float reparticionPorcentaje = porcentajeRestante / (float)adquirientesNoAcreditados.Count;
                foreach (List<string> dataAdquriente in adquirientesNoAcreditados)
                {
                    dataAdquriente[1] = reparticionPorcentaje.ToString("F2");
                }
            }

            adquirientesAcreditados.AddRange(adquirientesNoAcreditados);
            return adquirientesAcreditados;
        }
        
        public bool VerificarRegistrosAnteriores(Enajenacion enajenacion)
        {
            int anoActual = enajenacion.FechaInscripcion.Year;
            var adquirientePorAno = db.Multipropietario
                .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                .ToList();

            if (adquirientePorAno.Count > 0)
            {
                if (int.Parse(adquirientePorAno[0].NumeroInscripcion) <= int.Parse(enajenacion.NumeroInscripcion))
                {
                    foreach (var adquiriente in adquirientePorAno)
                    {
                        db.Multipropietario.Remove(adquiriente);
                        db.SaveChanges();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public bool VerificarDatosFormularioHerencia(List<List<string>> adquirientes)
        {
            var acreditacionAdquirientes = ObtenerAdquirientesPorAcreditacion(adquirientes);
            List<List<string>> adquirientesNoAcredidatos = acreditacionAdquirientes.Item2;

            float porcentajeAdquirientes = 0;
            foreach (List<string> adquiriente in adquirientes)
            {
                if (adquiriente[1].All(char.IsDigit) == false || adquiriente[1] == "" || adquiriente[1] == null)
                {
                    return false;
                }
                porcentajeAdquirientes += float.Parse(adquiriente[1]);
            }
            if ((int)Math.Round(porcentajeAdquirientes) > 100)
            {
                return false;
            }
            if ((int)Math.Round(porcentajeAdquirientes) < 100 && adquirientesNoAcredidatos.Count == 0)
            {
                return false;
            }
            return true;
        }
    }
}