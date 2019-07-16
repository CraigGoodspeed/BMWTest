using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMWTest
{
    class DoCopy : INotifyPropertyChanged
    {
        int totalFileCount = -1;
        int filesCopied;
        List<Instruction> copyInstructions;

        public DoCopy(string sourcePath, string destinationPath, bool includeSubDirectories, bool deleteInDestination)
        {
            
            Container source = new Container(sourcePath);
            Container destination = new Container(destinationPath);
            recurseFolders(source.items, source.Directory, includeSubDirectories);
            recurseFolders(destination.items, destination.Directory, includeSubDirectories);
            copyInstructions = new List<Instruction>();
            copyInstructions.AddRange((from src in source.items
                       join dest in destination.items
                              on src.path.Replace(source.rootPath, string.Empty) equals dest.path.Replace(destination.rootPath, string.Empty) into joinedList
                       from sub in joinedList.DefaultIfEmpty()
                       where sub == null || src.size != sub.size || src.lastUpdate != sub.lastUpdate
                                select new CopyInstruction(destination.rootPath, source.rootPath, src.path , src.size == -1)).ToList());
            if (deleteInDestination)
            copyInstructions.AddRange((from dest in destination.items
                                       join src in source.items
                                              on dest.path.Replace(destination.rootPath, string.Empty) equals src.path.Replace(source.rootPath, string.Empty) into joinedList
                                       from sub in joinedList.DefaultIfEmpty()
                                       where sub == null 
                                       select new RemoveInstruction(dest.path, dest.size == -1)).ToList());
            
            totalFileCount = copyInstructions.Count();
        }

        public void doCopy()
        {
            Logger.Log("starting the process");
            foreach (Instruction ci in copyInstructions)
            {
                ci.doInstruction();
                filesCopied++;
                OnPropertyChanged("filesCopied");
                Logger.Log(string.Format("{0} complete", fileCopyPercentage));
            }
        }

        public int fileCopyPercentage
        {
            get
            {
                return (int)((double)filesCopied / totalFileCount * 100);
            }
        }

        public static void recurseFolders(List<Item> addHere, DirectoryInfo recurseMe, bool includeSubDirectories)
        {
            addHere.AddRange((from i in recurseMe.GetFiles()
                              select new Item(i.FullName, i.LastWriteTime, i.Length)).ToList());
            addHere.AddRange((from i in recurseMe.GetDirectories() select new Item(i.FullName, i.LastWriteTime, -1)));
            if (includeSubDirectories)
                recurseMe.GetDirectories().ToList().ForEach(i => recurseFolders(addHere, i, includeSubDirectories));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public interface Instruction{
        void doInstruction();
    }
    public class CopyInstruction : Instruction
    {
        public string destinationRoot, sourceToCopy, sourceRoot;
        bool isDirectory;

        protected CopyInstruction() { }
        public CopyInstruction(string destinationRoot, string sourceRoot, string sourceToCopy, bool isDirectory)
        {
            this.destinationRoot = destinationRoot;
            this.sourceRoot = sourceRoot;
            this.sourceToCopy = sourceToCopy;
            this.isDirectory = isDirectory;
        }
        public void doInstruction()
        {
            string destination = string.Concat(destinationRoot, sourceToCopy.Replace(sourceRoot, string.Empty));
            Logger.Log(string.Format("copying file {0}", destination));
            string directory = Path.GetDirectoryName(destination);
            if(isDirectory)
                Directory.CreateDirectory(directory);
            else
            {
                string dir = Path.GetDirectoryName(destination);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(sourceToCopy, destination, true);
            }
        }
    }
    public class RemoveInstruction : Instruction
    {
        string path;
        bool isDirectory;
        public RemoveInstruction(string path, bool isDirectory)
        {
            this.path = path;
            this.isDirectory = isDirectory;
        }
        public void doInstruction()
        {
            Logger.Log(string.Format("Deleting {0}", path));
            if (isDirectory)
                Directory.Delete(path);
            else
                File.Delete(path);
        }
    }
    public class Container
    {
        public List<Item> items{get;set;}
        public string rootPath{get;set;}
        public Container(string rootPath)
        {
            this.rootPath = rootPath;
            items = new List<Item>();
            Directory = new DirectoryInfo(rootPath);
        }
        public DirectoryInfo Directory{get;set;}
    }
    public class Item{
        public string path { get; set; }
        public DateTime lastUpdate { get; set; }
        public long size { get; set; }
        public Item(string path, DateTime lastUpdate, long size)
        {
            this.path = path;
            this.lastUpdate = lastUpdate;
            this.size = size;
        }
    }
}
