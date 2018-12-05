using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Linker
{
    class WriteOCode
    {
        const string FileName = "../../../MEMORYDUMP.DAT";
        List<string> code;
        List<string> FormattedCode = new List<string>();
        public WriteOCode(List<string> ocode)
        {
            code = ocode;
            FormatCode();
        }
        //formats the object code so that this code does not need to be duplicated for filewritetoscreen and filewritetofile
        private void FormatCode()
        {
            int startAddr = 02750;
            int hexCount = 0;
            int pairCount = 0;
            string line = string.Empty;
            line +="        ";
            for(int i = 0; i<16; i++)
            {
                line += string.Format("{0,-3} ",i.ToString("X"));
            }
            FormattedCode.Add(line);
            line = string.Empty;
            foreach(var thing in code)
            {
                if (hexCount == 0)
                    line += startAddr.ToString()+"   ";
                line += thing;
                hexCount++;
                pairCount++;
                if(pairCount == 2)
                {
                    pairCount = 0;
                    line += "  ";
                }
                if (hexCount == 32)
                {
                    hexCount = 0;
                    startAddr += 10;
                    FormattedCode.Add(line);
                    line = string.Empty;
                }
            }
            FormattedCode.Add(line);
        }
        //dumps output to screen
        public void WriteToScreen()
        {
            foreach (var thing in FormattedCode)
                Console.WriteLine(thing);
        }
        //dumps output to file
        public void WriteToFile()
        {
            File.WriteAllLines(FileName, FormattedCode);
        }
    }
}
