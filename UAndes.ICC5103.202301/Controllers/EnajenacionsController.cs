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

            var jsonEnajente = JsonConvert.SerializeObject(ListaDeEnajenates);
            var jsonAdquiriente = JsonConvert.SerializeObject(ListaDeAdquirientes);


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
