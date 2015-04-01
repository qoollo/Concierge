using System;
using System.Data;
using FluentAssert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;

namespace Qoollo.Concierge.FunctionalTests.AppBuilderTests
{
    [TestClass]
    public class StartupParametersTests : AppBuilderTestBase
    {
        [TestMethod]
        public void Run_OneParameterWithValue_Parsed()
        {
            bool parameterWasParsed = false;

            AppBuilder.AddStartupParameter("-s", () => parameterWasParsed = true);

            string args = "-s";

            AppBuilder.Run(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));

            parameterWasParsed.ShouldBeTrue();
        }

        [TestMethod]
        public void Run_OneParameterWithValue_ValueParsedCorrectly()
        {
            bool parameterWasParsed = false;

            AppBuilder.AddStartupParameter("-s", str =>
            {
                parameterWasParsed = true;
                str.ShouldBeEqualTo("testValue");
            });

            string args = "-s testValue";
            AppBuilder.Run(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));

            parameterWasParsed.ShouldBeTrue();
        }

        [TestMethod]
        public void Run_ParameterWithValueGivenTwoTimes_BothParsedAndNoException()
        {
            int parameterHandledCount = 0;

            AppBuilder.AddStartupParameter("-s", str =>
            {
                parameterHandledCount++;
                str.ShouldBeEqualTo("testValueS");
            });

            AppBuilder.AddStartupParameter("-v", str =>
            {
                parameterHandledCount++;
                str.ShouldBeEqualTo("testValueV");
            });

            string args = "-s testValueS -v testValueV -s testValueS";
            AppBuilder.Run(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));

            parameterHandledCount.ShouldBeEqualTo(3);
        }

        [TestMethod]
        [ExpectedException(typeof (CmdValueForArgumentNotFoundException))]
        public void Run_UnknownParameter_ThrowsCmdValueForArgumentNotFoundException()
        {
            AppBuilder.AddStartupParameter("-known", str => { str.ShouldBeEqualTo("testValueKnown"); });

            string args = "-known testValueKnown -unknown -start None";
            AppBuilder.Run(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
        }

        [TestMethod]
        [ExpectedException(typeof (DuplicateNameException))]
        public void AddStartupParameter_TwoEqualParams_ThrowsDuplicateNameException()
        {
            AppBuilder.AddStartupParameter("-s", str => { str.ShouldBeEqualTo("testValueS"); });

            AppBuilder.AddStartupParameter("-s", str => { str.ShouldBeEqualTo("testValueV"); });
        }

        [TestMethod]
        public void Run_InitializationHelper_CheckCountParamExecutions()
        {
            int counter = 0;
            AppBuilder.AddStartupParameter("-s", () => counter++);

            AppBuilder.Run("-s -s".Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));

            counter.ShouldBeEqualTo(2);
        }

        [TestMethod]
        public void Run_ParseArgs_ParametrWithAndWithoutArgs_ConcatResult()
        {
            string s = "";
            AppBuilder.AddStartupParameter("-s", str => s += str);
            AppBuilder.AddStartupParameter("-d", () => s += "*");

            AppBuilder.DefaultStartupString = "";
            AppBuilder.Run("-s 1 -d -s 2".Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));

            s.ShouldBeEqualTo("1*2");
        }
    }
}