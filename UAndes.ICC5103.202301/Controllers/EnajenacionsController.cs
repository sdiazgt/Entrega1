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


namespace UAndes.ICC5103._202301.Controllers
{
    public class EnajenacionsController : Controller
    {
        private InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();



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

            List<string> ListaDeEnajenates = Request.Form.GetValues(keyEnajenate)?.ToList();
            List<string> ListaDeAdquirientes = Request.Form.GetValues(keyAdquiriente)?.ToList();

            double Comparar1;
            double Comparar2;

            for (int i = 0; i < ListaDeEnajenates.Count; i++)
            {
                Comparar1 = (i + 1) % 3;
                
                if ( (int)Comparar1 == 0 && i != 0)
                {
                    if (ListaDeEnajenates[i] != "YES" && ListaDeEnajenates[i] != "NO")
                    {
                        ListaDeEnajenates.Insert(i, "NO");
                    }
                }
            }

            for (int i = 0; i < ListaDeAdquirientes.Count; i++)
            {
                Comparar2 = (i + 1) % 3;
                if ((int)Comparar2 == 0 && i != 0)
                {
                    if (ListaDeAdquirientes[i] != "YES" && ListaDeAdquirientes[i] != "NO")
                    {
                        ListaDeAdquirientes.Insert(i, "NO");
                    }
                }
            }

            if ((ListaDeEnajenates.Count % 3) != 0)
            {
                ListaDeEnajenates.Insert(ListaDeEnajenates.Count, "NO");
            }
            if ((ListaDeAdquirientes.Count % 3) != 0)
            {
                ListaDeAdquirientes.Insert(ListaDeAdquirientes.Count, "NO");
            }



            List<List<string>> ListaEnajenantesFormateada = new List<List<string>>();
            List<List<string>> ListaAdquirientesFormateada = new List<List<string>>();
            List<string> ListaTemporal = new List<string>();

            foreach (string value in ListaDeEnajenates) {
                if (value == "NO" || value == "YES")
                {
                    ListaTemporal.Add(value);
                    ListaEnajenantesFormateada.Add(ListaTemporal.ToList());
                    ListaTemporal.Clear();
                }
                else 
                {
                    ListaTemporal.Add(value);
                }
                
    
            }

            ListaTemporal.Clear();
            foreach (string value in ListaDeAdquirientes)
            {
                if (value == "NO" || value == "YES")
                {
                    ListaTemporal.Add(value);
                    ListaAdquirientesFormateada.Add(ListaTemporal.ToList());
                    ListaTemporal.Clear();

                }
                else
                {
                    ListaTemporal.Add(value);
                }

            }

            //Validacion de porcentajes para adquirientes en caso que CNE sea "Regularización de Patrimonio" o "Herencia".
            if (enajenacion.CNE == "2")
            {
                int PorcentajeTotal = 0;
                List<List<string>> AdquirientesNoAcredidatos = new List<List<string>>();
                List<List<string>> AdquirientesAcredidatos = new List<List<string>>();

                foreach (List<string> DataAdquriente in ListaAdquirientesFormateada)
                {
                    if (DataAdquriente[2] == "YES")
                    {
                        AdquirientesNoAcredidatos.Add(DataAdquriente);
                        continue;
                    }
                    else
                    {
                        if (DataAdquriente[1].All(char.IsDigit) && DataAdquriente[1] != "")
                        {
                            PorcentajeTotal += int.Parse(DataAdquriente[1]);
                            AdquirientesAcredidatos.Add(DataAdquriente);
                        }
                        else
                        {
                            ModelState.AddModelError("Adquirientes", "Ingrese un valor % valido para los Adquirientes");
                            View(enajenacion);
                        }
                    }
                }
                if(PorcentajeTotal > 100)
                {
                    ModelState.AddModelError("Adquirientes", "Ingrese una suma de % valido para los Adquirientes");

                    View(enajenacion);
                }
                if(AdquirientesNoAcredidatos.Count > 0)
                {
                    float PorcentajeRestante = 100 - (float)PorcentajeTotal;
                    float ReparticionPorcentaje = (float)PorcentajeRestante / (float)AdquirientesNoAcredidatos.Count;
                    foreach(List<string> DataAdquriente in AdquirientesNoAcredidatos)
                    {
                        DataAdquriente[1] = ReparticionPorcentaje.ToString();
                    }
                }
                else if (AdquirientesNoAcredidatos.Count == 0 && PorcentajeTotal < 100)
                {
                    ModelState.AddModelError("Adquirientes", "Ingrese una suma de % valido para los Adquirientes");

                    View(enajenacion);
                }

                AdquirientesAcredidatos.AddRange(AdquirientesNoAcredidatos);
                ListaAdquirientesFormateada = AdquirientesAcredidatos.ToList();

                //Creacion de objeto Multipropetiario
                Multipropietario multipropietario = new Multipropietario();
            }

            

            var jsonEnajente = JsonConvert.SerializeObject(ListaEnajenantesFormateada);
            var jsonAdquiriente = JsonConvert.SerializeObject(ListaAdquirientesFormateada);


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
