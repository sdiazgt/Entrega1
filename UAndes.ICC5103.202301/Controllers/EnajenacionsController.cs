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

using UAndes.ICC5103._202301.functions;

namespace UAndes.ICC5103._202301.Controllers
{
    public class EnajenacionsController : Controller
    {
        private InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();

        private FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario();
        private CasosEnajenantesFantasmas CasosFantasma = new CasosEnajenantesFantasmas();
        private CasosEnajenantes CasosNoFantasma= new CasosEnajenantes();
        private CasosGenerales CasosGenerales = new CasosGenerales();
        private FuncionesFormulario formulario = new FuncionesFormulario();

        private bool ProcesarCNE(List<List<string>> adquirientes, List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            string compraVenta = "1";
            string regularizacionDePatrimonio = "2";

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

                if (formulario.VerificarDatosFormularioHerencia(adquirientes) == false)
                {
                    ModelState.AddModelError("Adquirientes", "Hubo un error en el calculo de los adquirientes, ingresar una suma o valores validos");
                    return false;
                }

                adquirientes = formulario.ProcesarAdquirientesPorAcreditacion(adquirientesAcredidatos, adquirientesNoAcredidatos);

                if (formulario.VerificarRegistrosAnteriores(enajenacion) == false)
                {
                    ModelState.AddModelError("numeroInscripcion", "El numero de inscripcion no prevalece para el año ingresado");
                    return false;
                }

                List<Multipropietario> multipropietariosACrear = funcionMultipropietario.CrearObjetoMultipropietario(adquirientes, enajenacion);
                funcionMultipropietario.CerrarVigenciaMultipropietario(enajenacion);
                funcionMultipropietario.CrearMultipropietarios(multipropietariosACrear);
            }
            return true;
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

            var datosFormateados = formulario.FormatearAdquirientesYEnajenantes(listaDeAdquirientes, listaDeEnajenates);
            List<List<string>> listaAdquirientesFormateada = datosFormateados.Item1;
            List<List<string>> listaEnajenantesFormateada = datosFormateados.Item2;

            if (ProcesarCNE(listaAdquirientesFormateada, listaEnajenantesFormateada, enajenacion) == false)
            {
                return View(enajenacion);
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
