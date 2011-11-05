using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace qserver
{
    public class Util
    {

        /// <summary>
        /// Read configuration parameters
        /// </summary>
        protected static String m_MainConnectionString = ConfigurationManager.AppSettings.Get("MainConnectionString");
        protected static String m_NumberOfThreads = ConfigurationManager.AppSettings.Get("NumberOfThreads");

        public static String MainConnectionString
        {
            get
            {
                return m_MainConnectionString;
            }
        }

        public static String NumberOfThreads
        {
            get
            {
                return m_NumberOfThreads;
            }
        }

    }
}
