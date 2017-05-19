using System;
using System.Data.SqlClient;
using static Finals;

public partial class OffFlightRequest : System.Web.UI.Page
    {

        string sp;
        SqlParameter[] spParameterList;
        string jsonString;

        protected void Page_Load(object sender, EventArgs e)
        {
            dbExec dbx = new dbExec();
            string request = Request.Form["rcode"];
            string phoneNumber = Request.Form["phonenumber"];
            string deviceId = Request.Form["deviceid"];
            string userId = Request.Form["userid"];
            String returnResponse = null; //"2!-1";
            jsonString = dbx.getJsonResponse(RESPONSE_TYPE_NOTIF.ToString(),NOTIF_UNKNOWN_SERVER_ERROR);
            if (request == REQUEST_PSW)
            {
                sp = "get_PilotP";
                SqlParameter[] spParList = { new SqlParameter("@phoneNumber", phoneNumber), new SqlParameter("@deviceid", deviceId) };
                spParameterList = spParList;
                if (dbx.spExecReader(sp, spParList)) { jsonString = dbx.getJsonResponse(RESPONSE_TYPE_DATA_WITHLOAD, RESPONSE_TYPE_DATA_PSW); }
            }
            if (jsonString != null) returnResponse = jsonString;
            Response.Write(returnResponse);
        }
    }