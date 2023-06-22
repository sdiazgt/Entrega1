using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UAndes.ICC5103._202301.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Data.SqlClient;
using System.Runtime.InteropServices.ComTypes;

namespace UAndes.ICC5103._202301.Controllers
{
    public class EnajenacionsController : Controller
    {
        private InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();

        //Refactor
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
                if ((int)multipropietario.AnoVigenciaInicial <= anoMinimo)
                {
                        multipropietario.AnoVigenciaFinal = anoMinimo;
                }
                else { multipropietario.AnoVigenciaFinal = (int)enajenacion.FechaInscripcion.Year - 1; }
            }
            db.SaveChanges();            
        }

        public (List<List<string>>, List<List<string>>) FormatearAdquirientesYEnajenantes(List<string>  adquirientes, List<string> enajenantes)
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

        private void CrearMultipropietarios(List<Multipropietario> multipropietarios)
        {
            foreach (Multipropietario multipropietario in multipropietarios)
            {
                float porcentaje = float.Parse(multipropietario.PorcentajeDerechoPropietario);
                multipropietario.PorcentajeDerechoPropietario = porcentaje.ToString("F2");
                db.Multipropietario.Add(multipropietario);
                db.SaveChanges();
            }
        }

        private List<Multipropietario> CrearObjetoMultipropietario(List<List<string>> adquirientes, Enajenacion enajenacion)
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

        private List<Multipropietario> CrearObjetoMultipropietarioVacio(List<List<string>> enajenantes, Enajenacion enajenacion)
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

        private float ObtenerPorcentajeEnajenante(string rut,Enajenacion enajenacion)
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

        private bool EsMultipropietarioVigente(string rut, Enajenacion enajenacion)
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

        //Funciones relacionadas a la COMPRAVENTA con enajenantes fantasmas
        public bool BuscarEnajenantesFantasmas(List<List<string>> enajenantes, Enajenacion enajenacion)
        {

            foreach (List<string> enajenante in enajenantes)
            {
                string rut = enajenante[0];
                var multipropietarioEnajenante = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaFinal == null)
                    .Where(Data4 => Data4.RutPropietario == rut)
                    .ToList();
                if (multipropietarioEnajenante.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public List<Multipropietario> ObtenerEnajenantesNoFantasmas(List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            List<Multipropietario> enajenanteNoFantasma = new List<Multipropietario>();
            foreach (List<string> enajenante in enajenantes)
            {
                string rut = enajenante[0];
                var multipropietarioEnajenante = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaFinal == null)
                    .Where(Data4 => Data4.RutPropietario == rut)
                    .ToList();
                if (multipropietarioEnajenante.Count >= 1)
                {
                    enajenanteNoFantasma.Add(multipropietarioEnajenante[0]);
                }
            }

            return new List<Multipropietario>(enajenanteNoFantasma);
        }

        public bool CasoCienPorcientoAdquirientesFantasma(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            float porcentajeTotalAdquirientes = 0;
            foreach (List<string> adquiriente in adquirientes)
            {
                porcentajeTotalAdquirientes += float.Parse(adquiriente[1]);
            }

            if((int)Math.Round(porcentajeTotalAdquirientes) == 100)
            {
                List<Multipropietario> enajenanteNoFantasma = ObtenerEnajenantesNoFantasmas(enajenantes, enajenacion);
                List<Multipropietario> multipropietariosProcesado = new List<Multipropietario>();

                float porcentajeNoFantasmas = 0;

                foreach (Multipropietario enajenante in enajenanteNoFantasma)
                {
                    porcentajeNoFantasmas += float.Parse(enajenante.PorcentajeDerechoPropietario);
                }

                bool anadir = false;
                foreach (Multipropietario multipropietario in multipropietarios)
                {
                    foreach (List<string> enajenante in enajenantes)
                    {
                        string rutEnajenante = enajenante[0];
                        string rut = multipropietario.RutPropietario;
                        if (rutEnajenante == rut)
                        {
                            anadir = false;
                            break;
                        }
                        else if (rutEnajenante != rut)
                        {
                            if (porcentajeNoFantasmas > 0)
                            {
                                if (EsMultipropietarioVigente(rut, enajenacion) == false)
                                {
                                    multipropietario.PorcentajeDerechoPropietario = (
                                        float.Parse(multipropietario.PorcentajeDerechoPropietario) * porcentajeNoFantasmas / 100
                                        ).ToString();
                                    anadir = true;
                                    break;
                                }
                                else if (EsMultipropietarioVigente(rut, enajenacion))
                                {
                                    anadir = true;
                                }
                            }
                            else { anadir = true; }
                        }
                        else { anadir = false; }
                    }
                    if (anadir)
                    {
                        multipropietariosProcesado.Add(multipropietario);
                        anadir = false;
                    }
                }
                CerrarVigenciaMultipropietario(enajenacion);
                CrearMultipropietarios(multipropietariosProcesado);
                return true;
            }
            else { return false; }
        }

        public bool CasoEnajenanteAdquirienteFantasma(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            int cantidad = 1;
            if (adquirientes.Count == cantidad && enajenantes.Count == cantidad)
            {
                List<Multipropietario> multipropietarioVacio = CrearObjetoMultipropietarioVacio(enajenantes, enajenacion);
                List<Multipropietario> multipropietariosParaProcesar = multipropietarios.Concat(multipropietarioVacio).ToList();
                CerrarVigenciaMultipropietario(enajenacion);
                CrearMultipropietarios(multipropietariosParaProcesar);
                return true;
            }
            return false;
        }

        public void CasoDefaultFantasma(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            List<Multipropietario> multipropietariosProcesado = new List<Multipropietario>();
            bool anadir = false;

            foreach (Multipropietario multipropietario in multipropietarios)
            {
                foreach (List<string> enajenante in enajenantes)
                {
                    string rutEnajenante = enajenante[0];
                    string rut = multipropietario.RutPropietario;
                    if (rutEnajenante == rut)
                    {
                        if (EsMultipropietarioVigente(multipropietario.RutPropietario, enajenacion))
                        {
                            multipropietario.PorcentajeDerechoPropietario = (
                                float.Parse(multipropietario.PorcentajeDerechoPropietario) - float.Parse(enajenante[1])
                                ).ToString();
                            anadir = true;
                            break;
                        }
                    }
                    else if (rutEnajenante != rut)
                    {
                        anadir = true;
                    }
                    
                }
                if (anadir)
                {
                    multipropietariosProcesado.Add(multipropietario);
                    anadir = false;
                }
            }

            foreach (List<string> enajenante in enajenantes)
            {
                if (BuscarEnajenantesFantasmas(new List<List<string>>() { enajenante }, enajenacion))
                {
                    List<List<string>> enajenanteVacio = new List<List<string>>() { enajenante };
                    List<Multipropietario> multipropetarioVacio = CrearObjetoMultipropietarioVacio(enajenanteVacio, enajenacion);
                    multipropietariosProcesado.AddRange(multipropetarioVacio);
                }
            }
            CerrarVigenciaMultipropietario(enajenacion);
            CrearMultipropietarios(multipropietariosProcesado);
        }

        public bool CasoEnajenateFantasma(List<Multipropietario> multipropietarios, List<List<string>> adquirientes, 
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            if (BuscarEnajenantesFantasmas(enajenantes, enajenacion))
            {
                if (CasoCienPorcientoAdquirientesFantasma(multipropietarios, adquirientes, enajenantes, enajenacion))
                {
                    return true;
                }
                else if (CasoEnajenanteAdquirienteFantasma(multipropietarios, adquirientes, enajenantes, enajenacion))
                {
                    return true;
                }
                else { 
                    CasoDefaultFantasma(multipropietarios, adquirientes, enajenantes, enajenacion);
                    return true;
                }
            }
            return false;
        }

        //Funciones relacionadas a los casos de COMPRAVENTA sin enajenantes fantasmas
        public bool CasoCienPorcientoAdquirientes(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            float porcentajeTotalAdquirientes = 0;
            foreach (List<string> adquiriente in adquirientes)
            {
                porcentajeTotalAdquirientes += float.Parse(adquiriente[1]);
            }

            if ((int)Math.Round(porcentajeTotalAdquirientes) == 100)
            {
                List<Multipropietario> multipropietariosProcesado = new List<Multipropietario>();

                float porcentajeEnajenantes = 0;

                foreach (List<string> enajenante in enajenantes)
                {
                    porcentajeEnajenantes += ObtenerPorcentajeEnajenante(enajenante[0], enajenacion);
                }
                bool anadir = false;
                foreach (Multipropietario multipropietario in multipropietarios)
                {
                    foreach (List<string> enajenante in enajenantes)
                    {
                        string rutEnajenante = enajenante[0];
                        string rut = multipropietario.RutPropietario;
                        if (rutEnajenante == rut)
                        {
                            anadir = false;
                            break;
                        }
                        else if(rutEnajenante != rut)
                        {
                            if (porcentajeEnajenantes > 0)
                            {
                                if (EsMultipropietarioVigente(multipropietario.RutPropietario, enajenacion) == false)
                                {
                                    multipropietario.PorcentajeDerechoPropietario = (
                                        float.Parse(multipropietario.PorcentajeDerechoPropietario) * porcentajeEnajenantes / 100
                                        ).ToString();
                                    anadir = true;
                                    break;
                                }
                                else if (EsMultipropietarioVigente(multipropietario.RutPropietario, enajenacion))
                                {
                                    anadir = true;
                                }
                            }
                            else { anadir = true; }
                        }
                        else { anadir = false; }
                    }
                    if (anadir)
                    {
                        multipropietariosProcesado.Add(multipropietario);
                        anadir = false;
                    }
                }
                CerrarVigenciaMultipropietario(enajenacion);
                CrearMultipropietarios(multipropietariosProcesado);
                return true;
            }
            else { return false; }
        }

        public bool CasoDerechos(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            List<Multipropietario> multipropietariosProcesado = new List<Multipropietario>();
            int cantidad = 1;
            if (adquirientes.Count == cantidad && enajenantes.Count == cantidad)
            {
                float porcentajeEnajenante = 0;
                foreach (Multipropietario multipropietario in multipropietarios)
                {
                    if (multipropietario.RutPropietario == enajenantes[0][0])
                    {
                        porcentajeEnajenante = float.Parse(multipropietario.PorcentajeDerechoPropietario);
                        multipropietario.PorcentajeDerechoPropietario = (
                            float.Parse(multipropietario.PorcentajeDerechoPropietario) - (
                                float.Parse(multipropietario.PorcentajeDerechoPropietario) * float.Parse(enajenantes[0][1]) / 100
                            )
                        ).ToString();
                    }
                    else if (multipropietario.RutPropietario == adquirientes[0][0])
                    {
                        multipropietario.PorcentajeDerechoPropietario = (
                            porcentajeEnajenante * float.Parse(adquirientes[0][1]) / 100
                            ).ToString();
                    }
                    multipropietariosProcesado.Add(multipropietario);
                }
                CerrarVigenciaMultipropietario(enajenacion);
                CrearMultipropietarios(multipropietariosProcesado);
                return true;
            }
            return false;
        }

        public void CasoDominios(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            //Codigo nuevo
            List<Multipropietario> multipropietariosProcesado = new List<Multipropietario>();
            bool anadir = false;

            foreach (Multipropietario multipropietario in multipropietarios)
            {
                foreach (List<string> enajenante in enajenantes)
                {
                    if (enajenante[0] != multipropietario.RutPropietario)
                    {
                        anadir = true;
                    }
                    else
                    {
                        if (EsMultipropietarioVigente(multipropietario.RutPropietario, enajenacion))
                        {
                            multipropietario.PorcentajeDerechoPropietario = (
                                float.Parse(multipropietario.PorcentajeDerechoPropietario) - float.Parse(enajenante[1])
                                ).ToString();
                            anadir = true;
                        }
                    }
                }
                if (anadir)
                {
                    multipropietariosProcesado.Add(multipropietario);
                    anadir = false;
                }
            }
            if (BuscarEnajenantesFantasmas(enajenantes, enajenacion) == false)
            {
                CerrarVigenciaMultipropietario(enajenacion);
            }
            CrearMultipropietarios(multipropietariosProcesado);
        }

        //Funciones relacionadas a las acciones Post procesamiento de datos de COMPRAVENTA
        public void RepartirAAdquirientesVacios(Enajenacion enajenacion)
        {
            int cantidadDeVacios = 0;
            string cero = "0.00";
            float porcentajeTotal = 0;

            int anoActual = enajenacion.FechaInscripcion.Year;
            var adquirientes = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                    .ToList();
            foreach (var adquiriente in adquirientes)
            {
                if (adquiriente.PorcentajeDerechoPropietario == cero)
                {
                    cantidadDeVacios++;
                }
                porcentajeTotal = porcentajeTotal + float.Parse(adquiriente.PorcentajeDerechoPropietario);
            }
            if (cantidadDeVacios > 0)
            {
                foreach (var adquiriente in adquirientes)
                {
                    if (adquiriente.PorcentajeDerechoPropietario == cero)
                    {
                        adquiriente.PorcentajeDerechoPropietario = ((100 - porcentajeTotal) / (float)cantidadDeVacios).ToString("F2");
                    }
                }
                db.SaveChanges();
            }
        }

        public void BorrarAdquirientesVacios(Enajenacion enajenacion)
        {
            string cero = "0.00";
            int anoActual = enajenacion.FechaInscripcion.Year;

            var adquirientes = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                    .ToList();
            foreach (var adquiriente in adquirientes)
            {
                if (adquiriente.PorcentajeDerechoPropietario == cero)
                {
                    db.Multipropietario.Remove(adquiriente);
                }
            }
        }

        public void MergeFormulariosExistentes(Enajenacion enajenacion)
        {
            List<List<string>> multipropietariosACombinar = new List<List<string>>();
            int anoActual = enajenacion.FechaInscripcion.Year;
            List<string> stringsAVerificar = new List<string> { "YES", "NO" };

            var multipropietarios = db.Multipropietario
            .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
            .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
            .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
            .Where(Data4 => Data4.AnoVigenciaFinal == null)
            .ToList();

            foreach (var multipropietario in multipropietarios)
            {
                List<Multipropietario> aBorrar = new List<Multipropietario>();
                string rut = multipropietario.RutPropietario;

                var multipropietariosRepetidos = db.Multipropietario
                .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                .Where(Data5 => Data5.RutPropietario == rut)
                .ToList();

                if (multipropietariosRepetidos.Count > 1)
                {
                    int numeroInscripcionMaximo = 0;
                    float sumaPorcentajes = 0;

                    foreach (var item in multipropietariosRepetidos)
                    {
                        sumaPorcentajes = sumaPorcentajes + float.Parse(item.PorcentajeDerechoPropietario);

                        if (numeroInscripcionMaximo <= int.Parse(enajenacion.NumeroInscripcion))
                        {
                            numeroInscripcionMaximo = int.Parse(enajenacion.NumeroInscripcion);
                        }
                        aBorrar.Add(item);
                    }
                    List<string> aAgregar = new List<string>() {
                            rut,
                            sumaPorcentajes.ToString("F2"),
                            stringsAVerificar[1]
                    };
                    multipropietariosACombinar.Add(aAgregar);
                    enajenacion.NumeroInscripcion = numeroInscripcionMaximo.ToString();
                }
                if (aBorrar.Count > 0)
                {
                    foreach (Multipropietario multipropietarioABorrar in aBorrar)
                    {
                        db.Multipropietario.Remove(multipropietarioABorrar);
                    }
                    db.SaveChanges();
                }
            }
            

            List<Multipropietario> multipropietarioMerge = CrearObjetoMultipropietario(multipropietariosACombinar, enajenacion);
            CrearMultipropietarios(multipropietarioMerge);
            return;
        }

        public void TransformarPorcentajeNegativo(Enajenacion enajenacion)
        {
            string cero = "0.00";
            int anoActual = enajenacion.FechaInscripcion.Year;

            var multipropietarios = db.Multipropietario
                .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                .ToList();

            if (multipropietarios.Count > 0)
            {
                foreach (var multipropietario in multipropietarios)
                {
                    float porcentajeMultipropietario = float.Parse(multipropietario.PorcentajeDerechoPropietario);
                    if (porcentajeMultipropietario < 0)
                    {
                        multipropietario.PorcentajeDerechoPropietario = cero;
                    }
                }
                db.SaveChanges();
            }
        }

        public void AjustarPorcentajeFinal(Enajenacion enajenacion)
        {
            int anoActual = enajenacion.FechaInscripcion.Year;

            var totalAdquirientesAProcesar = db.Multipropietario
               .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
               .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
               .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
               .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
               .ToList();

            float porcentajeTotal = 0;
            foreach (var adquiriente in totalAdquirientesAProcesar)
            {
                porcentajeTotal = porcentajeTotal + float.Parse(adquiriente.PorcentajeDerechoPropietario);
            }

            if ((int)Math.Round(porcentajeTotal) > 100)
            {
                float ponderacion = 100 / porcentajeTotal;
                foreach (var adquiriente in totalAdquirientesAProcesar)
                {
                    adquiriente.PorcentajeDerechoPropietario = (
                        (float.Parse(adquiriente.PorcentajeDerechoPropietario) * ponderacion).ToString("F2"));
                }
                db.SaveChanges();
                BorrarAdquirientesVacios(enajenacion);
            }
            else if ((int)Math.Round(porcentajeTotal) < 100)
            {
                RepartirAAdquirientesVacios(enajenacion);
            }
            else if ((int)Math.Round(porcentajeTotal) == 100)
            {
                BorrarAdquirientesVacios(enajenacion);
            }
        }

        //Funciones relacionadas a Herencia
        public bool VerificarRegistrosAnteriores(Enajenacion enajenacion)
        {
            int anoActual = enajenacion.FechaInscripcion.Year;
            var adquirientePorAno= db.Multipropietario
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

        //Funciones relacionadas al manejo de paginas
        // GET: Enajenacions
        public ActionResult Index()
        {
            return View(db.Enajenacion.ToList());
        }

        // GET: Enajenacions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Enajenacion enajenacion = db.Enajenacion.Find(id);

            if (enajenacion == null)
            {
                return HttpNotFound();
            }

            return View(enajenacion);
        }

        // GET: Enajenacions/Create
        public ActionResult Create()
        {

            var comuna = new Comuna();

            List<string> comunas = comuna.ListaDeComunas();

            ViewBag.comunas = comunas;

            return View();
        }

        // POST: Enajenacions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CNE,Comuna,Manzana,RolPredial,Enajenantes,Adquirientes,Foja,FechaInscripcion,NumeroInscripcion")] Enajenacion enajenacion)
        {
            var comuna = new Comuna();
            List<string> comunas = comuna.ListaDeComunas();
            ViewBag.comunas = comunas;

            //  En el caso para la lista de Enajenantes y Adquirientes se debe agregar el valor "NO", esto ocurre cuando
            //  en el checklist no se agrega el valor "YES", por lo cual se verifica si se agrega o no.
            string keyEnajenate = "inputEnajentes[]";
            string keyAdquiriente = "inputAdquirientes[]";
            List<string> listaDeEnajenates = Request.Form.GetValues(keyEnajenate)?.ToList();
            List<string> listaDeAdquirientes = Request.Form.GetValues(keyAdquiriente)?.ToList();

            //Se le asigna un numero a estas variables para poder llamarlas descriptivamente
            string compraVenta = "1";
            string regularizacionDePatrimonio = "2";

            var datosFormateados = FormatearAdquirientesYEnajenantes(listaDeAdquirientes, listaDeEnajenates);
            List<List<string>> listaAdquirientesFormateada = datosFormateados.Item1;
            List<List<string>> listaEnajenantesFormateada = datosFormateados.Item2;


            if (enajenacion.CNE == compraVenta)
            {
                List<Multipropietario> multipropietariosProcesados = FormularioAProcesar(listaAdquirientesFormateada, enajenacion);
                Debug.WriteLine("1");
                Debug.WriteLine(multipropietariosProcesados);
                List<Multipropietario> multipropietarios = CambiarFechaInicialMultipropietario(multipropietariosProcesados, enajenacion);
                Debug.WriteLine("2");
                Debug.WriteLine(multipropietarios);

                if (CasoEnajenateFantasma(multipropietarios, listaAdquirientesFormateada, listaEnajenantesFormateada, enajenacion) == false)
                {
                    if (CasoCienPorcientoAdquirientes(multipropietarios, listaAdquirientesFormateada, listaEnajenantesFormateada, enajenacion) == false)
                    {
                        if (CasoDerechos(multipropietarios, listaAdquirientesFormateada, listaEnajenantesFormateada, enajenacion) == false)
                        {
                            CasoDominios(multipropietarios, listaAdquirientesFormateada, listaEnajenantesFormateada, enajenacion);
                        }
                    }
                }
                MergeFormulariosExistentes(enajenacion);
                TransformarPorcentajeNegativo(enajenacion);
                AjustarPorcentajeFinal(enajenacion);
            }
            //Validacion de porcentajes para adquirientes en caso que CNE sea "Regularización de Patrimonio" o "Herencia".
            else if (enajenacion.CNE == regularizacionDePatrimonio)
            {
                var adquirientesPorAcreditacion = ObtenerAdquirientesPorAcreditacion(listaAdquirientesFormateada);
                List<List<string>> adquirientesAcredidatos = adquirientesPorAcreditacion.Item1;
                List<List<string>> adquirientesNoAcredidatos = adquirientesPorAcreditacion.Item2;
                
                if (VerificarDatosFormularioHerencia(listaAdquirientesFormateada) == false)
                {
                    ModelState.AddModelError("Adquirientes", "Hubo un error en el calculo de los adquirientes, ingresar una suma o valores validos");
                    return View(enajenacion);
                }

                listaAdquirientesFormateada = ProcesarAdquirientesPorAcreditacion(adquirientesAcredidatos, adquirientesNoAcredidatos);

                if (VerificarRegistrosAnteriores(enajenacion) == false)
                {
                    ModelState.AddModelError("numeroInscripcion", "El numero de inscripcion no prevalece para el año ingresado");
                    return View(enajenacion);
                }

                

                //Creacion de objeto Multipropetiario
                List<Multipropietario> multipropietariosACrear = CrearObjetoMultipropietario(listaAdquirientesFormateada, enajenacion);
                CerrarVigenciaMultipropietario(enajenacion);
                CrearMultipropietarios(multipropietariosACrear);
            }

            var jsonEnajente = JsonConvert.SerializeObject(listaEnajenantesFormateada);
            var jsonAdquiriente = JsonConvert.SerializeObject(listaAdquirientesFormateada);

            enajenacion.Enajenantes = jsonEnajente;
            enajenacion.Adquirientes = jsonAdquiriente;

            if (ModelState.IsValid)
            {
                db.Enajenacion.Add(enajenacion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(enajenacion);
        }

        // GET: Enajenacions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enajenacion enajenacion = db.Enajenacion.Find(id);
            if (enajenacion == null)
            {
                return HttpNotFound();
            }
            return View(enajenacion);
        }

        // POST: Enajenacions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CNE,Comuna,Manzana,RolPredial,Enajenantes,Adquirientes,Foja,FechaInscripcion,NumeroInscripcion")] Enajenacion enajenacion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(enajenacion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(enajenacion);
        }

        // GET: Enajenacions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enajenacion enajenacion = db.Enajenacion.Find(id);
            if (enajenacion == null)
            {
                return HttpNotFound();
            }
            return View(enajenacion);
        }

        // POST: Enajenacions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Enajenacion enajenacion = db.Enajenacion.Find(id);
            db.Enajenacion.Remove(enajenacion);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
