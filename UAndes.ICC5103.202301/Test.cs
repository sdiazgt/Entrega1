using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using UAndes.ICC5103._202301.functions;
using UAndes.ICC5103._202301.Models;

namespace UAndes.ICC5103._202301
{
    [TestFixture]
    public class Tests
    {

        [TestCase]
        public void UnitTest1()
        {
            FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario();
            List<List<string>> Enajenantes = new List<List<string>>();
            List<List<string>> Adquiriente = new List<List<string>>() { new List<string> { "125", "30", "NO" } };
            Enajenantes.Add(new List<string> { "2", "100", "NO" });
            //Adquirientes.Add(new List<string> { "126", "30", "NO" });
            //Adquirientes.Add(new List<string> { "127", "30", "NO" });

            Enajenacion enajenacion = new Enajenacion
            {
                CNE = "1",
                Comuna = "Santiago",
                Manzana = "13",
                RolPredial = "11",
                Enajenantes = null,
                Adquirientes = null,
                Foja = "10",
                FechaInscripcion = new DateTime(2022,05,09),
                NumeroInscripcion = "1"
            };

            Multipropietario multipropietarioTest = new Multipropietario
            {
                Comuna = enajenacion.Comuna,
                Manzana = enajenacion.Manzana,
                RolPredial = enajenacion.RolPredial,
                RutPropietario = "125",
                PorcentajeDerechoPropietario = "30",
                Foja = enajenacion.Foja,
                NumeroInscripcion = enajenacion.NumeroInscripcion,
                FechaInscripcion = enajenacion.FechaInscripcion,
                AnoInscripcion = 2022,
                AnoVigenciaInicial = 2022
            };

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var expected = serializer.Serialize(multipropietarioTest);
            var actual = serializer.Serialize(funcionMultipropietario.CrearObjetoMultipropietario(Adquiriente, enajenacion)[0]);

            Assert.AreEqual(actual, expected);
        }

        [TestCase]
        public void UnitTest2()
        {
            FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario();
            List<List<string>> Enajenantes = new List<List<string>>();
            Enajenantes.Add(new List<string> { "2", "100", "NO" });

            Enajenacion enajenacion = new Enajenacion
            {
                CNE = "1",
                Comuna = "Santiago",
                Manzana = "13",
                RolPredial = "11",
                Enajenantes = null,
                Adquirientes = null,
                Foja = "10",
                FechaInscripcion = new DateTime(2022, 05, 09),
                NumeroInscripcion = "1"
            };

            Multipropietario multipropietarioTest = new Multipropietario
            {
                Comuna = enajenacion.Comuna,
                Manzana = enajenacion.Manzana,
                RolPredial = enajenacion.RolPredial,
                RutPropietario = "2",
                PorcentajeDerechoPropietario = "0.00",
                Foja = null,
                NumeroInscripcion = null,
                FechaInscripcion = null,
                AnoInscripcion = null,
                AnoVigenciaInicial = 2022
            };

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var expected = serializer.Serialize(multipropietarioTest);
            var actual = serializer.Serialize(funcionMultipropietario.CrearObjetoMultipropietarioVacio(Enajenantes, enajenacion)[0]);

            Assert.AreEqual(actual, expected);
        }

        [TestCase]
        public void UnitTest3()
        {
            FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario();
            List<List<string>> Enajenantes = new List<List<string>>();
            Enajenantes.Add(new List<string> { "2", "100", "NO" });

            Enajenacion enajenacion = new Enajenacion
            {
                CNE = "1",
                Comuna = "Santiago",
                Manzana = "13",
                RolPredial = "11",
                Enajenantes = null,
                Adquirientes = null,
                Foja = "10",
                FechaInscripcion = new DateTime(2022, 05, 09),
                NumeroInscripcion = "1"
            };

            Multipropietario multipropietarioTest = new Multipropietario
            {
                Comuna = enajenacion.Comuna,
                Manzana = enajenacion.Manzana,
                RolPredial = enajenacion.RolPredial,
                RutPropietario = "2",
                PorcentajeDerechoPropietario = "0.00",
                Foja = null,
                NumeroInscripcion = null,
                FechaInscripcion = null,
                AnoInscripcion = null,
                AnoVigenciaInicial = 2019
            };

            Multipropietario multipropietarioCompare = new Multipropietario
            {
                Comuna = enajenacion.Comuna,
                Manzana = enajenacion.Manzana,
                RolPredial = enajenacion.RolPredial,
                RutPropietario = "2",
                PorcentajeDerechoPropietario = "0.00",
                Foja = null,
                NumeroInscripcion = null,
                FechaInscripcion = null,
                AnoInscripcion = null,
                AnoVigenciaInicial = 2022
            };

            List<Multipropietario> Tester = new List<Multipropietario>()
            {
                multipropietarioTest
            };

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var expected = serializer.Serialize(multipropietarioCompare);
            var actual = serializer.Serialize(funcionMultipropietario.CambiarFechaInicialMultipropietario(Tester, enajenacion)[0]);

            Assert.AreEqual(actual, expected);
        }

        [TestCase]
        public void UnitTest4()
        {
            FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario();
            List<List<string>> Enajenantes = new List<List<string>>();
            Enajenantes.Add(new List<string> { "2", "100", "NO" });

            Enajenacion enajenacion = new Enajenacion
            {
                CNE = "1",
                Comuna = "Santiago",
                Manzana = "13",
                RolPredial = "11",
                Enajenantes = null,
                Adquirientes = null,
                Foja = "10",
                FechaInscripcion = new DateTime(2022, 05, 09),
                NumeroInscripcion = "1"
            };

            Multipropietario multipropietarioTest = new Multipropietario
            {
                Comuna = enajenacion.Comuna,
                Manzana = enajenacion.Manzana,
                RolPredial = enajenacion.RolPredial,
                RutPropietario = "125",
                PorcentajeDerechoPropietario = "30",
                Foja = enajenacion.Foja,
                NumeroInscripcion = enajenacion.NumeroInscripcion,
                FechaInscripcion = enajenacion.FechaInscripcion,
                AnoInscripcion = 2022,
                AnoVigenciaInicial = 0
            };

            List<Multipropietario> Tester = new List<Multipropietario>()
            {
                multipropietarioTest
            };

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var expected = serializer.Serialize(multipropietarioTest);
            var actual = serializer.Serialize(funcionMultipropietario.CrearObjetoMultipropetarioVigente(Tester, enajenacion)[0]);

            Assert.AreEqual(actual, expected);
        }
    }
}