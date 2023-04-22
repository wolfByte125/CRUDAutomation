using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CRUDAutomation
{
    internal class Program
    {
        static void Ref(ref string update)
        {
            update += " added";
        }

        static void Main(string[] args)
        {
            string prj = Assembly.GetCallingAssembly().GetName().Name;
            Ref(ref prj);
            CRUDAutomationService crudAutomation = new CRUDAutomationService();
            crudAutomation.IServiceCreator(new Content
            {
                ModelName = "CheckAgain",
                WithStatus = true
            });
        }
    }
}
