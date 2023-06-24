using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UAndes.ICC5103._202301.Models;

namespace UAndes.ICC5103._202301.functions
{
    public class CasosEnajenantesFantasmas
    {
        private readonly InscripcionesBrDbEntities db;
        private readonly FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario(new InscripcionesBrDbEntities());

        public CasosEnajenantesFantasmas(InscripcionesBrDbEntities DB)
        {
            db = DB;
        }

        //Funciones donde se realiza la logica relacionada a los casos FANTASMAS de una COMPRAVENTA

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

        private List<Multipropietario> ObtenerEnajenantesNoFantasmas(List<List<string>> enajenantes, Enajenacion enajenacion)
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

        private bool CasoCienPorcientoAdquirientesFantasma(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            float porcentajeTotalAdquirientes = 0;
            foreach (List<string> adquiriente in adquirientes)
            {
                porcentajeTotalAdquirientes += float.Parse(adquiriente[1]);
            }

            if ((int)Math.Round(porcentajeTotalAdquirientes) == 100)
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
                                if (funcionMultipropietario.EsMultipropietarioVigente(rut, enajenacion) == false)
                                {
                                    multipropietario.PorcentajeDerechoPropietario = (
                                        float.Parse(multipropietario.PorcentajeDerechoPropietario) * porcentajeNoFantasmas / 100
                                        ).ToString();
                                    anadir = true;
                                    break;
                                }
                                else if (funcionMultipropietario.EsMultipropietarioVigente(rut, enajenacion))
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
                funcionMultipropietario.CerrarVigenciaMultipropietario(enajenacion);
                funcionMultipropietario.CrearMultipropietarios(multipropietariosProcesado);
                return true;
            }
            else { return false; }
        }

        private bool CasoEnajenanteAdquirienteFantasma(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            int cantidad = 1;
            if (adquirientes.Count == cantidad && enajenantes.Count == cantidad)
            {
                List<Multipropietario> multipropietarioVacio = funcionMultipropietario.CrearObjetoMultipropietarioVacio(enajenantes, enajenacion);
                List<Multipropietario> multipropietariosParaProcesar = multipropietarios.Concat(multipropietarioVacio).ToList();
                funcionMultipropietario.CerrarVigenciaMultipropietario(enajenacion);
                funcionMultipropietario.CrearMultipropietarios(multipropietariosParaProcesar);
                return true;
            }
            return false;
        }

        private void CasoDefaultFantasma(List<Multipropietario> multipropietarios, List<List<string>> enajenantes, Enajenacion enajenacion)
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
                        if (funcionMultipropietario.EsMultipropietarioVigente(multipropietario.RutPropietario, enajenacion))
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
                    List<Multipropietario> multipropetarioVacio = funcionMultipropietario.CrearObjetoMultipropietarioVacio(enajenanteVacio, enajenacion);
                    multipropietariosProcesado.AddRange(multipropetarioVacio);
                }
            }
            funcionMultipropietario.CerrarVigenciaMultipropietario(enajenacion);
            funcionMultipropietario.CrearMultipropietarios(multipropietariosProcesado);
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
                else
                {
                    CasoDefaultFantasma(multipropietarios, enajenantes, enajenacion);
                    return true;
                }
            }
            return false;
        }

    }
}