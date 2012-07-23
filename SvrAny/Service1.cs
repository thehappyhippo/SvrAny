using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Management;
using Microsoft.Win32;

namespace de.uni_potsdam.hpi.fsoc
{
    public partial class Service1 : ServiceBase
    {
        Process _process;
        RegistryKey HKLM = Registry.LocalMachine;
        String servicename = String.Empty;

        System.Diagnostics.EventLog _e = new EventLog();
        System.Timers.Timer _timer = new System.Timers.Timer(5000);

        protected String BinPath { get; set; }
        protected String Arguments { get; set; }
        
        public Service1()
        {
               
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            //if (!EventLog.SourceExists("Demo"))
            //    EventLog.CreateEventSource("Demo", "");
            //_e.Source = "Demo";
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Enabled = false;
           

            ServiceController[] scServices;
            scServices = ServiceController.GetServices();

            // Display the list of services currently running on this computer.
            int my_pid = System.Diagnostics.Process.GetCurrentProcess().Id;

            foreach (ServiceController scTemp in scServices)
            {
                // Write the service name and the display name
                // for each running service.

                // Query WMI for additional information about this service.
                // Display the start name (LocalSytem, etc) and the service
                // description.
                ManagementObject wmiService;
                wmiService = new ManagementObject("Win32_Service.Name='" + scTemp.ServiceName + "'");
                wmiService.Get();

                int id = Convert.ToInt32(wmiService["ProcessId"]);
                if (id == my_pid)
                {
                    servicename=scTemp.ServiceName;
                    break;
                }
            }
            //SelectQuery query = new SelectQuery("SELECT * FROM Win32_Service where ProcessId  = " + System.Diagnostics.Process.GetCurrentProcess().Id);
            //ManagementObjectSearcher search = new ManagementObjectSearcher(query);
            //foreach (ManagementObject o in search.Get())
            //{
            //    _e.WriteEntry(o.GetPropertyValue("Name").ToString());
                
                
            //}
            if (!EventLog.SourceExists(servicename))
                EventLog.CreateEventSource(servicename, "");
            _e.Source = servicename;
            _e.WriteEntry("Init finished");
            
            this.S();

        }
        
        protected override void OnStart(string[] args)
        {
            _timer.Enabled = true;
           
        }

        protected override void OnStop()
        {
            _e.WriteEntry("Stopping");
            if (!_process.HasExited)
            {
                _process.Kill();
                _e.WriteEntry("Process killed");
            }
        }

        void S()
        {
            try
            {
                
                RegistryKey servicekey=HKLM.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\" + servicename,false);
                _e.WriteEntry(servicekey.Name);
                string path = servicekey.GetValue("BinPath").ToString();
                string args = servicekey.GetValue("ProcArguments").ToString();
            
                _e.WriteEntry(path +" "+ args);

                _process = Process.Start(path, args);
                _process.Exited += new EventHandler(_process_Exited);
            }
            catch (Exception e)
            {
                _e.WriteEntry(e.Message);
                this.Stop();
                
            }
            
        }

        void _process_Exited(object sender, EventArgs e)
        {
            this.Stop();
        }

        
    }
}
