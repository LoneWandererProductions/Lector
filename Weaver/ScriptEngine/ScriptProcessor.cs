namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Executes script blocks, handles if/else, do-while, and goto/label flow.
    /// Routes all commands to Weave.
    /// </summary>
    public sealed class ScriptProcessor
    {
        private readonly Weave _weave;
        private readonly List<(string Category, string? Statement)> _blocks;
        private readonly Dictionary<string, int> _labelMap = new(StringComparer.OrdinalIgnoreCase);

        public ScriptProcessor(Weave weave, List<(string Category, string? Statement)> blocks)
        {
            _weave = weave;
            _blocks = blocks;

            // Build label lookup table
            for (int i = 0; i < _blocks.Count; i++)
            {
                var (cat, stmt) = _blocks[i];
                if (cat == "Label" && stmt != null)
                {
                    var labelName = stmt.TrimEnd(':').Trim();
                    _labelMap[labelName] = i;
                }
            }
        }

        /// <summary>
        /// Executes the script sequentially.
        /// </summary>
        public void Execute()
        {
            int pc = 0; // program counter
            var loopStack = new Stack<(int StartIndex, string Condition)>();

            while (pc < _blocks.Count)
            {
                var (cat, stmt) = _blocks[pc];

                switch (cat)
                {
                    case "Command":
                        if (stmt != null)
                        {
                            var result = _weave.ProcessInput(stmt);
                            if (!result.Success && result.Feedback != null)
                            {
                                // Optional interactive feedback loop
                            }
                        }
                        pc++;
                        break;

                    case "If_Condition":
                        bool condition = EvaluateCondition(stmt ?? "false");
                        if (condition)
                        {
                            pc++; // enter block
                        }
                        else
                        {
                            pc = FindBlockEnd(pc, "If_End", "Else_Open");
                        }
                        break;

                    case "Else_Open":
                        pc++; // enter else block
                        break;

                    case "If_End":
                    case "Else_End":
                        pc++;
                        break;

                    case "Do_Open": // custom for do { ... } while
                        // Record loop start
                        if (stmt != null)
                            loopStack.Push((pc, stmt)); // stmt may hold optional loop name
                        pc++;
                        break;

                    case "Do_End": // marks end of do block, next is while(condition)
                        var next = _blocks[pc + 1];
                        if (next.Category == "While_Condition")
                        {
                            string cond = next.Statement ?? "false";
                            var loop = loopStack.Pop();
                            if (EvaluateCondition(cond))
                            {
                                pc = loop.StartIndex + 1; // loop again
                                loopStack.Push(loop); // re-push
                            }
                            else
                            {
                                pc += 2; // skip while, exit loop
                            }
                        }
                        else
                        {
                            pc++; // no while, continue
                        }
                        break;

                    case "While_Condition":
                        // Should only appear after Do_End
                        pc++;
                        break;

                    case "Goto":
                        if (stmt != null)
                        {
                            var targetLabel = stmt.Split('(', ')')[1].Trim();
                            if (_labelMap.TryGetValue(targetLabel, out var idx))
                            {
                                pc = idx + 1;
                            }
                            else
                            {
                                throw new Exception($"Unknown label '{targetLabel}'");
                            }
                        }
                        else
                        {
                            pc++;
                        }
                        break;

                    default:
                        pc++;
                        break;
                }
            }
        }

        private int FindBlockEnd(int startIndex, string endToken, string? altToken = null)
        {
            for (int i = startIndex + 1; i < _blocks.Count; i++)
            {
                if (_blocks[i].Category == endToken || altToken != null && _blocks[i].Category == altToken)
                    return i;
            }
            return _blocks.Count; // fallthrough
        }

        private bool EvaluateCondition(string condition)
        {
            // Extremely minimal for now: "true" => true, "false" => false
            condition = condition.Trim();
            return condition.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
