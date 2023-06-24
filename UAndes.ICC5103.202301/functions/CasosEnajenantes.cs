using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UAndes.ICC5103._202301.Models;

namespace UAndes.ICC5103._202301.functions
{
    public class CasosEnajenantes
    {
        private readonly FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario(new InscripcionesBrDbEntities());
        private readonly CasosEnajenantesFantasmas CasosFantasma = new CasosEnajenantesFantasmas(new InscripcionesBrDbEntities());
        private readonly FuncionesFormulario formulario = new FuncionesFormulario(new InscripcionesBrDbEntities());

        //Funciones donde se realiza la logica relacionada a los casos NORMALES de una COMPRAVENTA

        private bool CasoCienPorcientoAdquirientes(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
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
                    porcentajeEnajenantes += formulario.ObtenerPorcentajeEnajenante(enajenante[0], enajenacion);
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
                            if (porcentajeEnajenantes > 0)
                            {
                                if (funcionMultipropietario.EsMultipropietarioVigente(multipropietario.RutPropietario, enajenacion) == false)
                                {
                                    multipropietario.PorcentajeDerechoPropietario = (
                                        float.Parse(multipropietario.PorcentajeDerechoPropietario) * porcentajeEnajenantes / 100
                                        ).ToString();
                                    anadir = true;
                                    break;
                                }
                                else if (funcionMultipropietario.EsMultipropietarioVigente(multipropietario.RutPropietario, enajenacion))
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

        private bool CasoDerechos(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
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
                funcionMultipropietario.CerrarVigenciaMultipropietario(enajenacion);
                funcionMultipropietario.CrearMultipropietarios(multipropietariosProcesado);
                return true;
            }
            return false;
        }

        private void CasoDominios(List<Multipropietario> multipropietarios, List<List<string>> enajenantes, Enajenacion enajenacion)
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
                        if (funcionMultipropietario.EsMultipropietarioVigente(multipropietario.RutPropietario, enajenacion))
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
            if (CasosFantasma.BuscarEnajenantesFantasmas(enajenantes, enajenacion) == false)
            {
                funcionMultipropietario.CerrarVigenciaMultipropietario(enajenacion);
            }
            funcionMultipropietario.CrearMultipropietarios(multipropietariosProcesado);
        }

        public void CasoEnajenantes(List<Multipropietario> multipropietarios, List<List<string>> adquirientes,
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            if (CasoCienPorcientoAdquirientes(multipropietarios, adquirientes, enajenantes, enajenacion) == false)
            {
                if (CasoDerechos(multipropietarios, adquirientes, enajenantes, enajenacion) == false)
                {
                    CasoDominios(multipropietarios, enajenantes, enajenacion);
                }
            }
        }
    }
}