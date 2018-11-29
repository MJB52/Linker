using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linker
{
    class ExternSymTable
    {
        List<Node> Table = new List<Node>();
        public void Insert(Node node)
        {
            Table.Add(node);
        }
        public Node GetNode(string symbol)
        {
            return Table.FirstOrDefault(c => c.Symbol == symbol);
        }
    }
    class Node
    {
        public string CSect { get; set; }
        public string Symbol { get; set; }
        public string Addr { get; set; }
        public string CsAddr { get; set; }
        public string LAddr { get; set; }
        public string Length { get; set; }
    }
}
