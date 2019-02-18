using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SankakuDownloader2.Core
{
    public sealed class FileRecovery
    {
        private readonly string _textFile;

        public FileRecovery(string textFile) => _textFile     = textFile;

        public FileEntry[] Entries { get; private set; }

        public void Read()
        {
            List<FileEntry> entries = new List<FileEntry>();

            using (StreamReader reader = new StreamReader(new FileStream(_textFile, FileMode.Open)))
            {
                for (int i = 0; i < 7; i++)
                    reader.ReadLine();
                char[] splitter = {' '};

                StringBuilder nameBuilder = new StringBuilder();
                StringBuilder path        = new StringBuilder();

                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    nameBuilder.Clear();
                    long size = -1;
                    path.Clear();

                    string[] splitLine = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

                    for (var index = 0; index < splitLine.Length; index++)
                    {
                        var splitentry = splitLine[index];

                        if (char.IsDigit(splitentry[0]))
                        {
                            if (long.TryParse(splitentry, out long tempSize))
                            {
                                int nextElement = index + 1;
                                if (splitLine.Length <= nextElement)
                                    continue;

                                string text = splitLine[nextElement];

                                if (text.Length >= 3 && char.IsLetter(text[0]) && ':' == text[1] && '\\' == text[2])
                                {
                                    size = tempSize;
                                    continue;
                                }
                            }
                        }

                        if (size != -1)
                        {
                            path.Append(' ').Append(splitentry);
                            continue;
                        }

                        nameBuilder.Append(' ').Append(splitentry);
                    }

                    entries.Add(new FileEntry {Name = nameBuilder.ToString().Trim(), Path = path.ToString().Trim(), Size = size});
                }

                Entries = entries.ToArray();
            }
        }
    }
}