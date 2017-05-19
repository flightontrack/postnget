
    public abstract class Finals
    {
        //public const string LOG_FILES_PATH = "d:/home/LogFiles/";
        public const string LOG_FILES_PATH = "c:/temp/Consoleout/";
        //public const string FLIGHTONTRACK_LINK = "http://flightontrack.azurewebsites.net/PilotUser/CreateUserPilot";
        //public const string FLIGHTONTRACK_LINK = "http://flightontrack-test.azurewebsites.net/PilotUser/CreateUserPilot";
        public const string FLIGHTONTRACK_LINK = "http://localhost:50652/PilotUser/CreateUserPilot";

        public const string REQUEST_PSW = "6";
        public const char RESPONSE_TYPE_COMMAND = '1';
        public const char RESPONSE_TYPE_DATA = '0';
        public const string SEPARATOR_DATA = "!";
        public const string SEPARATOR_FLIGHTID = ":";
        public const char RESPONSE_TYPE_NOTIF = '2';
        public const string NOTIF_UNKNOWN_REQUEST = "1";
        public const string NOTIF_DB_ERROR = "2";
        public const string NOTIF_NORESPONSEDATA = "3";
        public const string NOTIF_UNKNOWN_SERVER_ERROR = "-1";
        public const string DEFAULT_VALUE = "-1";
        public const string UNKNOWN_FLIGHT = "-1";

        public const string RESPONSE_TYPE_DATA_WITHLOAD = "0";
        public const string RESPONSE_TYPE_NOTIF_WITHLOAD = "2";
        public const string RESPONSE_TYPE_DATA_PSW = "aP";

    }

