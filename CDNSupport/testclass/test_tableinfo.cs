using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CDNSupport.testclass
{
    [Serializable]
    public class test_tableinfo
    {
        public test_tableinfo(string intent, string service, string service_item, string type,string answer) {
            this.intent = intent;
            this.service = service;
            this.service_item = service_item;
            this.type = type;
            this.answer = answer;
        }
        public string intent { set; get; }
        public string service { set; get; }
        public string service_item { set; get; }
        public string type { set; get; }

        public string answer { set; get; }
    }
}