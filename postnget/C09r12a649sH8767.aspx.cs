
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data.SqlClient;

public partial class C09r12a649sH8767 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        dbExec dbx = new dbExec();
        string request = Request.ToString();
        //Request.SaveAs("C:\\temp\\Request\\crash.txt", true);
        Request.SaveAs("d:/home/LogFiles/crash.txt", true);
        dbx.spExecNonQuery("[insert_CrashLog]", new SqlParameter("@CrashText", request));
        //  string flightId = Request.Form["flightid"];
    }
}
