﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UAndes.ICC5103._202301.Models;

namespace UAndes.ICC5103._202301.Controllers
{
    public class MultipropietariosController : Controller
    {
        private InscripcionesBrDbEntities db = new InscripcionesBrDbEntities();

        public bool EsNumero(string numero)
        {
            var esNumero = int.TryParse(numero, out int n);
            return esNumero;
        }

        


        // GET: Multipropietarios
        public ActionResult Index(string comuna, string manzana, string predio, string ano)
        {

            comuna = comuna ?? "";
            manzana = manzana ?? "";
            predio = predio ?? "";
            ano = ano ?? "";

            var listaComunas = new Comuna();

            List<string> comunas = listaComunas.ListaDeComunas();

            ViewBag.comunas = comunas;

            if (string.IsNullOrEmpty(comuna) && string.IsNullOrEmpty(manzana) && string.IsNullOrEmpty(predio) && string.IsNullOrEmpty(ano))
            {
                return View(db.Multipropietario.ToList());
            }

            int anoProcesado = 0;
            if (String.IsNullOrEmpty(ano) != true)
            {
                anoProcesado = int.Parse(ano);
            }

            List<Multipropietario> multipropietariosBusqueda = db.Multipropietario.Where(item =>
                                            (item.Comuna == comuna || string.IsNullOrEmpty(comuna)) &&
                                            (item.Manzana == manzana.ToString() || string.IsNullOrEmpty(manzana)) &&
                                            (item.RolPredial == predio.ToString() || string.IsNullOrEmpty(predio)) &&
                                            (item.AnoVigenciaFinal == anoProcesado || string.IsNullOrEmpty(ano))
                                            ).ToList();

            if (multipropietariosBusqueda.Count > 0)
            {
                return View(multipropietariosBusqueda);
            }
            else
            {
                List<Multipropietario> multipropietariosVigentes = db.Multipropietario.Where(item =>
                                            (item.Comuna == comuna || string.IsNullOrEmpty(comuna)) &&
                                            (item.Manzana == manzana.ToString() || string.IsNullOrEmpty(manzana)) &&
                                            (item.RolPredial == predio.ToString() || string.IsNullOrEmpty(predio)) &&
                                            (item.AnoVigenciaInicial >= anoProcesado) &&
                                            (item.AnoVigenciaFinal == null)
                                            ).ToList();

              
                return View(multipropietariosVigentes);
            }
        }

        // GET: Multipropietarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Multipropietario multipropietario = db.Multipropietario.Find(id);
            if (multipropietario == null)
            {
                return HttpNotFound();
            }
            return View(multipropietario);
        }

        // GET: Multipropietarios/Create
        public ActionResult Create()
        {
            var comuna2 = new Comuna();

            List<string> comunas2 = comuna2.ListaDeComunas();

            ViewBag.comunas = comunas2;

            return View();
        }

        // POST: Multipropietarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Comuna,Manzana,RolPredial,RutPropietario,PorcentajeDerechoPropietario,Foja,AnoInscripcion,NumeroInscripcion,FechaInscripcion,AnoVigenciaInicial,AnoVigenciaFinal")] Multipropietario multipropietario)
        {
            if (ModelState.IsValid)
            {
                db.Multipropietario.Add(multipropietario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(multipropietario);
        }

        // GET: Multipropietarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Multipropietario multipropietario = db.Multipropietario.Find(id);
            if (multipropietario == null)
            {
                return HttpNotFound();
            }
            return View(multipropietario);
        }

        // POST: Multipropietarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Comuna,Manzana,RolPredial,RutPropietario,PorcentajeDerechoPropietario,Foja,AnoInscripcion,NumeroInscripcion,FechaInscripcion,AnoVigenciaInicial,AnoVigenciaFinal")] Multipropietario multipropietario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(multipropietario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(multipropietario);
        }

        // GET: Multipropietarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Multipropietario multipropietario = db.Multipropietario.Find(id);
            if (multipropietario == null)
            {
                return HttpNotFound();
            }
            return View(multipropietario);
        }

        // POST: Multipropietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Multipropietario multipropietario = db.Multipropietario.Find(id);
            db.Multipropietario.Remove(multipropietario);
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
