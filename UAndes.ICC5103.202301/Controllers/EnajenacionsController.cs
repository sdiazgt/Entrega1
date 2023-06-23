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
        private readonly InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();

        private readonly FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario();
        private readonly CasosEnajenantesFantasmas CasosFantasma = new CasosEnajenantesFantasmas();
        private readonly CasosEnajenantes CasosNoFantasma= new CasosEnajenantes();
        private readonly CasosGenerales CasosGenerales = new CasosGenerales();
        private readonly FuncionesFormulario formulario = new FuncionesFormulario();
        private readonly RecalcularFormulario recalcular = new RecalcularFormulario();

        private bool VerificarNumeroInscripcionMenor(Enajenacion enajenacion, List<List<string>> adquirientes)
        {
            foreach (List<string> adquiriente in adquirientes)
            {
                string rut = adquiriente[0];
                var multipropietarios = db.Multipropietario
                .Where(Data1 => Data1.Comuna == enajenacion.Comuna)
                .Where(Data2 => Data2.Manzana == enajenacion.Manzana)
                .Where(Data3 => Data3.RolPredial == enajenacion.RolPredial)
                .Where(Data4 => Data4.AnoVigenciaFinal == null)
                .Where(Data5 => Data5.RutPropietario == rut)
                .ToList();

                if (multipropietarios.Count > 0)
                {
                    foreach (Multipropietario multipropietario in multipropietarios)
                    {
                        string rutMultipropietario = multipropietario.RutPropietario;
                        int inscripcion = int.Parse(multipropietario.NumeroInscripcion);
                        int inscripcionFormulario = int.Parse(enajenacion.NumeroInscripcion);

                        if (rut == rutMultipropietario && inscripcionFormulario < inscripcion)
                        {
                            ModelState.AddModelError("numeroInscripcion", "El numero de inscripcion no prevalece para el año ingresado");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool VerificarNumeroInscripcion(Enajenacion enajenacion)
        {
            string numeroInscripcion = enajenacion.NumeroInscripcion;
            if (numeroInscripcion.All(char.IsDigit) == false)
            {
                ModelState.AddModelError("numeroInscripcion", "El numero de inscripcion debe ser un numero");
                return false;
            }
            return true;
        }

        private bool VerificarFormatoRut(string rut)
        {
            int posicionGuion = 2;
            char guion = rut[rut.Length - posicionGuion];
            char valorGuion = '-';

            if (guion != valorGuion)
            {
                return false;
            }
            else
            {
                // Se cambian
                int posicionK = 1;
                char k = rut[rut.Length - posicionK];
                char valorK = 'k';
                char valorKMayuscula = 'K';
                string rutSinK = rut;
                if ( k == valorK)
                {
                    rutSinK = rut.Replace('k', '0');
                }
                else if (k == valorKMayuscula)
                {
                    rutSinK = rut.Replace('K', '0');
                }

                string rutSinGuion = rutSinK.Replace('-', '0');

                if(rutSinGuion.All(char.IsDigit) == false)
                {
                    return false;
                }
            }
            return true;
        }

        private bool VerificarRut(List<List<string>> adquirientes, List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            string regularizacionDePatrimonio = "2";
            int largoMaximo = 10;
            foreach (List<string> adquiriente in adquirientes)
            {
                if (adquiriente[0].Length < largoMaximo || adquiriente[0].Length > largoMaximo)
                {
                    ModelState.AddModelError("Adquirientes", "Los ruts ingresados no siguen el formato, este debe ser el rut sin punto y con guion");
                    return false;
                }
                else if (VerificarFormatoRut(adquiriente[0]) == false)
                {  
                    ModelState.AddModelError("Adquirientes", "Los ruts ingresados no siguen el formato, este debe ser el rut sin punto y con guion");
                    return false;
                }
            }
            if (enajenacion.CNE != regularizacionDePatrimonio)
            foreach (List<string> enajenante in enajenantes)
            {
                if (enajenante[0].Length < largoMaximo || enajenante[0].Length > largoMaximo)
                {
                    ModelState.AddModelError("enajenantes", "Los ruts ingresados no siguen el formato, este debe ser el rut sin punto y con guion");
                    return false;
                }
                else if (VerificarFormatoRut(enajenante[0]) == false)
                {
                    ModelState.AddModelError("enajenantes", "Los ruts ingresados no siguen el formato, este debe ser el rut sin punto y con guion");
                    return false;
                }
            }
            return true;
        }

        private bool ProcesarCNE(List<List<string>> adquirientes, List<List<string>> enajenantes, Enajenacion enajenacion)
        {
            string compraVenta = "1";
            string regularizacionDePatrimonio = "2";

            if (VerificarRut(adquirientes, enajenantes, enajenacion) == false || VerificarNumeroInscripcion(enajenacion) == false ||
                VerificarNumeroInscripcionMenor(enajenacion, adquirientes) == false)
            {
                return false;
            }

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

            string keyEnajenate = "inputEnajentes[]";
            string keyAdquiriente = "inputAdquirientes[]";
            List<string> listaDeEnajenates = Request.Form.GetValues(keyEnajenate)?.ToList();
            List<string> listaDeAdquirientes = Request.Form.GetValues(keyAdquiriente)?.ToList();

            //  En el caso para la lista de Enajenantes y Adquirientes se debe agregar el valor "NO", esto ocurre cuando
            //  en el checklist no se agrega el valor "YES", por lo cual se verifica si se agrega o no.
            var datosFormateados = formulario.FormatearAdquirientesYEnajenantes(listaDeAdquirientes, listaDeEnajenates);
            List<List<string>> listaAdquirientesFormateada = datosFormateados.Item1;
            List<List<string>> listaEnajenantesFormateada = datosFormateados.Item2;

            if (formulario.EsFormularioRepetido(enajenacion) == false && formulario.EsFormularioDeAnoAnterior(enajenacion) == false)
            {
                if (ProcesarCNE(listaAdquirientesFormateada, listaEnajenantesFormateada, enajenacion) == false)
                {
                    return View(enajenacion);
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
                if (formulario.EsFormularioRepetido(enajenacion) || formulario.EsFormularioDeAnoAnterior(enajenacion))
                {
                    string comunaFormulario = enajenacion.Comuna;
                    string manzanaFormulario = enajenacion.Manzana;
                    string predioFormulario = enajenacion.RolPredial;
                    recalcular.RecalcularFormularios(comunaFormulario, manzanaFormulario, predioFormulario);
                }
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

            string comuna = enajenacion.Comuna;
            string manzana = enajenacion.Manzana;
            string predio = enajenacion.RolPredial;

            db.Enajenacion.Remove(enajenacion);
            db.SaveChanges();

            recalcular.RecalcularFormularios(comuna, manzana, predio);

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
