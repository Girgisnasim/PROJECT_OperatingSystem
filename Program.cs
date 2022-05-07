using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections.Generic;


namespace cmd
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
        public static char[] dir_name = new char[11];
        public static byte dir_atlr;
        public static byte[] dir_empty = new byte[12];
        public static int fristcluster;
        public static int dir_filesize;


        /* public directory_entery(char[] dir_name, byte dir_atlr, int dir_filesize)
         {
             dir_name = dir_name;
             dir_atlr = dir_atlr;
             dir_filesize = dir_filesize;

         }*/

    }
    public class directory : directory_entery
    {
        public static List<directory_entery> dirsfile = new List<directory_entery>();

        directory parant;

        public directory(char[] a, byte c, int frist_cluster, directory parent)
        {
            dir_name = a;
            dir_atlr = c;
            fristcluster = fristcluster;
            this.parant = parant;











        }



        static void writedirectory()
        {
            byte[] b = new byte[dirsfile.Count * 32];
            int x = -1;
            for (int i = 0; i < dirsfile.Count; i++)
            {
                byte[] c = BitConverter.GetBytes(dir_filesize);
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
                if (fristcluster == 0)
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

                    virtual_disk.Write_cluster(cluster_index, arry[i]);
                    Mini_fat.setclusterstatues(cluster_index, -1);
                    if (cluster_index != -1)
                        Mini_fat.setclusterstatues(laster_cluster, cluster_index);
                    laster_cluster = cluster_index;
                    cluster_index = Mini_fat.get_available_block();


                }
                Mini_fat.write_fat();






            }


        }
        public void read_Directory()
        {
            if (fristcluster != 0)
            {
                dirsfile = new List<directory_entery>();
                int clusterindex = fristcluster;
                int next = Mini_fat.setcluster(clusterindex);
                do
                {
                    byte[] c = virtual_disk.read_cluster(clusterindex);
                    for (int i = 0; i < c.Length; i++)
                    {
                        byte[] w = new byte[32];
                        c.CopyTo(w, i * 32);

                    }


                } while (clusterindex != 0);


            }

        }



    }
    class Program
    {
        static void Clear()
        {
            Console.Clear();
        }
        static void Quit()
        {


            Environment.Exit(-1);


            // System.Threading.Thread.Sleep(5000);
        }
        static void Help()
        {

            Console.WriteLine("CD             Displays the name of or changes the current directory.");
            Console.WriteLine("CLS            Clears the screen.");
            Console.WriteLine("DIR            Displays a list of files and subdirectories in a directory.");
            Console.WriteLine("EXIT           Quits the CMD.EXE program (command interpreter).");
            Console.WriteLine("COPY           Copies one or more files to another location.");
            Console.WriteLine("DEL            Deletes one or more files.");
            Console.WriteLine("HELP           Provides Help information for Windows commands.");
            Console.WriteLine("MD             Creates a directory.");
            Console.WriteLine("RD             Removes a directory.");
            Console.WriteLine("RENAME         Renames a file or files.");
            Console.WriteLine("TYPE           Displays the contents of a text file.");

        }

        static void Main(string[] args)
        {

            string x, y;

            virtual_disk.inti_disk(@"F:\cmd.txt");

            var Getdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = @"F:\ project";
            string path2 = @"F:\cmd.txt";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            while (true)
            {
                Console.Write(Getdir + ">> ");
                y = Console.ReadLine();
                if (y == "cls")
                    Clear();
                else if (y == "quit")
                    Quit();
                else if (y == "help")
                    Help();
                else
                    Console.WriteLine(y + " is not recognized as an internal or external command program or batch file.");


            }



        }
    }
}
