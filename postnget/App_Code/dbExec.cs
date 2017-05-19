using System;
//using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using static Finals;


public class dbExec
{
    //public const char RESPONSE_TYPE_COMMAND = '1';
    //public const char RESPONSE_TYPE_DATA = '0';
    //public const string SEPARATOR_DATA = "!";
    //public const string SEPARATOR_FLIGHTID = ":";
    //public const char RESPONSE_TYPE_NOTIF = '2';
    //public const string NOTIF_UNKNOWN_REQUEST = "1";
    //public const string NOTIF_DB_ERROR = "2";
    //public const string NOTIF_NORESPONSEDATA = "3";
    //public const string NOTIF_UNKNOWN_SERVER_ERROR = "-1";


    public Boolean IsCreateFlight;
    public Boolean IsUpdFlightStatus;
    public Boolean IsUpdFlightAttrib;
    public Boolean IsSetLocation;
    public int? FlightControllerID;
    public int? FlightStateID;
    public int? ChecksumID_Response;
    public int responseType;
    public int responseFlightId;
    public string responseData;
    public string returnResponse;
    public bool returnIfSuccess=true;
    public bool returnScalar;
    public int? returnInt=null;
    public object returnScalarObj;


    SqlDataReader sqlDataReader = null;
    SqlConnection sqlConnection = null;
    SqlCommand cmd = null;
    //FileStream filestream;
    //StreamWriter streamwriter;

    public dbExec()
    {
        sqlConnection = new SqlConnection();
        sqlConnection.ConnectionString = GetConnectionString();

        cmd = new SqlCommand();
        cmd.Connection = sqlConnection;
        cmd.CommandType = CommandType.StoredProcedure;
    }

    public bool getAction(params SqlParameter[] spParameterList)
    {
        try
        {
            if(spExecReader("[set_FlightController]", spParameterList)){
                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        IsCreateFlight = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("a_IsCreateFlight"));
                        IsUpdFlightStatus = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("a_IsUpdFlightStatus"));
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("a_IsUpdFlightAttrib"))) { IsUpdFlightAttrib = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("a_IsUpdFlightAttrib")); }
                        IsSetLocation = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("a_IsSetLocation"));
                        FlightControllerID = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("FlightControllerID"));
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("FlightStateID"))) { FlightStateID = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("FlightStateID")); }
                        ChecksumID_Response = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("ChecksumID_Response"));
                    }
                }
                else{
                    responseType = RESPONSE_TYPE_NOTIF;
                    responseData = NOTIF_UNKNOWN_REQUEST;
                    //Console.WriteLine("getAction:  No rows returned.");
                }
            if (sqlDataReader != null) sqlDataReader.Close();
            if (sqlConnection != null) sqlConnection.Close();
            }
            else return false;
        }
        catch (Exception e)
        {
            onDbExecFailure("getAction;", e);
            //returnIfSuccess = false;
            if (sqlDataReader != null) sqlDataReader.Close();
        }
        finally
        {
            if (sqlConnection != null) sqlConnection.Close();
        }
    return true;
    }

    public bool getResponse(params SqlParameter[] spParameterList)
    {
        if(spExecReader("[get_Response]", spParameterList)){
            if (sqlDataReader.HasRows)
            {
                while (sqlDataReader.Read())
                {
                    responseType = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("responseType"));
                    responseData = sqlDataReader.GetString(sqlDataReader.GetOrdinal("responseData"));
                    responseFlightId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("responseFlightId"));
                }
            }
            else
            {
                responseType = RESPONSE_TYPE_NOTIF;
                responseData = NOTIF_NORESPONSEDATA;
                Console.WriteLine("getResponse: No rows returned.");
            }
        if (sqlDataReader != null) sqlDataReader.Close();
        if (sqlConnection != null) sqlConnection.Close();
        }    
        else return false;
    return true;
    }

    public bool getProcessStatus(params SqlParameter[] spParameterList)
    {
        return dbExecScalarBool("get_ProcessStatus", spParameterList);
    }

    public bool  getPilotID(string userid){
        returnInt = null;
        return dbExecScalarInt("get_PilotID",
            new SqlParameter("@userid", userid));
            //new SqlParameter("@pilotcode", phoneNumber),
            //new SqlParameter("@deviceid", deviceid));
    }
    public bool createPilotAndUser(string phoneNumber, string deviceid, string psw,string username, string userid)
    {

        returnIfSuccess = true;
        returnInt = null;
        dbExecScalarInt("create_Pilot",
            new SqlParameter("@pilotcode", phoneNumber),
            new SqlParameter("@deviceid", deviceid),
            new SqlParameter("@androidid", psw),
            new SqlParameter("@username", username),
            new SqlParameter("@userid", userid)
            );

        WebRequest request = WebRequest.Create(FLIGHTONTRACK_LINK);
        request.Method = "POST";
        //string postData = "param1=" + phoneNumber + "&param2=" + deviceid + "&param3=" + psw;
        string postData = "param1=" + returnInt;
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteArray.Length;
        try
        {
        /// inext line is a patch that update PilotUserName = null which later should be updated again to a value if the user created successfully
        //spExecNonQuery("[update_PilotUserName]", new SqlParameter("@pilotcode", phoneNumber), new SqlParameter("@deviceid", deviceid));
        
        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);
        dataStream.Close();
            WebResponse response = request.GetResponse();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                string responseText = sr.ReadToEnd();
                return spExecNonQuery("[update_PilotUserName]", new SqlParameter("@pilotcode", phoneNumber), new SqlParameter("@deviceid", deviceid), new SqlParameter("@pilotusername", responseText));
            }
        }
        catch (Exception e) {
            onDbExecFailure("createPilotAndUser;" , e);
            returnIfSuccess = false;
            if (sqlDataReader != null) sqlDataReader.Close();
            return returnIfSuccess;
        }
    }
    
    public bool dbExecScalarBool(string storedProcedureName, params SqlParameter[] spParameterList)
    {
        returnIfSuccess = true;
        try
        {
            cmd.CommandText = storedProcedureName;
            if (spParameterList.Length > 0)
            {
                cmd.Parameters.Clear();
                for (int i = 0; i < spParameterList.Length; i++)
                {
                    cmd.Parameters.Add(spParameterList[i]);
                }
            }
            sqlConnection.Open();
            returnScalar = (bool)cmd.ExecuteScalar();
        }
        catch (Exception e)
        {
            onDbExecFailure("dbExecScalarBool;" +storedProcedureName, e);
            returnIfSuccess = false; //wrong needs to be redone
            if (sqlDataReader != null) sqlDataReader.Close();
        }
        finally
        {
            if (sqlConnection != null) sqlConnection.Close();
        }
        return returnIfSuccess;
    }

    public bool dbExecScalarInt(string storedProcedureName, params SqlParameter[] spParameterList)
    {
        returnIfSuccess = true;
        try
        {
            cmd.CommandText = storedProcedureName;
            if (spParameterList.Length > 0)
            {
                cmd.Parameters.Clear();
                for (int i = 0; i < spParameterList.Length; i++)
                {
                    cmd.Parameters.Add(spParameterList[i]);
                }
            }
            sqlConnection.Open();
            var returnobject=cmd.ExecuteScalar();
            //if (!DBNull.Value.Equals(retururnobject)) { returnInt = (int)retururnobject; }
            if (returnobject != null && !DBNull.Value.Equals(returnobject)) { returnInt = (int)returnobject; }
            //if (DBNull.Value.Equals(cmd.ExecuteScalar())) { };
        }
        catch (Exception e)
        {
            onDbExecFailure("dbExecScalarInt;" + storedProcedureName, e);
            returnIfSuccess = false;
            if (sqlDataReader != null) sqlDataReader.Close();
        }
        finally
        {
            if (sqlConnection != null) sqlConnection.Close();
        }
        return returnIfSuccess;
    }


    public bool spExecReader(string storedProcedureName, params SqlParameter[] spParameterList)
    {
        try
        {
            returnIfSuccess = true;
            cmd.CommandText = storedProcedureName;
            if (spParameterList.Length > 0)
            {
                cmd.Parameters.Clear();
                for (int i = 0; i < spParameterList.Length; i++)
                {
                    cmd.Parameters.Add(spParameterList[i]);
                }
            }
            sqlConnection.Open();
            sqlDataReader = cmd.ExecuteReader();
        }
        catch (Exception e)
        {
            onDbExecFailure("spExecReader",e);
            returnIfSuccess = false;
            if (sqlDataReader != null) sqlDataReader.Close();
        }

        return returnIfSuccess;
    }
    
    public bool spExecNonQuery(string storedProcedureName, params SqlParameter[] spParameterList)
        {
            try
            {
                returnIfSuccess = true;
                cmd.CommandText = storedProcedureName;
                if (spParameterList.Length > 0)
                {
                    cmd.Parameters.Clear();
                    for (int i = 0; i < spParameterList.Length; i++)
                    {
                        cmd.Parameters.Add(spParameterList[i]);
                    }
                }
                sqlConnection.Open();
                cmd.ExecuteNonQuery();
                //returnInt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                onDbExecFailure("spExecNonQuery", e, storedProcedureName);
                returnIfSuccess = false;
            }
            finally
            {
                if (sqlConnection != null) sqlConnection.Close();
            }
            return returnIfSuccess;
        }

    private string GetConnectionString()
    {
        return ConfigurationManager.ConnectionStrings
            ["MSSQLConnectionString"].ConnectionString;
    }
    private void onDbExecFailure(string source, Exception e, string parname = "Unknown")
    {
        responseType = RESPONSE_TYPE_NOTIF;
        responseData = NOTIF_DB_ERROR; //db error
        LogHelper.onFailureLog(source, e, parname);
    }


    public bool dbExecScalar(string storedProcedureName, SqlParameter[] spParameterList)
    {
        returnIfSuccess = true;
        try
        {
            cmd.CommandText = storedProcedureName;
            if (spParameterList.Length > 0)
            {
                cmd.Parameters.Clear();
                for (int i = 0; i < spParameterList.Length; i++)
                {
                    cmd.Parameters.Add(spParameterList[i]);
                }
            }
            sqlConnection.Open();
            returnScalarObj = null;
            returnScalarObj = cmd.ExecuteScalar();
        }
        catch (Exception e)
        {
            onDbExecFailure("dbExecScalar;" + storedProcedureName, e);
            returnIfSuccess = false;
            if (sqlDataReader != null) sqlDataReader.Close();
        }
        finally
        {
            if (sqlConnection != null) sqlConnection.Close();
        }
        return returnIfSuccess;
    }

    public string getJson()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        if (returnScalarObj == null) return null;
        return serializer.Serialize(returnScalarObj);
    }

    public string getJsonResponse(string  responsetypekey,string  responsetypevalue = DEFAULT_VALUE)
    {
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
        Dictionary<string, object> responsetyperow = new Dictionary<string, object>();
        responsetyperow.Add(responsetypekey, responsetypevalue);
        rows.Add(responsetyperow);

        if (responsetypekey[0] == RESPONSE_TYPE_DATA)
        {
            if (sqlDataReader == null || sqlDataReader.IsClosed) return null;
            var columns = new List<string>();


            for (int i = 0; i < sqlDataReader.FieldCount; i++)
            {
                columns.Add(sqlDataReader.GetName(i));
            }

            while (sqlDataReader.Read())
            {
                var row = columns.ToDictionary(column => column, column => sqlDataReader[column]);
                rows.Add(row);
            }
            if (sqlDataReader != null) sqlDataReader.Close();
            if (sqlConnection != null) sqlConnection.Close();
        }
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        return serializer.Serialize(rows);
    }
}