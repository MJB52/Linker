using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linker
{
    class ExternSymTable
    {
        public List<Node> Table = new List<Node>();
        public void Print()
        {
            string lineSep = new string('=', 58);
            Console.WriteLine(string.Format("{0,-10}{1,-8}{2,-10}{3,-10}{4,-10}{5,-10}", "CSECT","SYMBOL","ADDR","CSADDR","LDADDR","LENGTH"));
            Console.WriteLine(lineSep);
            foreach (var thing in Table)
                Console.WriteLine(thing.ToString());
            Console.WriteLine(lineSep);
        }
        public void Insert(Node node)
        {
            Table.Add(node);
        }
        public Node GetNode(string symbol)
        {
            return Table.FirstOrDefault(c => c.Symbol.Trim() == symbol.Trim() || c.CSect.Trim() == symbol.Trim());
        }
        public List<Node> GetNodes()
        {
            return Table;
        }
    }
    //holds all of the data
    class Node
    {
        public override string ToString()
        {
            return string.Format("{0,-10}{1,-8}{2,-10}{3,-10}{4,-10}{5,-10}",CSect,Symbol,Addr,CsAddr,LAddr,Length);
        }
        public string CSect { get; set; }
        public string Symbol { get; set; }
        public string Addr { get; set; }
        public string CsAddr { get; set; }
        public string LAddr { get; set; }
        public string Length { get; set; }
    }
}
