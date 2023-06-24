using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using UAndes.ICC5103._202301.functions;
using UAndes.ICC5103._202301.Models;
using UAndes.ICC5103._202301.Controllers;

namespace UAndes.ICC5103._202301
{
    [TestFixture]
    public class Tests
    {

        [TestCase]
        public void UnitTest1()
        {
            FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario(new InscripcionesBrDbEntities());
            List<List<string>> enajenantes = new List<List<string>>();
            List<List<string>> adquiriente = new List<List<string>>() { new List<string> { "125", "30", "NO" } };
            enajenantes.Add(new List<string> { "2", "100", "NO" });

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
            var actual = serializer.Serialize(funcionMultipropietario.CrearObjetoMultipropietario(adquiriente, enajenacion)[0]);

            Assert.AreEqual(actual, expected);
        }

        [TestCase]
        public void UnitTest2()
        {
            FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario(new InscripcionesBrDbEntities());
            List<List<string>> enajenantes = new List<List<string>>
            {
                new List<string> { "2", "100", "NO" }
            };

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
            var actual = serializer.Serialize(funcionMultipropietario.CrearObjetoMultipropietarioVacio(enajenantes, enajenacion)[0]);

            Assert.AreEqual(actual, expected);
        }

        [TestCase]
        public void UnitTest3()
        {
            FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario(new InscripcionesBrDbEntities());

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

            List<Multipropietario> tester = new List<Multipropietario>()
            {
                multipropietarioTest
            };

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var expected = serializer.Serialize(multipropietarioCompare);
            var actual = serializer.Serialize(funcionMultipropietario.CambiarFechaInicialMultipropietario(tester, enajenacion)[0]);

            Assert.AreEqual(actual, expected);
        }

        [TestCase]
        public void UnitTest4()
        {
            FuncionesMultipropietario funcionMultipropietario = new FuncionesMultipropietario(new InscripcionesBrDbEntities());

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

            List<Multipropietario> tester = new List<Multipropietario>()
            {
                multipropietarioTest
            };

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var expected = serializer.Serialize(multipropietarioTest);
            var actual = serializer.Serialize(funcionMultipropietario.CrearObjetoMultipropetarioVigente(tester, enajenacion)[0]);

            Assert.AreEqual(actual, expected);
        }

        [TestCase]
        public void UnitTest5()
        {
            FuncionesFormulario funcionFormulario = new FuncionesFormulario(new InscripcionesBrDbEntities());
            List<List<string>> expectedEnajenantes = new List<List<string>>
            {
                new List<string> { "383983", "30", "NO" },
                new List<string> { "839894", "70", "NO" }
            };
            List<List<string>> expectedAdquirientes = new List<List<string>>
            {
                new List<string> { "1", "30", "NO" },
                new List<string> { "2", "70", "NO" }
            };

            List<string> testEnajenantes = new List<string>() { "383983", "30", "839894", "70"};
            List<string> testAdquirientes = new List<string>() { "1", "30", "2", "70" };

            (List<List<string>>, List<List<string>>) testData = funcionFormulario.FormatearAdquirientesYEnajenantes(testAdquirientes, testEnajenantes);
            (List<List<string>>, List<List<string>>) compareData = (expectedAdquirientes, expectedEnajenantes);

            Assert.AreEqual(compareData, testData);

        }
        [TestCase]
        public void UnitTest6()
        {
            FuncionesFormulario funcionFormulario = new FuncionesFormulario(new InscripcionesBrDbEntities());
            List<List<string>> noAcreditadosTest = new List<List<string>>
            {
                new List<string> { "383983", "30", "YES" }
            };
            List<List<string>> siAcreditadosTest = new List<List<string>>
            {
                new List<string> { "1", "30", "NO" }
            };

            (List<List<string>>, List<List<string>>) compareData = (siAcreditadosTest, noAcreditadosTest);

            List<List<string>> testData = new List<List<string>>
            {
                new List<string> { "383983", "30", "YES" },
                new List<string> { "1", "30", "NO" }
            };

            (List<List<string>>, List<List<string>>) data = funcionFormulario.ObtenerAdquirientesPorAcreditacion(testData);

            Assert.AreEqual(compareData, data);

        }

        [TestCase]
        public void UnitTest7()
        {
            FuncionesFormulario funcionFormulario = new FuncionesFormulario(new InscripcionesBrDbEntities());
            List<List<string>> testData = new List<List<string>>
            {
                new List<string> { "383983", "30", "YES" },
                new List<string> { "1", "30", "NO" }
            };

            Assert.AreEqual(funcionFormulario.VerificarDatosFormularioHerencia(testData), true);

        }

        [TestCase]
        public void UnitTest8()
        {
            FuncionesFormulario funcionFormulario = new FuncionesFormulario(new InscripcionesBrDbEntities());
            List<List<string>> noAcreditadosTest = new List<List<string>>
            {
                new List<string> { "383983", "30", "YES" }
            };
            List<List<string>> siAcreditadosTest = new List<List<string>>
            {
                new List<string> { "1", "30", "NO" }
            };

            Assert.AreEqual(funcionFormulario.ProcesarAdquirientesPorAcreditacion(siAcreditadosTest, noAcreditadosTest), siAcreditadosTest);

        }

        [TestCase]
        public void UnitTest9()
        {
            EnajenacionsController enajenacionsController = new EnajenacionsController();
            List<List<string>> enajenantes = new List<List<string>>();
            List<List<string>> adquiriente = new List<List<string>>() { new List<string> { "20286382-5", "30", "NO" } };
            enajenantes.Add(new List<string> { "20286382-3", "100", "NO" });

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

            Assert.AreEqual(enajenacionsController.VerificarRut(adquiriente, enajenantes, enajenacion), true);

        }
    }
}