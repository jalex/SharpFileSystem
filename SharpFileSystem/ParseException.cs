using System;

namespace SharpFileSystem {

    public class ParseException : Exception {

        public string Input { get; private set; }
        public string Reason { get; private set; }

        public ParseException(string input) : base("Could not parse input \"" + input + "\"") {
            this.Input = input;
            this.Reason = null;
        }

        public ParseException(string input, string reason) : base("Could not parse input \"" + input + "\": " + reason) {
            this.Input = input;
            this.Reason = reason;
        }
    }
}
