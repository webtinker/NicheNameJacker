using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicheNameJacker.Common
{

    public class Question
    {
        public string name { get; set; }
        public int type { get; set; }
    }

    public class Answer
    {
        public string name { get; set; }
        public int type { get; set; }
        public int TTL { get; set; }
        public string data { get; set; }
    }


    public class RootObject
    {
        public int Status { get; set; }
        public bool TC { get; set; }
        public bool RD { get; set; }
        public bool RA { get; set; }
        public bool AD { get; set; }
        public bool CD { get; set; }
        public List<Question> Question { get; set; }
        public List<Answer> Answer { get; set; }
        public string Comment { get; set; }
    }

}
