/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Scripting
 * FILE:        ExpressionEvaluatorTests.cs
 * PURPOSE:     Test our Evaluation of expressions directly
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.ScriptEngine;
using Weaver.Messages;
using Weaver.Evaluate;

namespace Mediator.Scripting
{
    [TestClass]
    public class ExpressionEvaluatorTests
    {

        /// <summary>
        /// Evaluates the numeric with int and double variables works.
        /// </summary>
        [TestMethod]
        public void EvaluateDo_While()
        {
            // Arrange
            var registry = new VariableRegistry();
            registry.Set("count", 3, EnumTypes.Wint);

            var evaluator = new ExpressionEvaluator(registry);

            // Act
            var result = evaluator.Evaluate("count > 3");

            // Assert
            Assert.AreEqual(false, result, "Wrong Bool");

            // Act
            result = evaluator.Evaluate("count > 2");

            // Assert
            Assert.AreEqual(true, result, "Wrong Bool");
        }

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

        /// <summary>
        /// Evaluates the boolean literals works.
        /// </summary>
        [TestMethod]
        public void Evaluate_Boolean_Literals_Works()
        {
            var evaluator = new ExpressionEvaluator();

            Assert.IsTrue(evaluator.Evaluate("true"));
            Assert.IsFalse(evaluator.Evaluate("false"));
        }

        /// <summary>
        /// Evaluates the logical not works.
        /// </summary>
        [TestMethod]
        public void Evaluate_Logical_Not_Works()
        {
            var evaluator = new ExpressionEvaluator();

            Assert.IsTrue(evaluator.Evaluate("not false"));
            Assert.IsFalse(evaluator.Evaluate("not true"));
        }


        /// <summary>
        /// Evaluates the comparison operators works.
        /// </summary>
        [TestMethod]
        public void Evaluate_Comparison_Operators_Works()
        {
            var evaluator = new ExpressionEvaluator();

            Assert.IsTrue(evaluator.Evaluate("5 == 5"));
            Assert.IsFalse(evaluator.Evaluate("5 != 5"));
            Assert.IsTrue(evaluator.Evaluate("5 != 3"));
            Assert.IsTrue(evaluator.Evaluate("7 > 3"));
            Assert.IsFalse(evaluator.Evaluate("2 > 3"));
            Assert.IsTrue(evaluator.Evaluate("3 < 5"));
            Assert.IsFalse(evaluator.Evaluate("10 < 5"));
            Assert.IsTrue(evaluator.Evaluate("5 >= 5"));
            Assert.IsTrue(evaluator.Evaluate("6 >= 5"));
            Assert.IsTrue(evaluator.Evaluate("5 <= 5"));
            Assert.IsTrue(evaluator.Evaluate("4 <= 5"));
        }

        /// <summary>
        /// Evaluates the complex expression works.
        /// </summary>
        [TestMethod]
        public void Evaluate_Complex_Expression_Works()
        {
            var registry = new VariableRegistry();
            registry.Set("x", 5, EnumTypes.Wint);
            registry.Set("y", 10, EnumTypes.Wint);
            registry.Set("z", false, EnumTypes.Wbool);

            var evaluator = new ExpressionEvaluator(registry);

            Assert.IsTrue(evaluator.Evaluate("( x < y ) && not z"));
            Assert.IsFalse(evaluator.Evaluate("( x > y ) || z"));
        }

        /// <summary>
        /// Evaluates the unsupported operator throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Evaluate_Unsupported_Operator_Throws()
        {
            var evaluator = new ExpressionEvaluator();
            evaluator.Evaluate("5 ^^ 2"); // unsupported operator
        }
    }
}
