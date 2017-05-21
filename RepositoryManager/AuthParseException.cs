using System;

namespace RepositoryManager
{
    internal class AuthParseException : Exception
    {
        public AuthParseException(string path, int lineNum)
        {
            this.Path = path;
            this.LineNum = lineNum;
        }

        public string Path { get; }

        public int LineNum { get; }

        public override string Message
        {
            get
            {
                if (LineNum != -1)
                {
                    return $"Error parsing line {LineNum} of {Path}";
                }
                else
                {
                    return $"Error parsing {Path}";
                }
            }
        }

    }
}