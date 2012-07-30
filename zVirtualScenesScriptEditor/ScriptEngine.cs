using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Debugger;

namespace zVirtualScenesScriptEditor
{
    public class ScriptEngine
    {
        
        public event EventHandler<DebugInformation> Break;
        public string Execute(string Script, List<int> breakpoints = null)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Script))
            {
                Jint.JintEngine engine = new Jint.JintEngine();
                engine.AllowClr = true;
                engine.SetDebugMode(true);
                engine.DisableSecurity();
                
                if (breakpoints != null)
                {
                    foreach (var line in breakpoints)
                    {
                        engine.BreakPoints.Add(new Jint.Debugger.BreakPoint(line + 1, 1));
                    }
                    engine.Break += engine_Break;
                }

                try
                {
                    var result = engine.Run(Script, true);
                    if (result != null) sb.Append(result.ToString());

                }
                catch (Exception exc)
                {
                    sb.Append("An error has occured, details:\r\n");
                    sb.Append(exc.Message);
                }
                sb.Append("\r\nDone");
            }
            else
            {
                sb.Append("No script found to execute.");
            }
            return sb.ToString();
        }

        void engine_Break(object sender, Jint.Debugger.DebugInformation e)
        {            
            if (Break != null) Break(sender, e);
        }
    }
}