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
        public List<string> OCode = new List<string>();
        ExternSymTable SymTable = new ExternSymTable();
        public void HandleEachFile()
        {
            foreach (var thing in Files)
                if (thing.ToUpper().EndsWith(".O"))
                    Pass1Strategy(thing);
            int previousProgLength = 0;
            foreach (var thing in Files)
                if (thing.ToUpper().EndsWith(".O"))
                    previousProgLength = Pass2Strategy(thing, previousProgLength);
            SymTable.Print();
        }

        private int Pass2Strategy(string FileName, int previousProgLength)
        {
            var data = oread.GetData(FileName);
            var length = int.Parse(HandleFirstLine(data.First()), System.Globalization.NumberStyles.HexNumber);
            var csectName = GetCSectName(data.First());
            foreach (var line in data)
            {
                if (line.ToUpper().StartsWith('M'))
                    HandleMRecs(line, previousProgLength);
            }
            return length;
        }

        private void HandleMRecs(string line, int length)
        {
            var addr = int.Parse(GetStartAddr(line), System.Globalization.NumberStyles.HexNumber);
            var symbol = GetSymbolName(line);
            var foundNode = SymTable.GetNode(symbol);
            string updateVal = string.Empty;
            if (!string.IsNullOrWhiteSpace(foundNode.CSect))
                updateVal = foundNode.CsAddr;
            else
                updateVal = foundNode.LAddr;
            int j = 0;
            if(updateVal.Length == 5)
                for (int i = (addr * 2) + 1 + length; i < (addr * 2) + length + 1 + updateVal.Length; i++)
                {
                    OCode[i] = updateVal[j].ToString();
                    j++;
                }
            else
                for (int i = (addr * 2) + length; i < (addr * 2) + length + updateVal.Length; i++)
                {
                    OCode[i] = updateVal[j].ToString();
                    j++;
                }
        }

        private string GetSymbolName(string line)
        {
            int index;
            string name = string.Empty;
            if (line.Contains('+'))
                index = line.LastIndexOf('+');
            else
                index = line.LastIndexOf('-');
            for (int i = index + 1; i < line.Length; i++)
                name += line[i];
            return name;
        }

        private void Pass1Strategy(string FileName)
        {
            var data = oread.GetData(FileName);
            var length = int.Parse(HandleFirstLine(data.First()), System.Globalization.NumberStyles.HexNumber); 
            var csectName = GetCSectName(data.First());
            var byteTable = new string[length * 2];
            SymTable.Insert(new Node
            {
                CSect = csectName,
                Length = length.ToString("X").PadLeft(6, '0'),
                CsAddr = LoadAddr.PadLeft(5, '0'),
                Addr = string.Empty,
                LAddr = string.Empty,
                Symbol = string.Empty
            });
            data.Remove(data.First());
            var exxaddr = HandleLastLine(data.Last());
            data.Remove(data.Last());
            HandleDRecs(ref data, LoadAddr);
            foreach (var line in data)
            {
                if(!line.ToUpper().StartsWith('R') && !line.ToUpper().StartsWith('M'))
                    byteTable = HandleOtherLines(byteTable, line);
            }
            byteTable = FillInData(byteTable);
            data = oread.GetData(FileName);
            OCode.AddRange(byteTable);
            LoadAddr = LoadAddr.IncrementInHex(length.ToString("X"));
        }
        private string GetCSectName(string line)
        {
            string name = string.Empty;
            for(int i = 1; i < 5; i++)
                name += line[i].ToString();
            return name;
        }

        private void HandleDRecs(ref List<string> data, string csAddr)
        {
            foreach(string line in data)
            {
                string temp = line;
                if (line.ToUpper().StartsWith('D'))
                    BreakUpDRec(line, csAddr);
            }
            data.RemoveAll(c => c.ToUpper().StartsWith('D'));
        }

        private void BreakUpDRec(string line, string csAddr)
        {
            line = line.Substring(1);
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
                    Addr = addr.PadLeft(5, '0'),
                    LAddr = csAddr.IncrementInHex(addr).PadLeft(5, '0'),
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
            for (int i = 1; i < line.Length; i++)
                length += line[i];
            return length;
        }

        private string[] FillInData(string[] byteTable)
        {
            for (int i = 0; i < byteTable.Length; i++)
                if (string.IsNullOrWhiteSpace(byteTable[i]))
                    byteTable[i] = "U";
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
            startAddr = int.Parse(GetStartAddr(thing), System.Globalization.NumberStyles.HexNumber) * 2;
            lineLength = int.Parse(GetLineLength(thing), System.Globalization.NumberStyles.HexNumber) * 2;
            int i = startAddr;
            for (int j = 9; j < thing.Length; j++)
            {
                byteTable[i] = thing[j].ToString();
                i++;
            }
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
