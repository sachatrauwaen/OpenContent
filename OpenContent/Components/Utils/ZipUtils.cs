using DotNetNuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public class ZipUtils
    {
        private Type ZipInputStreamType;
        private Type ZipOutputStreamType;
        public ZipUtils()
        {
            ZipInputStreamType = Type.GetType("ICSharpCode.SharpZipLib.Zip.ZipInputStream, ICSharpCode.SharpZipLib");
            if (ZipInputStreamType == null)
            {
                ZipInputStreamType = Type.GetType("ICSharpCode.SharpZipLib.Zip.ZipInputStream, SharpZipLib");
            }
            ZipOutputStreamType = Type.GetType("ICSharpCode.SharpZipLib.Zip.ZipOutputStream, ICSharpCode.SharpZipLib");
            if (ZipOutputStreamType == null)
            {
                ZipOutputStreamType = Type.GetType("ICSharpCode.SharpZipLib.Zip.ZipOutputStream, SharpZipLib");
            }
        }

        public void ZipFiles(int CompressionLevel, FileStream strmZipFile, string[] files)
        {
            //ICSharpCode.SharpZipLib.Zip.ZipOutputStream strmZipStream = null;
            object strmZipStream = null;
            try
            {
                //strmZipStream = new ZipOutputStream(strmZipFile);
                strmZipStream = ZipOutputStreamType.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[] { strmZipFile }, null);
                //strmZipStream.SetLevel(CompressionLevel);
                ZipOutputStreamType.InvokeMember("SetLevel", BindingFlags.InvokeMethod, null, strmZipStream, new object[] { CompressionLevel });
                foreach (var item in files)
                {
                    //FileSystemUtils.AddToZip(ref strmZipStream, Path.GetFullPath(item), Path.GetFileName(item), "");
                    typeof(FileSystemUtils).InvokeMember("AddToZip", BindingFlags.InvokeMethod, null, null, new object[] { strmZipStream, Path.GetFullPath(item), Path.GetFileName(item), "" });
                }
            }
            finally
            {
                if (strmZipStream != null)
                {
                    //strmZipStream.Finish();
                    ZipOutputStreamType.InvokeMember("Finish", BindingFlags.InvokeMethod, null, strmZipStream, null);
                    //strmZipStream.Close();
                    ZipOutputStreamType.InvokeMember("Close", BindingFlags.InvokeMethod, null, strmZipStream, null);
                }
            }
        }

        public void UnzipFiles(Stream stream, string PhysicalPath)
        {
            //ICSharpCode.SharpZipLib.Zip.ZipOutputStream strmZipStream = null;
            object strmZipStream = null;
            strmZipStream = ZipInputStreamType.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[] { stream }, null);
            //FileSystemUtils.UnzipResources(new ZipInputStream(fuFile.FileContent), folder.PhysicalPath);
            typeof(FileSystemUtils).InvokeMember("UnzipResources", BindingFlags.InvokeMethod, null, null, new object[] { strmZipStream, PhysicalPath });
        }
    }
}