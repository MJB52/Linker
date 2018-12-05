using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Linker
{
    public class ReadObjectFiles
    {
        //reads data from a specific file
        public List<string> GetData(string fName)
        {
            fName = SetFileName(fName);
            List<string> Data = new List<string>();
            try
            {
                Data = new List<string>(File.ReadAllLines(fName));
                Data = Data.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            }
            catch (FileNotFoundException)//hopefully never happens
            {
                Console.WriteLine("{0} could not be found.", fName);
            }
            return Data;
        }
        //map to project folder not solution
        private string SetFileName(string file) //add filepath
        {
            return "../../../" + file;
        }
    }
}
