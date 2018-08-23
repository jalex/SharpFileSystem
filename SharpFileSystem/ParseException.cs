using System;

namespace SharpFileSystem {

    public class ParseException : Exception {

        #region properties

        public string Input { get; }
        public string Reason { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ParseException(string input) : base("Could not parse input \"" + input + "\"") {
            this.Input = input;
            this.Reason = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParseException(string input, string reason) : base("Could not parse input \"" + input + "\": " + reason) {
            this.Input = input;
            this.Reason = reason;
        }
    }
}
