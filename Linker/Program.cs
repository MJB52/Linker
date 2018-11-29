using System;

namespace Linker
{
    class Program
    {
        static void Main() 
        {
            //MasterClass? To make calls to other classes. 
            Master m = new Master();
            m.HandleEachFile();
            WriteOCode w = new WriteOCode(m.OCode);
            w.WriteToScreen();
            w.WriteToFile();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit the program...");
            Console.ReadKey();
            //ReadObjectFile //Enviornment.Args;
            //ParseStuff
            //LineReader
            //WriteFile
            //Output
        }
    }
}
