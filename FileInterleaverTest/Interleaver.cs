using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace FileInterleaverTest
{
    public static class Extensions
    {
        public static bool OneReadIncomplete(this List<FileStream> streamList)
        {
            bool retval = false;
            streamList.ForEach((stream) => {
                retval = retval || stream.Position < stream.Length;
            });
            return retval;
        }
    }

    class Interleaver
    {
        public static MemoryStream Interleave(List<string> paths, BackgroundWorker worker, int byteSize = 4)
        {
            worker.ReportProgress(0);
            long maxSize = 0, currentPos = 0;
            List<FileStream> fileStreams = new List<FileStream>();
            MemoryStream outStream = new MemoryStream();

            paths.ForEach((path) =>
            {
                if (File.Exists(path))
                {
                    FileStream fs = File.OpenRead(path);
                    maxSize += fs.Length;
                    fileStreams.Add(fs);
                }
            });

            byte[] buffer = new byte[4];
            int lastProgress = 0, currentProgress = 0;
            while (fileStreams.OneReadIncomplete())
            {
                fileStreams.ForEach((stream) => {
                    int readSize = stream.Read(buffer, 0, 4);
                    currentPos += readSize;
                    outStream.Write(buffer, 0, readSize);

                    currentProgress = (int)Math.Round((currentPos * 1f / maxSize) * 100f);
                    if (lastProgress != currentProgress) {
                        lastProgress = currentProgress;
                        worker.ReportProgress(currentProgress);
                    }
                });
            }

            fileStreams.ForEach((stream) =>
            {
                stream.Dispose();
                stream.Close();
            });
            return outStream;
        }
    }
}
