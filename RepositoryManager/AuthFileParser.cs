using System.Collections.Generic;
using System.IO;

namespace RepositoryManager
{
    /// <summary>
    /// Parses a tortoiseSVN config file
    /// </summary>
    internal class AuthFileParser
    {
        // Set a limit on maximum lines parsed to avoid stalling out on big files
        const int MaxLines = 1000;

        // Parser states
        enum States
        {
            ExpectingKeyDef,
            ExpectingKeyName,
            ExpectingValueDef,
            ExpectingValue
        }

        // Current state
        private States state = States.ExpectingKeyDef;

        // Data persisted between states
        private string keyName = "";
        private int nextLength = -1;

        // Values read so far
        private readonly Dictionary<string, string> props = new Dictionary<string, string>();

        // Only allow access through static ReadFile() method
        private AuthFileParser() { }

        private bool TryParseNextLine(string line)
        {
            switch (state)
            {
                case States.ExpectingKeyDef: return this.ParseKeyDef(line);
                case States.ExpectingKeyName: return this.ParseKeyName(line);
                case States.ExpectingValueDef: return this.ParseValueDef(line);
                case States.ExpectingValue: return this.ParseValue(line);
                default: return false;
            }
        }

        private bool ParseKeyDef(string line)
        {
            if (!this.ParseDefLine("K", line)) return false;
            state = States.ExpectingKeyName;
            return true;
        }

        private bool ParseKeyName(string line)
        {
            if (!this.ParseValLine(line)) return false;
            state = States.ExpectingValueDef;
            return true;
        }

        private bool ParseValueDef(string line)
        {
            if (!this.ParseDefLine("V", line)) return false;
            state = States.ExpectingValue;
            return true;
        }

        private bool ParseValue(string line)
        {
            if (!this.ParseValLine(line)) return false;
            state = States.ExpectingKeyDef;
            return true;
        }

        // Do some rudimentary validation to ensure the current line looks like a definition
        // line, then parse it.  A definition line looks something like "K #" or "V #",
        // where # is the length of the next line.  K means the next line will be a key name,
        // while V means it will be a value.  # will be stored in nextLength.
        private bool ParseDefLine(string prefix, string line)
        {
            line = line.Trim();
            if (!line.ToUpper().StartsWith(prefix + " ")) return false;
            string[] parts = line.Split(' ');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[1], out nextLength)) return false;
            return true;
        }

        // Read a key name or value line.  If this is a value line, then save the key/value
        // pair that has just been read.
        private bool ParseValLine(string line)
        {
            if (line.Length < nextLength) return false;
            string val = line.Substring(0, nextLength);
            nextLength = -1;

            if (state == States.ExpectingKeyName)
            {
                keyName = val.Trim();
                if (keyName == "") return false;
                if (keyName.Contains(" ")) return false;
            }
            else
            {
                props.Add(keyName, val);
                keyName = "";
            }

            return true;
        }

        public static Dictionary<string, string> ReadFile(string path)
        {
            AuthFileParser parser = new AuthFileParser();
            using (StreamReader rd = File.OpenText(path))
            {
                int lineNum = 1;
                string line = rd.ReadLine();
                while (line != null)
                {
                    if (lineNum > MaxLines) break;

                    // Skip comment lines
                    if (!line.Trim().StartsWith("#"))
                    {
                        // Check for end of file marker
                        if (parser.state == States.ExpectingKeyDef && line.Trim().ToUpper() == "END")
                        {
                            return parser.props;  // Return results
                        }

                        // Attempt to parse the line
                        if (!parser.TryParseNextLine(line)) throw new AuthParseException(path, lineNum);
                    }

                    // Read next line
                    lineNum++;
                    line = rd.ReadLine();
                }

                // If reached this point, we either encountered too many lines or the file
                // ended prematurely.
                throw new AuthParseException(path, -1);
            }
        }
    }
}
