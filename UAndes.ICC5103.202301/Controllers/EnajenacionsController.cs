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

            //Validacion de porcentajes para adquirientes en caso que CNE sea "Regularización de Patrimonio" o "Herencia".
            if (enajenacion.CNE == regularizacionDePatrimonio)
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

                var adquirientesARemplazar = db.Multipropietario
                    .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                    .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                    .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                    .Where(Data4 => Data4.AnoVigenciaInicial == enajenacion.FechaInscripcion.Year)
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
                    .Where(Data4 => Data4.AnoVigenciaFinal == null && Data4.AnoVigenciaInicial < enajenacion.FechaInscripcion.Year)
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
                    .Where(Data4 => Data4.AnoVigenciaFinal == null && Data4.AnoVigenciaInicial < enajenacion.FechaInscripcion.Year)
                    .ToList();

                //Creacion de objeto Multipropetiario

                foreach (List<string> adquiriente in listaAdquirientesFormateada)
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
