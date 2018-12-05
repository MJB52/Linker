using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linker
{
    /// <summary>
    /// Master class that controls the flow of logic. 
    /// </summary>
    class Master
    {
        string LoadAddr = "02750";
        ReadObjectFiles oread = new ReadObjectFiles();
        List<string> Files = new List<string>(Environment.GetCommandLineArgs());
        public List<string> OCode = new List<string>();
        ExternSymTable SymTable = new ExternSymTable();
        /// <summary>
        /// Calls pass 1 and pass 2 for each file
        /// </summary>
        public void HandleEachFile()
        {
            bool firstFile = true;
            foreach (var thing in Files)
                if (thing.ToUpper().EndsWith(".O"))
                {
                    Pass1Strategy(thing, firstFile);
                    firstFile = false;
                }
            int previousProgLength = 0;
            foreach (var thing in Files)
                if (thing.ToUpper().EndsWith(".O"))
                    previousProgLength = Pass2Strategy(thing, previousProgLength);
            SymTable.Print();
        }
        /// <summary>
        /// handles updating m record addresses and such
        /// </summary>
        /// <param name="FileName"></param> name of the file to read in data
        /// <param name="previousProgLength"></param> used for updating addresses in the object code
        /// <returns></returns>
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
        /// <summary>
        /// updates the object code list based on the type of mrecord
        /// </summary>
        /// <param name="line"></param> mrecord
        /// <param name="length"></param> previous program length
        private void HandleMRecs(string line, int length)
        {
            var addr = int.Parse(GetStartAddr(line), System.Globalization.NumberStyles.HexNumber);
            var symbol = GetSymbolName(line);
            var foundNode = SymTable.GetNode(symbol);
            var type = GetLineLength(line);
            int intType = 6;
            string updateVal = string.Empty;
            if (!string.IsNullOrWhiteSpace(foundNode.CSect))
                updateVal = foundNode.CsAddr;
            else
                updateVal = foundNode.LAddr;
            bool sign = false;
            if (line.Contains('+'))
                sign = true;
            int j = (addr * 2) + (length * 2);
            if (type == "05")
            {
                j += 1;
                intType = 5;
            }
            string[] oldVals = new string[intType];
            string update = string.Empty;
            OCode.CopyTo(j, oldVals, 0, intType);
            OCode.RemoveRange(j, intType);
            foreach (var thing in oldVals)
                update += thing;
            if (sign)
                update = update.IncrementInHex(updateVal).PadLeft(intType, '0');
            else
                update = update.DecrementInHex(updateVal).PadLeft(intType, '0');
            for (int i = 0; i < intType; i++)
                oldVals[i] = update[i].ToString();

            OCode.InsertRange(j, oldVals);
        }
        /// <summary>
        /// gets the name of a symbol for an mrecord
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
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
        /// <summary>
        /// data flow for pass one
        /// </summary>
        /// <param name="FileName"></param> read data
        /// <param name="firstFile"></param> if its a firstfile certain things will happen
        private void Pass1Strategy(string FileName, bool firstFile)
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
            if (firstFile)
            {
                var exxaddr = HandleLastLine(data.Last());
                Console.WriteLine($"Execution Address = {LoadAddr.IncrementInHex(exxaddr).PadLeft(5,'0')}");
                firstFile = false;
            }
            data.Remove(data.First());
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
        /// <summary>
        /// gets the name of the csect
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetCSectName(string line)
        {
            string name = string.Empty;
            for(int i = 1; i < 5; i++)
                name += line[i].ToString();
            return name;
        }
        /// <summary>
        /// finds the drecord lines
        /// </summary>
        /// <param name="data"></param>
        /// <param name="csAddr"></param>
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
        /// <summary>
        /// breaks up the drecord line and inserts data into the sym table
        /// </summary>
        /// <param name="line"></param>
        /// <param name="csAddr"></param>
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
        //last line of the file..only used for first file
        private string HandleLastLine(string line)
        {
            string length = string.Empty;
            for (int i = 1; i < line.Length; i++)
                length += line[i];
            return length;
        }
        /// <summary>
        /// fills in all of the empty places with a U
        /// </summary>
        /// <param name="byteTable"></param>
        /// <returns></returns>
        private string[] FillInData(string[] byteTable)
        {
            for (int i = 0; i < byteTable.Length; i++)
                if (string.IsNullOrWhiteSpace(byteTable[i]))
                    byteTable[i] = "U";
            return byteTable;
        }
        /// <summary>
        /// handles the first line of each file..basically gets the control section name
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string HandleFirstLine(string line)
        {
            string length = string.Empty;
            for (int i = line.Length - 6; i < line.Length; i++)
                length += line[i];
            return length;
        }
        /// <summary>
        /// handles t records basically
        /// </summary>
        /// <param name="byteTable"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private string [] HandleOtherLines(string [] byteTable, string line)
        {
            int lineLength;
            int startAddr;
            startAddr = int.Parse(GetStartAddr(line), System.Globalization.NumberStyles.HexNumber) * 2;
            lineLength = int.Parse(GetLineLength(line), System.Globalization.NumberStyles.HexNumber) * 2;
            int i = startAddr;
            for (int j = 9; j < line.Length; j++)
            {
                byteTable[i] = line[j].ToString();
                i++;
            }
            return byteTable;
        }
        //gets the start address of a specific line
        private string GetStartAddr(string thing)
        {
            string addr = string.Empty;
            for (int i = 1; i < 7; i++)
                addr += thing[i];
            return addr;
        }
        //gets the length of a line
        private string GetLineLength(string thing)
        {
            return thing[7] + thing[8].ToString();
        }
    }
}
