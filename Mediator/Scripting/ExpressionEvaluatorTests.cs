/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Scripting
 * FILE:        ExpressionEvaluatorTests.cs
 * PURPOSE:     Test our Evaluation of expressions directly
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.ScriptEngine;
using Weaver.Messages;

namespace Mediator.Scripting
{
    [TestClass]
    public class ExpressionEvaluatorTests
    {
        /// <summary>
        /// Evaluates the numeric with int and double variables works.
        /// </summary>
        [TestMethod]
        public void EvaluateNumeric_With_Int_And_Double_Variables_Works()
        {
            // Arrange
            var registry = new VariableRegistry();
            registry.Set("x", 10L, EnumTypes.Wint);
            registry.Set("y", 2.5, EnumTypes.Wdouble);

            var evaluator = new ExpressionEvaluator(registry);

            // Act
            var result = evaluator.EvaluateNumeric("x + y * 2");

            // Assert
            Assert.AreEqual(10 + 2.5 * 2, result, 0.0001);
        }

        /// <summary>
        /// Evaluates the numeric ignores non numeric variables.
        /// </summary>
        [TestMethod]
        public void EvaluateNumeric_Ignores_NonNumeric_Variables()
        {
            // Arrange
            var registry = new VariableRegistry();
            registry.Set("a", "hello", EnumTypes.Wstring);
            registry.Set("b", true, EnumTypes.Wbool);
            registry.Set("x", 10L, EnumTypes.Wint);

            var evaluator = new ExpressionEvaluator(registry);

            // Act
            var result = evaluator.EvaluateNumeric("x * 3");

            // Assert
            Assert.AreEqual(30, result);
        }
    }
}
