using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linker
{
    class Master
    {
        string LoadAddr = "02750";
        ReadObjectFiles oread = new ReadObjectFiles();
        List<string> Files = new List<string>(Environment.GetCommandLineArgs());
        List<string> OCode = new List<string>();
        ExternSymTable SymTable = new ExternSymTable();
        public void HandleEachFile()
        {
            foreach (var thing in Files)
                if(thing.ToUpper().EndsWith(".O"))
                    Strategy(thing);
        }
        public void Strategy(string FileName)
        {
            List<string> ObjCode = new List<string>();
            var data = oread.GetData(FileName);
            var length = HandleFirstLine(data.First());
            var csectName = GetCSectName(data.First());
            var byteTable = new string[Convert.ToInt32(length)];
            data.Remove(data.First());
            var exxaddr = HandleLastLine(data.Last());
            data.Remove(data.Last());
            HandleDRecs(data, csectName, length, LoadAddr);
            foreach (var line in data)
            {
                byteTable = HandleOtherLines(byteTable, line);
            }
            byteTable = FillInData(byteTable);
            OCode.AddRange(byteTable);
        }

        private string GetCSectName(string line)
        {
            string name = string.Empty;
            for(int i = 1; i < 6; i++)
                name += line[i].ToString();
            return name;
        }

        private void HandleDRecs(List<string> data,string name, string length, string csAddr)
        {
            foreach(string line in data)
            {
                if (line.ToUpper().StartsWith('D'))
                    BreakUpDRec(line, name, length, csAddr);
            }
        }

        private void BreakUpDRec(string line,string name, string proglength, string csAddr)
        {
            SymTable.Insert(new Node
            {
                CSect = name,
                Length = proglength,
                CsAddr = csAddr,
                Addr = string.Empty,
                LAddr = string.Empty,
                Symbol = string.Empty
            });
            line = line.Replace("D", "");
            string symbol = string.Empty, addr = string.Empty;
            int totalLength = line.Length;
            int count = 0;
            while (count - totalLength < 0)
            {
                symbol = string.Empty;
                addr = string.Empty;
                for (int i = 0; i < 4; i++)
                    symbol += line[i];
                for (int i = 4; i < 10; i++)
                    addr += line[i];
                SymTable.Insert(new Node
                {
                    CSect = string.Empty,
                    Length = string.Empty,
                    CsAddr = string.Empty,
                    Addr = addr,
                    LAddr = csAddr.IncrementInHex(addr),
                    Symbol = symbol
                });
                if (line.Length > 10)
                    line = line.Substring(10);
                count += 10;
            }
        }
        private string HandleLastLine(string line)
        {
            string length = string.Empty;
            for (int i = 1; i < 8; i++)
                length += line[i];
            return length;
        }

        private string[] FillInData(string[] byteTable)
        {
            for (int i = 0; i < byteTable.Length; i++)
                if (string.IsNullOrWhiteSpace(byteTable[i]))
                    byteTable[i] = "X";
            return byteTable;
        }

        private string HandleFirstLine(string line)
        {
            string length = string.Empty;
            for (int i = line.Length - 6; i < line.Length; i++)
                length += line[i];
            return length;
        }
        private string [] HandleOtherLines(string [] byteTable, string thing)
        {
            int lineLength;
            int startAddr;
            startAddr = Convert.ToInt32(GetStartAddr(thing)) * 2;
            lineLength = Convert.ToInt32(GetLineLength(thing)) * 2;
            for (int i = startAddr; i < lineLength; i++)
                for (int j = 9; j < thing.Length; j++)
                    byteTable[i] = thing[j].ToString();                
            return byteTable;
        }

        private string GetStartAddr(string thing)
        {
            string addr = string.Empty;
            for (int i = 1; i < 7; i++)
                addr += thing[i];
            return addr;
        }

        private string GetLineLength(string thing)
        {
            return thing[7] + thing[8].ToString();
        }
    }
}
