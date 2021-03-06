﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Web
{
    public class Config
    {
        private Config() { }

        public static readonly Config Current;
        static Config()
        {
            Current = new Config()
            {
                JSONP_ACTION_KEY = "callback",
                BACK_URL_KEY = "backurl",
                SESSION_USER_KEY = "SessionUserKey",
                CERT_CODE = "CertCode",
                VERIFY_MOBILE="VerifyMobile",
                COOKIE_REMEMBER_USER_KEY = "bu", //browser-user

            };
        }

        public string JSONP_ACTION_KEY { get; set; }
        public string BACK_URL_KEY { get; set; }
        public string SESSION_USER_KEY { get; set; }
        public string CERT_CODE { get; set; }
        public string VERIFY_MOBILE { get; set; }
        public string COOKIE_REMEMBER_USER_KEY { get; set; }
    }
}
