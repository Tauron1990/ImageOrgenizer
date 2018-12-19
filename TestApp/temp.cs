using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsFormsApplication1
{
    public enum SEVERITY
    {
        Success = 00,
        Informational = 01,
        Warning = 10,
        Error = 11
    }

    public enum FACILITY
    {
        NULL = 0,
        RPC = 1,
        DISPATCH = 2,
        STORAGE = 3,
        ITF = 4,
        WIN32 = 7,
        WINDOWS = 8,
        SSPI = 9,
        SECURITY = 9,
        CONTROL = 10,
        CERT = 11,
        INTERNET = 12,
        MEDIASERVER = 13,
        MSMQ = 14,
        SETUPAPI = 15,
        SCARD = 16,
        COMPLUS = 17,
        AAF = 18,
        URT = 19,
        ACS = 20,
        DPLAY = 21,
        UMI = 22,
        SXS = 23,
        WINDOWS_CE = 24,
        HTTP = 25,
        BACKGROUNDCOPY = 32,
        CONFIGURATION = 33
    }

    /// <summary>
    /// Use this class to decipher the meanings of hresult
    /// </summary>
    public class HResultDeciphering
    {
        private uint m_nHR = 0;
        private uint m_nCode = 0;
        private uint m_nCustomerCodeFlag = 0;
        private SEVERITY m_SEVERITY;
        private FACILITY m_FACILITY;

        /// <summary>
        /// Calculation of values is completely done in constructor
        /// because we will need those only here.
        /// </summary>
        /// <param name="_nHR"></param>
        public HResultDeciphering(uint _nHR)
        {
            this.m_nHR = _nHR;
            this.m_FACILITY = (FACILITY)(this.m_nHR >> 16 & 0x1FFF);
            this.m_SEVERITY = (SEVERITY)(this.m_nHR >> 31 & 0x1);
            this.m_nCustomerCodeFlag = this.m_nHR >> 29 & 0x3;
            this.m_nCode = this.m_nHR & 0xFFFF;
        }

        /// <summary>
        /// returns the facility of HRESULT
        /// </summary>
        public FACILITY Facility
        {
            get { return this.m_FACILITY; }
        }

        /// <summary>
        /// returns the severity of HRESULT
        /// </summary>
        public SEVERITY Severity
        {
            get { return this.m_SEVERITY; }
        }

        /// <summary>
        /// returns the customer code flag
        /// </summary>
        public uint CustomerCodeFlag
        {
            get { return this.m_nCustomerCodeFlag & 0x1; }
        }

        /// <summary>
        /// returns the code
        /// </summary>
        public uint Code
        {
            get { return this.m_nCode; }
        }

        //EDIT: ergänzt...

        /// <summary>
        /// use this to output
        /// </summary>
        public override string ToString()
        {
            string sNewLine = System.Environment.NewLine;
            return "Severity: " + this.Severity.ToString() + sNewLine
                + "Customer Code Flag: " + this.CustomerCodeFlag.ToString() + sNewLine
                + "Facility: " + this.Facility.ToString() + sNewLine
                + "Code: " + this.Code.ToString() + sNewLine;
        }
    }
}