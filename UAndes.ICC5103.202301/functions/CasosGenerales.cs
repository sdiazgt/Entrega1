using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UAndes.ICC5103._202301.Models;

namespace UAndes.ICC5103._202301.functions
{
    public class CasosGenerales
    {
        private InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();
        private FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario();

        private void RepartirAAdquirientesVacios(Enajenacion enajenacion)
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
                porcentajeTotal += float.Parse(adquiriente.PorcentajeDerechoPropietario);
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

        private void BorrarAdquirientesVacios(Enajenacion enajenacion)
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

        private void MergeFormulariosExistentes(Enajenacion enajenacion)
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
                        sumaPorcentajes += float.Parse(item.PorcentajeDerechoPropietario);

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


            List<Multipropietario> multipropietarioMerge = funcionMultipropietario.CrearObjetoMultipropietario(multipropietariosACombinar, enajenacion);
            funcionMultipropietario.CrearMultipropietarios(multipropietarioMerge);
            return;
        }

        private void TransformarPorcentajeNegativo(Enajenacion enajenacion)
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

        private void AjustarPorcentajeFinal(Enajenacion enajenacion)
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
                porcentajeTotal += float.Parse(adquiriente.PorcentajeDerechoPropietario);
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

        public void CasosPostProcesamiento(Enajenacion enajenacion)
        {
            MergeFormulariosExistentes(enajenacion);
            TransformarPorcentajeNegativo(enajenacion);
            AjustarPorcentajeFinal(enajenacion);
        }
    }
}