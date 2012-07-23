using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.IO;
namespace de.uni_potsdam.hpi.fsoc
{
    class PS1_Exec
    {
        static void Main(string[] args)
        {
            string code=String.Empty;
            string[] arguments;
            if (args.Length < 2)
                return;
            else if (args[0].StartsWith("-command"))
            {
                code = args[1];
            }
            else if (args[0].StartsWith("-file"))
            {
                code = new StreamReader(args[1]).ReadToEnd();
            }

            arguments = new String[args.Length - 2];
            for (int i = 2; i < args.Length; i++)
            {
                arguments[i - 2] = args[i];
            }
            new PS1_Exec().RunScript(code, arguments);

        }

        string RunScript(String code, String[] arguments)
        {
            try
            {
                Runspace runspace = RunspaceFactory.CreateRunspace();

                runspace.Open();
                runspace.SessionStateProxy.SetVariable("Args", this);
                Pipeline pipeline = runspace.CreatePipeline();
                pipeline.Commands.AddScript(code);

                // add an extra command to transform the script
                // output objects into nicely formatted strings

                // remove this line to get the actual objects
                // that the script returns. For example, the script

                // "Get-Process" returns a collection
                // of System.Diagnostics.Process instances.

                pipeline.Commands.Add("Out-String");

                // execute the script

                Collection<PSObject> results = pipeline.Invoke();

                // close the runspace

                runspace.Close();

                // convert the script result into a single string

                StringBuilder stringBuilder = new StringBuilder();
                foreach (PSObject obj in results)
                {
                    stringBuilder.AppendLine(obj.ToString());
                }

                return stringBuilder.ToString();
            }
            catch (Exception e)
            {
                StreamWriter s=new StreamWriter(@"C:\Users\Administrator.FSOC\Documents\bin\error.log");
                s.Write(e.Message);
                s.Close();
                return "";
            }
        }
    }
}
