using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW4_FinalTask2
{
    [Transaction(TransactionMode.Manual)]

    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            try
            {
                Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Add room tags");
                tran.Start();
                // create a new instance of class data
                RoomsData data = new RoomsData(commandData);
                
                tran.Commit();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // If there is something wrong, give error information and return failed
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
