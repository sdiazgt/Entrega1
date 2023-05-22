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

namespace UAndes.ICC5103._202301.Controllers
{
    public class EnajenacionsController : Controller
    {
        private InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();

        public int AnoMaximoVigenciaFinalMultipropietario()
        {
            List<Multipropietario> multipropietarios = db.Multipropietario.ToList();

            int anoMaximo = 0;

            foreach (Multipropietario multipropietario in multipropietarios)
            {
                if (multipropietario.AnoVigenciaInicial != null && multipropietario.AnoVigenciaInicial > anoMaximo)
                {
                    anoMaximo = multipropietario.AnoVigenciaInicial;
                }
            }
            return anoMaximo;
        }

        public List<List<string>> AdquirientesNoAcreditados(List<List<string>> Adquirientes)
        {
            List<string> datosAVerificar = new List<string> { "YES", "NO" };
            string cero = "0.00";

            foreach (List<string> adquiriente in Adquirientes) 
            {
                if ( adquiriente[2] == datosAVerificar[0])
                {
                    adquiriente[1] = cero;
                }
            }
            return Adquirientes;
        }

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

        public void VaciarPorcentajeEnajenante(List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            string cero = "0.00";
            foreach (List<string> enajenante in enajenantes)
            {
                int anoActual = enajenacion.FechaInscripcion.Year;
                string rut = enajenante[0];

                var adquirientes = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                    .Where(Data5 => Data5.RutPropietario == rut)
                    .ToList();

                if (adquirientes.Count > 0)
                {
                    adquirientes[0].PorcentajeDerechoPropietario = cero;
                    db.SaveChanges();
                }
            }
        }

        public void CambiarVigenciaAdquiriente(Enajenacion enajenacion)
        {
            int anoMinimo = 2019;
            int anoActual = enajenacion.FechaInscripcion.Year;

            var adquirientesACambiarVigencia = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaFinal == null && Data4.AnoVigenciaInicial < anoActual)
                    .ToList();

            if (adquirientesACambiarVigencia.Count > 0)
            {
                foreach (var adquiriente in adquirientesACambiarVigencia)
                {
                    if ((int)adquiriente.AnoVigenciaInicial <= anoMinimo)
                    {
                        adquiriente.AnoVigenciaFinal = anoMinimo;
                    }
                    else { adquiriente.AnoVigenciaFinal = (int)enajenacion.FechaInscripcion.Year - 1; }
                    db.SaveChanges();
                }
            }
        }

        public void CrearMultipropietario(List<List<string>> listaAdquirientes, Enajenacion enajenacion)
        {
            CambiarVigenciaAdquiriente(enajenacion);
            int anoMinimo = 2019;
            foreach (List<string> adquiriente in listaAdquirientes)
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
                int fechaVigenciaFinal;
                if ((int)Fecha.Year <= anoMinimo)
                {
                    multipropietario.AnoVigenciaInicial = anoMinimo;
                    fechaVigenciaFinal = anoMinimo;
                }
                else
                {
                    multipropietario.AnoVigenciaInicial = Fecha.Year;
                    fechaVigenciaFinal = Fecha.Year;
                }

                if (AnoMaximoVigenciaFinalMultipropietario() > fechaVigenciaFinal)
                {
                    multipropietario.AnoVigenciaFinal = fechaVigenciaFinal;
                }

                db.Multipropietario.Add(multipropietario);
                db.SaveChanges();
            }
        }

        public bool ReparticionPorSumaDeAdquirientes(List<List<string>> listaAdquirientesFormateada, 
            List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            float porcentajeTotalAdquirientes = 0;
            float porcentajeTotalEnajenantes = 0;

            foreach (List<string> enajenante in enajenantes)
            {
                int anoActual = enajenacion.FechaInscripcion.Year;
                string rut = enajenante[0];

                var datosEnajenante = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                    .Where(Data5 => Data5.RutPropietario == rut)
                    .ToList();
                if (datosEnajenante.Count > 0)
                {
                    porcentajeTotalEnajenantes = porcentajeTotalEnajenantes + float.Parse(datosEnajenante[0].PorcentajeDerechoPropietario);
                }
            }

            foreach (List<string> adquiriente in listaAdquirientesFormateada)
            {
                porcentajeTotalAdquirientes = porcentajeTotalAdquirientes + float.Parse(adquiriente[1]);
            }
            if ((int)Math.Round(porcentajeTotalAdquirientes) > 100)
            {
                return false;
            }
            else if ((int)Math.Round(porcentajeTotalAdquirientes) == 100)
            {
                foreach (List<string> adquiriente in listaAdquirientesFormateada)
                {
                    adquiriente[1] = ((float)(float.Parse(adquiriente[1]) * porcentajeTotalEnajenantes) / 100).ToString("F2");  
                }
                VaciarPorcentajeEnajenante(enajenantes, enajenacion);
                CrearMultipropietario(listaAdquirientesFormateada, enajenacion);
                return true;
            }           
            
            return false;
        }

        public bool ReparticionPorDerechos(List<List<string>> adquirientes, List<List<string>> enajenantes, Enajenacion enajenacion)
        {

            if (adquirientes.Count > 1 || enajenantes.Count > 1)
            {
                return false;
            }
            else if (adquirientes.Count == 1 && enajenantes.Count == 1)
            {
                //float PorcentajeAAdquirir = float.Parse(adquirientes[0][1]);
                float porcentajeEnajenando = float.Parse(enajenantes[0][1]);
                int anoActual = enajenacion.FechaInscripcion.Year;
                string rut = enajenantes[0][0];

                var multipropietario = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                    .Where(Data5 => Data5.RutPropietario == rut)
                    .ToList();

                if (multipropietario.Count > 0)
                {
                    float porcentajeEnajenante = float.Parse(multipropietario[0].PorcentajeDerechoPropietario);

                    float porcentajeRestante = (porcentajeEnajenando * porcentajeEnajenante) / 100;

                    multipropietario[0].PorcentajeDerechoPropietario = (porcentajeEnajenante - porcentajeRestante).ToString("F2");
                    adquirientes[0][1] = porcentajeRestante.ToString("F2");

                    db.SaveChanges();
                    CrearMultipropietario(adquirientes, enajenacion);

                    return true;
                }
            }
            return false;

        }

        public bool ReparticionPorDominios(List<List<string>> adquirientes, List<List<string>> enajenantes, Enajenacion enajenacion)
        {

            foreach (List<string> enajenante in enajenantes)
            {
                int anoActual = enajenacion.FechaInscripcion.Year;
                string rut = enajenante[0];

                var datosEnajenante = db.Multipropietario
                .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                .Where(Data5 => Data5.RutPropietario == rut)
                .ToList();

                if (datosEnajenante.Count > 0)
                {
                    float valorFinalPorcentaje = float.Parse(datosEnajenante[0].PorcentajeDerechoPropietario) - float.Parse(enajenante[1]);
                    datosEnajenante[0].PorcentajeDerechoPropietario = valorFinalPorcentaje.ToString("F2");
                }
                else
                {
                    return false;
                }

                db.SaveChanges();
            }
            CrearMultipropietario(adquirientes, enajenacion);
            return true;
        }

        public Enajenacion UnirFormulariosExistentes(Enajenacion enajenacion)
        {
            List<string> stringsAVerificar = new List<string> { "YES", "NO" };
            List<List<string>> aCombinar = new List<List<string>>();

            int anoActual = enajenacion.FechaInscripcion.Year;
            var multipropietarios = db.Multipropietario
            .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
            .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
            .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
            .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
            .ToList();

            foreach (var multipropietario in multipropietarios)
            {
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
                        db.Multipropietario.Remove(item);
                    }
                    List<string> aAgregar = new List<string>() { 
                            rut,
                            sumaPorcentajes.ToString("F2"),
                            stringsAVerificar[0] 
                    };

                    db.SaveChanges();
                    aCombinar.Add(aAgregar);
                    enajenacion.NumeroInscripcion = numeroInscripcionMaximo.ToString();
                }
            }
            CrearMultipropietario(aCombinar, enajenacion);
            return enajenacion;
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

            //Se le asigna un numero a estas variables para poder llamarlas descriptivamente
            string compraVenta = "1";
            string regularizacionDePatrimonio = "2";

            List<string> listaDeEnajenates = Request.Form.GetValues(keyEnajenate)?.ToList();
            List<string> listaDeAdquirientes = Request.Form.GetValues(keyAdquiriente)?.ToList();

            double valorComparacion1;
            double valorComparacion2;

            int anoMinimo = 2019;

            List<string> stringsAVerificar = new List<string> { "YES", "NO" };

            for (int i = 0; i < listaDeEnajenates.Count; i++)
            {
                valorComparacion1 = (i + 1) % 3;

                if ((int)valorComparacion1 == 0 && i != 0)
                {
                    if (listaDeEnajenates[i] != stringsAVerificar[0] && listaDeEnajenates[i] != stringsAVerificar[1])
                    {
                        listaDeEnajenates.Insert(i, stringsAVerificar[1]);
                    }
                }
            }

            for (int i = 0; i < listaDeAdquirientes.Count; i++)
            {
                valorComparacion2 = (i + 1) % 3;
                if ((int)valorComparacion2 == 0 && i != 0)
                {
                    if (listaDeAdquirientes[i] != stringsAVerificar[0] && listaDeAdquirientes[i] != stringsAVerificar[1])
                    {
                        listaDeAdquirientes.Insert(i, stringsAVerificar[1]);
                    }
                }
            }

            if ((listaDeEnajenates.Count % 3) != 0)
            {
                listaDeEnajenates.Insert(listaDeEnajenates.Count, stringsAVerificar[1]);
            }
            if ((listaDeAdquirientes.Count % 3) != 0)
            {
                listaDeAdquirientes.Insert(listaDeAdquirientes.Count, stringsAVerificar[1]);
            }



            List<List<string>> listaEnajenantesFormateada = new List<List<string>>();
            List<List<string>> listaAdquirientesFormateada = new List<List<string>>();
            List<string> listaTemporal = new List<string>();

            foreach (string value in listaDeEnajenates)
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
            foreach (string value in listaDeAdquirientes)
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

            if (enajenacion.CNE == compraVenta)
            {
                List<List<string>> adquirientesAProcesar = AdquirientesNoAcreditados(listaAdquirientesFormateada);

                if (ReparticionPorSumaDeAdquirientes(adquirientesAProcesar, listaEnajenantesFormateada, enajenacion)) ;
                else if (ReparticionPorDerechos(adquirientesAProcesar, listaEnajenantesFormateada, enajenacion)) ;
                else if (ReparticionPorDominios(adquirientesAProcesar, listaEnajenantesFormateada, enajenacion)) ;

                enajenacion = UnirFormulariosExistentes(enajenacion);
                TransformarPorcentajeNegativo(enajenacion);
                AjustarPorcentajeFinal(enajenacion);

            }
            //Validacion de porcentajes para adquirientes en caso que CNE sea "Regularización de Patrimonio" o "Herencia".
            else if (enajenacion.CNE == regularizacionDePatrimonio)
            {
                int porcentajeTotal = 0;

                List<List<string>> adquirientesNoAcredidatos = new List<List<string>>();
                List<List<string>> adquirientesAcredidatos = new List<List<string>>();

                foreach (List<string> dataAdquriente in listaAdquirientesFormateada)
                {
                    if (dataAdquriente[2] == stringsAVerificar[0])
                    {
                        adquirientesNoAcredidatos.Add(dataAdquriente);
                        continue;
                    }
                    else
                    {
                        if (dataAdquriente[1].All(char.IsDigit) && dataAdquriente[1] != "")
                        {
                            porcentajeTotal += int.Parse(dataAdquriente[1]);
                            adquirientesAcredidatos.Add(dataAdquriente);
                        }
                        else
                        {
                            ModelState.AddModelError("Adquirientes", "Ingrese un valor % valido para los Adquirientes");
                            View(enajenacion);
                        }
                    }
                }
                if (porcentajeTotal > 100)
                {
                    ModelState.AddModelError("Adquirientes", "Ingrese una suma de % valido para los Adquirientes");

                    View(enajenacion);
                }
                if (adquirientesNoAcredidatos.Count > 0)
                {
                    float porcentajeRestante = 100 - (float)porcentajeTotal;
                    float reparticionPorcentaje = (float)porcentajeRestante / (float)adquirientesNoAcredidatos.Count;
                    foreach (List<string> dataAdquriente in adquirientesNoAcredidatos)
                    {
                        dataAdquriente[1] = reparticionPorcentaje.ToString();
                    }
                }
                else if (adquirientesNoAcredidatos.Count == 0 && porcentajeTotal < 100)
                {
                    ModelState.AddModelError("Adquirientes", "Ingrese una suma de % valido para los Adquirientes");

                    View(enajenacion);
                }

                adquirientesAcredidatos.AddRange(adquirientesNoAcredidatos);
                listaAdquirientesFormateada = adquirientesAcredidatos.ToList();

                int anoActual = enajenacion.FechaInscripcion.Year;

                var adquirientesARemplazar = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaInicial == anoActual)
                    .ToList();

                if (adquirientesARemplazar.Count > 0)
                {
                    foreach (var adquiriente in adquirientesARemplazar)
                    {
                        if ( int.Parse(adquiriente.NumeroInscripcion) <= int.Parse(enajenacion.NumeroInscripcion))
                        {
                            db.Multipropietario.Remove(adquiriente);
                            db.SaveChanges();
                        }
                    }
                }


                var adquirientesACambiarVigencia = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaFinal == null && Data4.AnoVigenciaInicial < anoActual)
                    .ToList();

                if (adquirientesACambiarVigencia.Count > 0)
                {
                    foreach (var adquiriente in adquirientesACambiarVigencia)
                    {
                        if ((int)adquiriente.AnoVigenciaInicial <= anoMinimo)
                        {
                            adquiriente.AnoVigenciaFinal = anoMinimo;
                        }
                        else { adquiriente.AnoVigenciaFinal = (int)enajenacion.FechaInscripcion.Year - 1; }
                        db.SaveChanges();
                    }
                }
                
                var adquirientesConFechaMenor = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaFinal == null && Data4.AnoVigenciaInicial < anoActual)
                    .ToList();

                //Creacion de objeto Multipropetiario
                CrearMultipropietario(listaAdquirientesFormateada, enajenacion);
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
