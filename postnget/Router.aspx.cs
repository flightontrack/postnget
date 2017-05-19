using System;
using System.Data.SqlClient;
using static Finals;
public partial class Router : System.Web.UI.Page

{
    static bool inProcess=false;
    private bool isDebug;
    private string isDebugString;

    public Router() {
        isDebug = false;
        isDebugString = "false";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        var dbx = new dbExec();

        isDebugString = Request.Form["isdebug"];
        if (!(isDebugString == null)){
            isDebug = bool.Parse(isDebugString);        
        }

        string request = Request.Form["rcode"];
        string flightId = Request.Form["flightid"];
        string routeId = Request.Form["routeid"];
        string speed = Request.Form["speed"];
        string isSpeedLow = Request.Form["speedlowflag"];

        string phoneNumber = Request.Form["phonenumber"];
        string deviceid = Request.Form["deviceid"];
        string userid = Request.Form["userid"];
        string username = Request.Form["username"];
        string passw = Request.Form["aid"];
        string appversioncode = Request.Form["versioncode"];
        string AcftMake = Request.Form["AcftMake"];
        string AcftModel = Request.Form["AcftModel"];
        string AcftSeries = Request.Form["AcftSeries"];
        string AcftNum = Request.Form["AcftNum"];
        string AcftTagId = Request.Form["AcftTagId"];
        string AcftName = Request.Form["AcftName"];
        string isFlyingPattern = Request.Form["isFlyingPattern"];
        string speedThreshold = Request.Form["speed_thresh"];
        string freq = Request.Form["freq"];

        //string flightid = Request.Form["flightid"];
        string latitude = Request.Form["latitude"];
        string longitude = Request.Form["longitude"];

        string accuracy = Request.Form["accuracy"];
        string extraInfo = Request.Form["extrainfo"];
        string wpntnum = Request.Form["wpntnum"];
        string gsmSignal = Request.Form["gsmsignal"];
        int? commLogID = null;

        String returnResponse = RESPONSE_TYPE_NOTIF + SEPARATOR_DATA + NOTIF_UNKNOWN_SERVER_ERROR + SEPARATOR_FLIGHTID+UNKNOWN_FLIGHT; //"2!-1";
        if (inProcess)
        {
            //debug feature to prevent multiinstances through static variable
            Response.Write(returnResponse);
            return;
        };

        if (isDebug){
            if (dbx.dbExecScalarInt("[insert_CommunicationLog]",
                new SqlParameter("@rcode", request),
                new SqlParameter("@flightid", flightId),
                new SqlParameter("@speed", speed),
                new SqlParameter("@speedlowflag", isSpeedLow),
                new SqlParameter("@phonenumber", phoneNumber),
                new SqlParameter("@deviceid", deviceid),
                new SqlParameter("@aid", passw),
                new SqlParameter("@AcftMake", AcftMake),
                new SqlParameter("@AcftModel", AcftModel),
                new SqlParameter("@AcftSeries", AcftSeries),
                new SqlParameter("@AcftRegNum", AcftNum),
                new SqlParameter("@AcftTagId", AcftTagId),
                new SqlParameter("@AcftName", AcftName),
                new SqlParameter("@isFlyingPattern", isFlyingPattern),
                new SqlParameter("@speed_thresh", speedThreshold),
                new SqlParameter("@latitude", latitude),
                new SqlParameter("@longitude", longitude),
                new SqlParameter("@accuracy", accuracy),
                new SqlParameter("@extrainfo", extraInfo),
                new SqlParameter("@wpntnum", wpntnum),
                new SqlParameter("@userid",userid),
                new SqlParameter("@username",username),
                new SqlParameter("@freq",freq),
                new SqlParameter("@routeId",routeId),
                new SqlParameter("@appversioncode",appversioncode)
                )
               )
            {
                commLogID = dbx.returnInt;
            }
            else
            {
                Response.Write(returnResponse);
            }
        }
        bool returnSpSuccess = true;
        bool? isSpeedLowBoolean = null;
        if (isSpeedLow != null) {isSpeedLowBoolean = Boolean.Parse(isSpeedLow); };
        //return;
        //if (flightId != null)
        //{
        //    returnSpSuccess = dbx.getProcessStatus(new SqlParameter("@flightId", flightId));
        //    //if (returnSpSuccess & dbx.returnScalar) { Response.Write(returnResponse); return; }
        //    //else { returnSpSuccess = dbx.spExecNonQuery("[update_ProcessStatus]", new SqlParameter("@FlightID", flightId), new SqlParameter("@InProgress", true)); 
        //    //}
        //}

        if (returnSpSuccess && dbx.getAction(
            new SqlParameter("@requestID", request),
            new SqlParameter("@flightId", flightId),
            new SqlParameter("@speed", speed),
            new SqlParameter("@isspeedlow", isSpeedLowBoolean)
            ))
        {
            if (dbx.FlightControllerID.HasValue)
            {
                if (dbx.IsCreateFlight)
                {
                    //debug feature to prevent multiinstances through static variable
                    //inProcess = true;

                    Boolean isFlyingPatternBoolean = false;
                    flightId = UNKNOWN_FLIGHT;
                    dbx.getPilotID(userid);
                    if (dbx.returnInt == null) {
                        dbx.createPilotAndUser(phoneNumber, deviceid, passw, username, userid);
                    }
                    else dbx.spExecNonQuery("[update_Pilot]", new SqlParameter("@userid", userid), new SqlParameter("@username", username));

                    if (isFlyingPattern != null) { isFlyingPatternBoolean = Boolean.Parse(isFlyingPattern); };
                    returnSpSuccess = (dbx.dbExecScalarInt("[create_Flight]",

                        new SqlParameter("@Pilotid", dbx.returnInt), 
                        new SqlParameter("@AcftMake", AcftMake),
                        new SqlParameter("@AcftModel", AcftModel), 
                        new SqlParameter("@AcftSeries", AcftSeries), 
                        new SqlParameter("@AcftNum", AcftNum),
                        new SqlParameter("@AcftTagId", AcftTagId), 
                        new SqlParameter("@AcftName", AcftName), 
                        new SqlParameter("@isFlyingPattern", isFlyingPatternBoolean), 
                        new SqlParameter("@SpeedThreshhold", speedThreshold),
                        new SqlParameter("@FlightControllerID", dbx.FlightControllerID), 
                        new SqlParameter("@Freq", freq),
                        new SqlParameter("@Routeid", routeId),
                        new SqlParameter("@VersionCode", appversioncode)));

                    if (returnSpSuccess) { flightId = dbx.returnInt.ToString(); 
                    }
                    else {
                        LogHelper.onLog("[create_Flight]", "FlightID = " + flightId);
                        Response.Write(returnResponse);
                        return;
                    }
                }
                if (dbx.IsUpdFlightStatus)
                {
                    returnSpSuccess = dbx.spExecNonQuery("[update_FlightStatus]",
                    new SqlParameter("@FlightStateID", dbx.FlightStateID),
                    new SqlParameter("@FlightID", flightId));
                }
                if (returnSpSuccess && dbx.IsUpdFlightAttrib)
                {
                    returnSpSuccess = dbx.spExecNonQuery("[update_FlightAttrib]",
                       new SqlParameter("@FlightID", flightId),
                       new SqlParameter("@FlightControllerID", dbx.FlightControllerID));
                }
                if (returnSpSuccess && dbx.IsSetLocation)
                {

                    string date = Server.UrlDecode(Request.Form["date"]);
                    date = convertFromMySqlDate(date);

                    returnSpSuccess = dbx.spExecNonQuery("set_Location",
                    new SqlParameter("@flightid", flightId),
                    new SqlParameter("@flightControllerID", dbx.FlightControllerID),
                    new SqlParameter("@latitude", latitude),
                    new SqlParameter("@longitude", longitude),
                    new SqlParameter("@speed", speed),
                    new SqlParameter("@date", date),
                    new SqlParameter("@accuracy", accuracy),
                    new SqlParameter("@extraInfo", extraInfo),
                    new SqlParameter("@wpntnum", wpntnum),
                    new SqlParameter("@signalstrength", gsmSignal)
                    );
                }

                if (dbx.getResponse(
                    new SqlParameter("@FlightControllerID", dbx.FlightControllerID)
                ))
                {
                    returnResponse = dbx.responseType.ToString() + SEPARATOR_DATA + dbx.responseData + SEPARATOR_FLIGHTID + dbx.responseFlightId.ToString();
                }
            }
            else { returnResponse = RESPONSE_TYPE_NOTIF + SEPARATOR_DATA + NOTIF_UNKNOWN_REQUEST + SEPARATOR_FLIGHTID + dbx.responseFlightId.ToString(); }
        }
        Response.Write(returnResponse);

        if (flightId != null) { returnSpSuccess = dbx.spExecNonQuery("[update_ProcessStatus]", new SqlParameter("@FlightID", flightId), new SqlParameter("@InProgress", false)); }
        if (isDebug && commLogID != null) { returnSpSuccess = dbx.spExecNonQuery("[update_CommunicationLog]", new SqlParameter("@commlogid", commLogID), new SqlParameter("@response", returnResponse)); }
    }
    private string convertFromMySqlDate(string date)
    {
        DateTime dt = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss",
        System.Globalization.CultureInfo.InvariantCulture);
        return dt.ToString();
    }
}
