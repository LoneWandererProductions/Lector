/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        EvaluateCommandTests.cs
 * PURPOSE:     Tests for the EvaluateCommand.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Mediator
{
    /// <summary>
    /// Evaluate command tests.
    /// </summary>
    [TestClass]
    public class EvaluateCommandTests
    {
        private VariableRegistry? _registry;
        private ExpressionEvaluator? _evaluator;
        private EvaluateCommand? _command;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _registry = new VariableRegistry();
            _evaluator = new ExpressionEvaluator(_registry);
            _command = new EvaluateCommand(_evaluator, _registry);
        }

        /// <summary>
        /// Evaluates the simple arithmetic returns correct value.
        /// </summary>
        [TestMethod]
        public void EvaluateSimpleArithmeticReturnsCorrectValue()
        {
            var result = _command.Execute(new string[] { "1 + 2 + 3" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("6", result.Message);
        }

        /// <summary>
        /// Evaluates the store result in registry works correctly.
        /// </summary>
        [TestMethod]
        public void EvaluateStoreResultInRegistryWorksCorrectly()
        {
            var result = _command.Execute(new string[] { "4 + 5", "total" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Stored '9' in 'total'.", result.Message);

            Assert.IsTrue(_registry.TryGet("total", out var val, out var type));
            Assert.AreEqual(9.0, val);
            Assert.AreEqual(EnumTypes.Wdouble, type);
        }

        /// <summary>
        /// Evaluates the boolean expression returns true.
        /// </summary>
        [TestMethod]
        public void EvaluateBooleanExpressionReturnsTrue()
        {
            _registry?.Set("a", 10, EnumTypes.Wdouble);
            _registry?.Set("b", 5, EnumTypes.Wdouble);

            var result = _command.Execute(new string[] { "a > b" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("True", result.Message);
        }

        /// <summary>
        /// Evaluates the boolean expression and or evaluation.
        /// </summary>
        [TestMethod]
        public void EvaluateBooleanExpressionAndOrEvaluation()
        {
            _registry?.Set("flag1", true, EnumTypes.Wbool);
            _registry?.Set("flag2", false, EnumTypes.Wbool);

            var result = _command.Execute(new string[] { "flag1 and flag2" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("False", result.Message);

            result = _command.Execute(new string[] { "flag1 or flag2" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("True", result.Message);
        }

        /// <summary>
        /// Evaluates the unary not expression returns correct value.
        /// </summary>
        [TestMethod]
        public void EvaluateUnaryNotExpressionReturnsCorrectValue()
        {
            _registry?.Set("flag1", true, EnumTypes.Wbool);

            var result = _command.Execute(new string[] { "not flag1" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("False", result.Message);
        }

        /// <summary>
        /// Evaluates the registry variables in expression calculates correctly.
        /// </summary>
        [TestMethod]
        public void EvaluateRegistryVariablesInExpressionCalculatesCorrectly()
        {
            _registry?.Set("x", 2, EnumTypes.Wdouble);
            _registry?.Set("y", 3, EnumTypes.Wdouble);
            _registry.Set("z", 1, EnumTypes.Wdouble);

            var result = _command.Execute(new string[] { "x + y - z" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("4", result.Message);
        }

        /// <summary>
        /// Evaluates the multiple registry variables arithmetic calculates correctly.
        /// </summary>
        [TestMethod]
        public void EvaluateMultipleRegistryVariablesArithmeticCalculatesCorrectly()
        {
            _registry?.Set("score1", 5, EnumTypes.Wdouble);
            _registry?.Set("score2", 10, EnumTypes.Wdouble);
            _registry.Set("score3", 3, EnumTypes.Wdouble);

            var result = _command.Execute(new string[] { "score1 + score2 - score3 * 2" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("9", result.Message);
        }
    }
}