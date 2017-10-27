using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.FileIndexer
{
    public interface IFileIndexer
    {
        // return true if this fileindexer can handle this kind of file (based on the file extension)
        bool CanIndex(string file);

        //return a text representation of the content of the file for indexing
        string GetContent(string file);
    }
}
