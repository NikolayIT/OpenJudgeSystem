namespace OJS.Workers.Tools.Similarity
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This Class implements the Difference Algorithm published in
    /// "An O(ND) Difference Algorithm and its Variations" by Eugene Myers
    /// Algorithmica Vol. 1 No. 2, 1986, p 251.
    /// There are many C, Java, Lisp implementations public available but they all seem to come
    /// from the same source (diffutils) that is under the (unfree) GNU public License
    /// and cannot be reused as a sourcecode for a commercial application.
    /// There are very old C implementations that use other (worse) algorithms.
    /// Microsoft also published sourcecode of a diff-tool (windiff) that uses some tree data.
    /// Also, a direct transfer from a C source to C# is not easy because there is a lot of pointer
    /// arithmetic in the typical C solutions and i need a managed solution.
    /// These are the reasons why I implemented the original published algorithm from the scratch and
    /// make it available without the GNU license limitations.
    /// I do not need a high performance diff tool because it is used only sometimes.
    /// I will do some performace tweaking when needed.
    /// The algorithm itself is comparing 2 arrays of numbers so when comparing 2 text documents
    /// each line is converted into a (hash) number. See DiffText().
    /// Some chages to the original algorithm:
    /// The original algorithm was described using a recursive approach and comparing zero indexed arrays.
    /// Extracting sub-arrays and rejoining them is very performance and memory intensive so the same
    /// (readonly) data arrays are passed around together with their lower and upper bounds.
    /// This circumstance makes the LCS and SMS functions more complicate.
    /// I added some code to the LCS function to get a fast response on sub-arrays that are identical,
    /// completely deleted or inserted.
    /// The result from a comparisation is stored in 2 arrays that flag for modified (deleted or inserted)
    /// lines in the 2 data arrays. These bits are then analyzed to produce a array of Item objects.
    /// Further possible optimizations:
    /// (first rule: don't do it; second: don't do it yet)
    /// The arrays DataA and DataB are passed as parameters, but are never changed after the creation
    /// so they can be members of the class to avoid the parameter overhead.
    /// In SMS is a lot of boundary arithmetic in the for-D and for-k loops that can be done by increment
    /// and decrement of local variables.
    /// The DownVector and UpVector arrays are always created and destroyed each time the SMS gets called.
    /// It is possible to reuse them when transferring them to members of the class.
    /// See TODO: hints.
    /// diff.cs: A port of the algorithm to C#
    /// Copyright (c) by Matthias Hertel, http://www.mathertel.de
    /// This work is licensed under a BSD style license. See http://www.mathertel.de/License.aspx
    /// </summary>
    public class SimilarityFinder : ISimilarityFinder
    {
        /// <summary>
        /// Find the difference in 2 text documents, comparing by textlines.
        /// The algorithm itself is comparing 2 arrays of numbers so when comparing 2 text documents
        /// each line is converted into a (hash) number. This hash-value is computed by storing all
        /// textlines into a common hashtable so i can find duplicates in there, and generating a
        /// new number each time a new textline is inserted.
        /// </summary>
        /// <param name="textA">A-version of the text (usually the old one)</param>
        /// <param name="textB">B-version of the text (usually the new one)</param>
        /// <param name="trimSpace">When set to true, all leading and trailing whitespace characters are stripped out before the comparation is done.</param>
        /// <param name="ignoreSpace">When set to true, all whitespace characters are converted to a single space character before the comparation is done.</param>
        /// <param name="ignoreCase">When set to true, all characters are converted to their lowercase equivivalence before the comparation is done.</param>
        /// <returns>Returns a array of Items that describe the differences.</returns>
        public Difference[] DiffText(
            string textA,
            string textB,
            bool trimSpace,
            bool ignoreSpace,
            bool ignoreCase)
        {
            // prepare the input-text and convert to comparable numbers.
            var h = new Hashtable(textA.Length + textB.Length);

            // The A-Version of the data (original data) to be compared.
            var dataA = new DiffData(this.DiffCodes(textA, h, trimSpace, ignoreSpace, ignoreCase));

            // The B-Version of the data (modified data) to be compared.
            var dataB = new DiffData(this.DiffCodes(textB, h, trimSpace, ignoreSpace, ignoreCase));

            var max = dataA.Length + dataB.Length + 1;

            // vector for the (0,0) to (x,y) search
            var downVector = new int[(2 * max) + 2];

            // vector for the (u,v) to (N,M) search
            var upVector = new int[(2 * max) + 2];

            this.LongestCommonSubsequence(dataA, 0, dataA.Length, dataB, 0, dataB.Length, downVector, upVector);

            this.Optimize(dataA);
            this.Optimize(dataB);
            return this.CreateDiffs(dataA, dataB);
        }

        /// <summary>
        /// Find the difference in 2 arrays of integers.
        /// </summary>
        /// <param name="arrayA">A-version of the numbers (usually the old one)</param>
        /// <param name="arrayB">B-version of the numbers (usually the new one)</param>
        /// <returns>Returns a array of Items that describe the differences.</returns>
        public Difference[] DiffInt(int[] arrayA, int[] arrayB)
        {
            // The A-Version of the data (original data) to be compared.
            var dataA = new DiffData(arrayA);

            // The B-Version of the data (modified data) to be compared.
            var dataB = new DiffData(arrayB);

            var max = dataA.Length + dataB.Length + 1;

            // vector for the (0,0) to (x,y) search
            var downVector = new int[(2 * max) + 2];

            // vector for the (u,v) to (N,M) search
            var upVector = new int[(2 * max) + 2];

            this.LongestCommonSubsequence(dataA, 0, dataA.Length, dataB, 0, dataB.Length, downVector, upVector);
            return this.CreateDiffs(dataA, dataB);
        }

        /// <summary>
        /// This function converts all textlines of the text into unique numbers for every unique textline
        /// so further work can work only with simple numbers.
        /// </summary>
        /// <param name="text">The input text</param>
        /// <param name="usedTextlines">This extern initialized hashtable is used for storing all ever used textlines.</param>
        /// <param name="trimSpace">Ignore leading and trailing space characters</param>
        /// <param name="ignoreSpace">Ignore spaces</param>
        /// <param name="ignoreCase">Ignore text case</param>
        /// <returns>a array of integers.</returns>
        private int[] DiffCodes(string text, Hashtable usedTextlines, bool trimSpace, bool ignoreSpace, bool ignoreCase)
        {
            // get all codes of the text
            int lastUsedCode = usedTextlines.Count;

            // strip off all cr, only use lf as textline separator.
            text = text.Replace("\r", string.Empty);
            var lines = text.Split('\n');

            var codes = new int[lines.Length];

            for (int i = 0; i < lines.Length; ++i)
            {
                var s = lines[i];
                if (trimSpace)
                {
                    s = s.Trim();
                }

                if (ignoreSpace)
                {
                    s = Regex.Replace(s, "\\s+", " "); // TODO: optimization: faster blank removal.
                }

                if (ignoreCase)
                {
                    s = s.ToLower();
                }

                var code = usedTextlines[s];
                if (code == null)
                {
                    lastUsedCode++;
                    usedTextlines[s] = lastUsedCode;
                    codes[i] = lastUsedCode;
                }
                else
                {
                    codes[i] = (int)code;
                }
            }

            return codes;
        }

        /// <summary>
        /// This is the algorithm to find the Shortest Middle Snake (SMS).
        /// </summary>
        /// <param name="dataA">sequence A</param>
        /// <param name="lowerA">lower bound of the actual range in DataA</param>
        /// <param name="upperA">upper bound of the actual range in DataA (exclusive)</param>
        /// <param name="dataB">sequence B</param>
        /// <param name="lowerB">lower bound of the actual range in DataB</param>
        /// <param name="upperB">upper bound of the actual range in DataB (exclusive)</param>
        /// <param name="downVector">a vector for the (0,0) to (x,y) search. Passed as a parameter for speed reasons.</param>
        /// <param name="upVector">a vector for the (u,v) to (N,M) search. Passed as a parameter for speed reasons.</param>
        /// <returns>a MiddleSnakeData record containing x,y and u,v</returns>
        private ShortestMiddleSnakeReturnData ShortestMiddleSnake(
            DiffData dataA,
            int lowerA,
            int upperA,
            DiffData dataB,
            int lowerB,
            int upperB,
            int[] downVector,
            int[] upVector)
        {
            var ret = default(ShortestMiddleSnakeReturnData);
            int max = dataA.Length + dataB.Length + 1;

            int downK = lowerA - lowerB; // the k-line to start the forward search
            int upK = upperA - upperB; // the k-line to start the reverse search

            int delta = (upperA - lowerA) - (upperB - lowerB);
            bool oddDelta = (delta & 1) != 0;

            // The vectors in the publication accepts negative indexes. the vectors implemented here are 0-based
            // and are access using a specific offset: UpOffset UpVector and DownOffset for DownVektor
            int downOffset = max - downK;
            int upOffset = max - upK;

            int maxD = ((upperA - lowerA + upperB - lowerB) / 2) + 1;

            // Debug.Write(2, "SMS", String.Format("Search the box: A[{0}-{1}] to B[{2}-{3}]", LowerA, UpperA, LowerB, UpperB));

            // init vectors
            downVector[downOffset + downK + 1] = lowerA;
            upVector[upOffset + upK - 1] = upperA;

            for (int d = 0; d <= maxD; d++)
            {
                // Extend the forward path.
                for (int k = downK - d; k <= downK + d; k += 2)
                {
                    // Debug.Write(0, "SMS", "extend forward path " + k.ToString());

                    // find the only or better starting point
                    int x;
                    if (k == downK - d)
                    {
                        x = downVector[downOffset + k + 1]; // down
                    }
                    else
                    {
                        x = downVector[downOffset + k - 1] + 1; // a step to the right
                        if ((k < downK + d) && (downVector[downOffset + k + 1] >= x))
                        {
                            x = downVector[downOffset + k + 1]; // down
                        }
                    }

                    int y = x - k;

                    // find the end of the furthest reaching forward D-path in diagonal k.
                    while ((x < upperA) && (y < upperB) && (dataA.Data[x] == dataB.Data[y]))
                    {
                        x++;
                        y++;
                    }

                    downVector[downOffset + k] = x;

                    // overlap ?
                    if (oddDelta && (upK - d < k) && (k < upK + d))
                    {
                        if (upVector[upOffset + k] <= downVector[downOffset + k])
                        {
                            ret.X = downVector[downOffset + k];
                            ret.Y = downVector[downOffset + k] - k;
                            return ret;
                        }
                    }
                }

                // Extend the reverse path.
                for (int k = upK - d; k <= upK + d; k += 2)
                {
                    // Debug.Write(0, "SMS", "extend reverse path " + k.ToString());

                    // find the only or better starting point
                    int x;
                    if (k == upK + d)
                    {
                        x = upVector[upOffset + k - 1]; // up
                    }
                    else
                    {
                        x = upVector[upOffset + k + 1] - 1; // left
                        if ((k > upK - d) && (upVector[upOffset + k - 1] < x))
                        {
                            x = upVector[upOffset + k - 1]; // up
                        }
                    }

                    var y = x - k;

                    while ((x > lowerA) && (y > lowerB) && (dataA.Data[x - 1] == dataB.Data[y - 1]))
                    {
                        x--;
                        y--; // diagonal
                    }

                    upVector[upOffset + k] = x;

                    // overlap ?
                    if (!oddDelta && (downK - d <= k) && (k <= downK + d))
                    {
                        if (upVector[upOffset + k] <= downVector[downOffset + k])
                        {
                            ret.X = downVector[downOffset + k];
                            ret.Y = downVector[downOffset + k] - k;
                            return ret;
                        }
                    }
                }
            }

            throw new Exception("the algorithm should never come here.");
        }

        /// <summary>
        /// This is the divide-and-conquer implementation of the longest common-subsequence (LCS)
        /// algorithm.
        /// The published algorithm passes recursively parts of the A and B sequences.
        /// To avoid copying these arrays the lower and upper bounds are passed while the sequences stay constant.
        /// </summary>
        /// <param name="dataA">sequence A</param>
        /// <param name="lowerA">lower bound of the actual range in DataA</param>
        /// <param name="upperA">upper bound of the actual range in DataA (exclusive)</param>
        /// <param name="dataB">sequence B</param>
        /// <param name="lowerB">lower bound of the actual range in DataB</param>
        /// <param name="upperB">upper bound of the actual range in DataB (exclusive)</param>
        /// <param name="downVector">a vector for the (0,0) to (x,y) search. Passed as a parameter for speed reasons.</param>
        /// <param name="upVector">a vector for the (u,v) to (N,M) search. Passed as a parameter for speed reasons.</param>
        private void LongestCommonSubsequence(
            DiffData dataA,
            int lowerA,
            int upperA,
            DiffData dataB,
            int lowerB,
            int upperB,
            int[] downVector,
            int[] upVector)
        {
            // Debug.Write(2, "LCS", String.Format("Analyse the box: A[{0}-{1}] to B[{2}-{3}]", LowerA, UpperA, LowerB, UpperB));

            // Fast walkthrough equal lines at the start
            while (lowerA < upperA && lowerB < upperB && dataA.Data[lowerA] == dataB.Data[lowerB])
            {
                lowerA++;
                lowerB++;
            }

            // Fast walkthrough equal lines at the end
            while (lowerA < upperA && lowerB < upperB && dataA.Data[upperA - 1] == dataB.Data[upperB - 1])
            {
                --upperA;
                --upperB;
            }

            if (lowerA == upperA)
            {
                // mark as inserted lines.
                while (lowerB < upperB)
                {
                    dataB.Modified[lowerB++] = true;
                }
            }
            else if (lowerB == upperB)
            {
                // mark as deleted lines.
                while (lowerA < upperA)
                {
                    dataA.Modified[lowerA++] = true;
                }
            }
            else
            {
                // Find the middle snake and length of an optimal path for A and B
                var smsrd = this.ShortestMiddleSnake(dataA, lowerA, upperA, dataB, lowerB, upperB, downVector, upVector);

                // Debug.Write(2, "MiddleSnakeData", String.Format("{0},{1}", smsrd.x, smsrd.y));

                // The path is from LowerX to (x,y) and (x,y) to UpperX
                this.LongestCommonSubsequence(dataA, lowerA, smsrd.X, dataB, lowerB, smsrd.Y, downVector, upVector);
                this.LongestCommonSubsequence(dataA, smsrd.X, upperA, dataB, smsrd.Y, upperB, downVector, upVector);
            }
        }

        /// <summary>Scan the tables of which lines are inserted and deleted,
        /// producing an edit script in forward order.
        /// </summary>
        /// dynamic array
        private Difference[] CreateDiffs(DiffData dataA, DiffData dataB)
        {
            var differences = new ArrayList();

            var lineA = 0;
            var lineB = 0;
            while (lineA < dataA.Length || lineB < dataB.Length)
            {
                if ((lineA < dataA.Length) && (!dataA.Modified[lineA]) && (lineB < dataB.Length)
                    && (!dataB.Modified[lineB]))
                {
                    // equal lines
                    lineA++;
                    lineB++;
                }
                else
                {
                    // maybe deleted and/or inserted lines
                    var startA = lineA;
                    var startB = lineB;

                    // while (LineA < DataA.Length && DataA.modified[LineA])
                    while (lineA < dataA.Length && (lineB >= dataB.Length || dataA.Modified[lineA]))
                    {
                        lineA++;
                    }

                    // while (LineB < DataB.Length && DataB.modified[LineB])
                    while (lineB < dataB.Length && (lineA >= dataA.Length || dataB.Modified[lineB]))
                    {
                        lineB++;
                    }

                    if ((startA < lineA) || (startB < lineB))
                    {
                        // store a new difference-item
                        var item = new Difference
                                               {
                                                   StartA = startA,
                                                   StartB = startB,
                                                   DeletedA = lineA - startA,
                                                   InsertedB = lineB - startB
                                               };
                        differences.Add(item);
                    }
                }
            }

            var result = new Difference[differences.Count];
            differences.CopyTo(result);

            return result;
        }

        /// <summary>
        /// If a sequence of modified lines starts with a line that contains the same content
        /// as the line that appends the changes, the difference sequence is modified so that the
        /// appended line and not the starting line is marked as modified.
        /// This leads to more readable diff sequences when comparing text files.
        /// </summary>
        /// <param name="data">A Diff data buffer containing the identified changes.</param>
        private void Optimize(DiffData data)
        {
            int startPos = 0;
            while (startPos < data.Length)
            {
                while ((startPos < data.Length) && (data.Modified[startPos] == false))
                {
                    startPos++;
                }

                int endPos = startPos;
                while ((endPos < data.Length) && data.Modified[endPos])
                {
                    endPos++;
                }

                if ((endPos < data.Length) && (data.Data[startPos] == data.Data[endPos]))
                {
                    data.Modified[startPos] = false;
                    data.Modified[endPos] = true;
                }
                else
                {
                    startPos = endPos;
                }
            }
        }
    }
}
