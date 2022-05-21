using System;

using System.Reflection;//reflection Assembly.GetExecutingAssembly().Location

using System.IO;//directory

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;



namespace osProj

{
    static class virtual_disk
    {
        public static FileStream disk;
        public static void inti_disk(string str)
        {

            if (!File.Exists(str))
            {
                disk = File.Create(str);
                byte[] b = new byte[1024];
                Write_cluster(0, b);
                Mini_fat.prepare_fat();
                Mini_fat.write_fat();



            }
            else
            {
                disk = new FileStream(str, FileMode.Open);
                Mini_fat.read_fat();



            }


        }
        public static byte[] read_cluster(int cluster_index)
        {
            disk.Seek(cluster_index * 1024, SeekOrigin.Begin);
            byte[] bi = new byte[1024];
            disk.Read(bi);
            return bi;


        }

        public static void Write_cluster(int cluster_index, byte[] b)
        {

            disk.Seek(cluster_index * 1024, SeekOrigin.Begin);

            disk.Write(b);
            disk.Flush();


        }
    }
    public class Mini_fat
    {
        static int[] fat = new int[1024];
        public static void prepare_fat()
        {
            for (int i = 0; i < 1024; i++)
            {
                if (i == 0 || i == 4)
                    fat[i] = -1;
                else if (i == 1 || i == 2 || i == 3)
                    fat[i] = i + 1;
                else
                    fat[i] = 0;
            }
        }
        public static void write_fat()
        {
            byte[] res = new byte[fat.Length * sizeof(int)];
            System.Buffer.BlockCopy(fat, 0, res, 0, 4096 * 4);
            for (int i = 0; i < 4; i++)
            {
                byte[] c = new byte[1024];
                for (int j = 0; j < 1024; j++)
                {
                    c[j] = res[(1024 * i) + j];
                }
                virtual_disk.Write_cluster(i + 1, c);

            }
        }
        public static void read_fat()
        {
            byte[] B = new byte[4096];
            for (int i = 0; i < 4; i++)
            {
                byte[] c = new byte[1024];

                virtual_disk.read_cluster(i + 1);
                for (int j = 0; j < 1024; j++)
                {
                    B[(1024 * i) + j] = c[j];
                }

                int[] res = new int[fat.Length * sizeof(byte)];
                System.Buffer.BlockCopy(B, 0, res, 0, res.Length);

            }
        }
        public static int setcluster(int index)
        {
            Console.WriteLine(fat[index]);
            return fat[index];
        }
        public static void setclusterstatues(int index, int value)
        {
            fat[index] = value;
        }
        public static int get_available_block()
        {
            int i;
            for (i = 1; i < 1024; i++)
            {
                if (fat[i] == 0)
                    break;

            }
            return i;


        }

    }
    public class directory_entery
    {
        char[] dir_name = new char[11];
        byte dir_atlr;
        byte[] dir_empty = new byte[12];
        public int fristcluster;
        int dir_filesize;


        public directory_entery(char[] dir_name, byte dir_atlr, int dir_filesize)
        {
            this.dir_name = dir_name;
            this.dir_atlr = dir_atlr;
            this.dir_filesize = dir_filesize;

        }
        /*  public int firstcluster()
          {
              return fristcluster;
          }*/



    }
    public class directory : directory_entery
    {
        public static List<directory_entery> dirsfile;

        directory parant;

        public directory(char[] dir_name, byte dir_atlr, int dir_filesize, directory parent) : base(dir_name, dir_atlr, dir_filesize)
        {
            if (parent != null)
                this.parant = parent;

            dirsfile = new List<directory_entery>();

        }



        public void writedirectory()
        {
            byte[] b = new byte[dirsfile.Count * 32];
            int x = -1;



            for (int i = 0; i < dirsfile.Count; i++)
            {
                byte[] c = dirsfile.OfType<byte>().ToArray();
                Array.Reverse(c);

                for (int j = 0; j < c.Length; j++)
                {
                    x++;
                    b[x + j] = c[j];
                }
            }
            if (b.Length > 1024)
            {
                List<byte[]> arry = new List<byte[]> { };

                for (int s = 0; s < b.Length / 1024; s++)
                {
                    byte[] sb = new byte[1024];
                    for (int d = 0; d < 1024; d++)
                    {
                        sb[d] = b[(1024 * s) + d];
                    }

                    arry.Add(sb);
                }
                int laster_cluster = -1;
                int cluster_index;
                if (this.fristcluster == 0)
                {
                    cluster_index = Mini_fat.get_available_block();
                    cluster_index = fristcluster;

                }
                else
                {
                    cluster_index = fristcluster;

                }
                for (int i = 0; i < arry.Count; i++)
                {
                    if (cluster_index != 0)
                    {
                        virtual_disk.Write_cluster(cluster_index, arry[i]);
                        Mini_fat.setclusterstatues(cluster_index, -1);
                        if (cluster_index != -1)
                            Mini_fat.setclusterstatues(laster_cluster, cluster_index);
                        laster_cluster = cluster_index;
                        cluster_index = Mini_fat.get_available_block();
                    }


                }
                //   Mini_fat.write_fat();
                if (this.parant != null)
                {
                    this.parant.writedirectory();
                }






            }
            Mini_fat.write_fat();

        }
        public void read_Directory()
        {
            if (this.fristcluster != 0)
            {
                dirsfile = new List<directory_entery>();
                int cluster = this.fristcluster;
                int next = Mini_fat.setcluster(cluster);
                List<byte> Is = new List<byte>();
                do
                {
                    Is.AddRange(virtual_disk.read_cluster(cluster));
                    cluster = next;
                    if (cluster != 0)
                        next = Mini_fat.setcluster(cluster);



                } while (next != 0);
                for (int i = 0; i < Is.Count; i++)
                {
                    byte[] w = new byte[32];
                    for (int j = i * 32, y = 0; y < w.Length && j < Is.Count; j++, y++)
                    {

                        w[y] = Is[j];

                    }
                }


            }

        }



    }
    public class command
    {
        public static void check(string x)
        {
            string[] q = x.Split(' ');
            // Console.WriteLine(q.Length);
            if (x == "cls"|| x == "CLS")
                Clear();
            else if (x == "quit"|| x == "QUIT")
                Quit();
            else if (x == "help"|| x == "HELP")
                Help();
            else if (q[0] == "md" || q[0] == "MD")
                MD(q[1]);
            else if (q[0] == "rd" || q[0] == "RD")
                rd(q[1]);
            else if (q[0] == "type"||q[0] == "TYPE")
            {
                for(int i=1; i< q.Length; i++)
                {
                    type(q[i]);
                }
            }

            else if (q[0] == "del"|| q[0] == "DEL")
            {
                for(int i=1;i<q.Length;i++)
                delete(q[i]);

            }
                
            else if (q.Length >2 && q[0] == "copy")
                copy(q[1],q[2]);

            else if (q[0] == "help" && q.Length > 1)
            {

                if (q[1] == "clear")
                    cls();
                else if (q[1] == "quit")
                    quit();
                else if (q[1] == "help")
                    help();
                else if (q[1] == "md")
                    made();
                else if (q[1] == "rename")
                    rename();
                else if (q[1] == "type")
                    Type();
                else if (q[1] == "rd")
                    remove();
                else if (q[1] == "del")
                    Delete();
                else if (q[1] == "copy")
                    COPY();
                else
                    Console.WriteLine(x + " is not recognized as an internal or external command program or batch file.");

            }
            else if (q.Length > 2 && q[0] == "rename")
                Rename(q[1], q[2]);


            else
                Console.WriteLine(x + " is not recognized as an internal or external command program or batch file.");


            //Console.WriteLine(x + " is not recognized as an internal or external command program or batch file.");


        }
        static void Clear()
        {
            Console.Clear();
        }


        static void Quit()
        {


            Environment.Exit(-1);


            // System.Threading.Thread.Sleep(5000);
        }
        static void MD(string a)
        {
            
            string dir = $@"D:\{a}";
            // If directory does not exist, create it
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
           
            
        }
          

        public static void rd(string a)
        {
            
            string dir = $@"D:\{a}";
            // If directory does not exist, create it
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir);
            }
            else
                Console.WriteLine("the file is not exists");
        }
        public static  void Rename(string a ,string c)
        {
            rd(a);
            MD(c);
            Console.WriteLine("file is renamed");
        }
       public static void type(string a)
        {
            string[] lines = File.ReadAllLines($@"D:\{a}");

            foreach (string line in lines)
                Console.WriteLine(line);
        }
        public static void copy(string sour , string dest)
        {
            string sourceFile = $@"D:\{sour}";
            string destinationFile = $@"E:\{dest}";
           
            System.IO.File.Copy(sourceFile, destinationFile);
            Console.WriteLine("the process is done");
            
        }
        public static void delete(string sour)
        {
            string sourceFile = $@"D:\{sour}";
            

            System.IO.File.Delete(sourceFile);

        }
        static void Help()
        {

            Console.WriteLine("  CD :       Displays the name of or changes the current directory.");
            Console.WriteLine("  CLS :      Clears the screen.");
            Console.WriteLine("  DIR  :     Displays a list of files and subdirectories in a directory.");
            Console.WriteLine("  EXIT:      Quits the CMD.EXE program (command interpreter).");
            Console.WriteLine("  COPY :     Copies one or more files to another location.");
            Console.WriteLine("  DEL  :     Deletes one or more files.");
            Console.WriteLine("  HELP:      Provides Help information for Windows commands.");
            Console.WriteLine("  MD  :      Creates a directory.");
            Console.WriteLine("  RD  :      Removes a directory.");
            Console.WriteLine("  RENAME:    Renames a file or files.");
            Console.WriteLine("  TYPE  :    Displays the contents of a text file.");

        }
        public static void cls()
        {
            Console.WriteLine();
            Console.WriteLine("CLS            Clears the screen.");
            Console.WriteLine();
           

        }
        public static void quit()
        {
            Console.WriteLine();
            Console.WriteLine("EXIT           Quits the CMD.EXE program (command interpreter).");
            Console.WriteLine();
           
        }
        public static void help()
        {
            Console.WriteLine();
            Console.WriteLine("HELP           Provides Help information for Windows commands.");
            Console.WriteLine();
            
        }
        public static void made()
        {
            Console.WriteLine();
            Console.WriteLine("MD             Creates a directory.");
            Console.WriteLine();
            
        }
        public static void remove()
        {
            Console.WriteLine();
            Console.WriteLine("RD             Removes a directory.");
            Console.WriteLine();
            
        }
        public static void rename()
        {
            Console.WriteLine();
            Console.WriteLine("RENAME         Renames a file or files.");
            Console.WriteLine();
            
        }
        public static void Type()
        {
            Console.WriteLine();
            Console.WriteLine("TYPE           Displays the contents of a text file.");
            Console.WriteLine();
            
        }
        public static void COPY()
        {
            Console.WriteLine();
            Console.WriteLine("COPY           Copies one or more files to another location.");
            Console.WriteLine();

        }
        public static void Delete()
        {
            Console.WriteLine();
            Console.WriteLine("DEL            Deletes one or more files.");
            Console.WriteLine();

        }

    }
    public static class ExtendedMethod
    {
        public static void Rename(this FileInfo fileInfo, string newName)
        {
            fileInfo.MoveTo(fileInfo.Directory.FullName + "\\" + newName);
        }
    }
    class Program

    {






            static void Main(string[] args)

            {


                string x, y;
            virtual_disk.inti_disk(@"F:\cmd.txt");


            var Getdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
               

                while (true)
                {
                    Console.Write(Getdir + ">> ");
                    y = Console.ReadLine();
               
                command.check(y);

                       


                }

            }

        

    }
}
