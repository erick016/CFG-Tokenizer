/*                  COPYRIGHT 2018 JAMES ERICKSON                                         

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Owner
    {
        string owner = null;
        public string get()
        {
            return owner;
        }
        public void set(string arg)
        {
            owner = arg;
        }
    }

    class semanticContainer
    {
        int index;
        Owner owner;
        string associatedText = null;
        List<string> fileList = null;
        List<int> neighborList = null;
    }

    class Program
    {

        public static List<string> seperateString(string text, Char[] delimiters)
        {
            string[] returnStringsArray = text.Split(delimiters);
            List<string> returnStrings = returnStringsArray.OfType<string>().ToList();

            foreach (char delimiter in delimiters)
            {
                returnStrings.RemoveAll(x => x == delimiter.ToString());
            }

            returnStrings.RemoveAll(x => x == "");
            return returnStrings;
        }

        public static SortedDictionary<Tuple<int, int>, string> generateClauseMap(List<string> lines)
        {
            Char[] clauseDelimiter = { ':', ';' };
            SortedDictionary<Tuple<int, int>, string> returnMap = new SortedDictionary<Tuple<int, int>, string>();
            List<string> clauseList = new List<string>();

            int lineNumber = 0;
            foreach (string currStr in lines)
            {
                clauseList = seperateString(currStr, clauseDelimiter);
                int clauseNumber = 0;
                foreach (string clause in clauseList)
                {
                    Console.WriteLine(clause + " [" + lineNumber + "] " + " [" + clauseNumber + "] ");
                    returnMap.Add(new Tuple<int, int>(lineNumber, clauseNumber), clause);
                    clauseNumber++;
                }

                lineNumber++;
            }


            return returnMap;
        }

        public static SortedDictionary<Tuple<int, int>, string> Parser(string fName, string path)
        {
            String contents = "";

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(fName))
                {
                    // Read the stream to a string, and write the string to the console.
                    contents = sr.ReadToEnd();
                    Console.WriteLine(contents);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            Char[] lineDelimiter = { '\r', '\n' };
            List<string> lines = seperateString(contents, lineDelimiter);

            SortedDictionary<Tuple<int, int>, string> clauseMap = generateClauseMap(lines);

            return clauseMap;
        }

        /*
            Input: string clause - The clause is one string in one of the linesof the text file
            Output: A tuple with any desired type. The type should be the same as whatever goes into the dynamic field. 
        */

        public static Tuple<Type, dynamic> getSemantics(String clause) //probably no escaped semicolons right now
        {
            Regex fileListRegEx = new Regex(@"^(?:\s*\()(\w|\W)*(?:\)\s*)$");
            Regex quoteRegEx = new Regex(@"^(?:\s*" + "\")" + @"(\w|\W)*" + "(?:\"" + @"\s*)$");
            Regex alphaNumericRegEx = new Regex("^[a-zA-Z0-9 ]*$");

            if (fileListRegEx.IsMatch(clause))
            {
                // clause = fileListRegEx.GroupNameFromNumber(1);

                Char[] clauseDelimiter = { ',' };
                List<string> tokens = seperateString(clause, clauseDelimiter);
                Console.WriteLine("Entered file list with " + clause);
                Tuple<Type, dynamic> returnTuple = new Tuple<Type, dynamic>(typeof(List<string>), tokens);

                return returnTuple;
            }

            else if (quoteRegEx.IsMatch(clause))
            {
                // clause = fileListRegEx.GroupNameFromNumber(1);

                Tuple<Type, dynamic> returnTuple = new Tuple<Type, dynamic>(typeof(string), clause);
                Console.WriteLine("Entered quote with " + clause);
                return returnTuple;
            }
            else if (clause.Contains(".") && !clause.Contains("(")) //if it's a neighbors list
            {
                Char[] clauseDelimiter = { ',', '.', ' ' };
                List<string> tokensAsString = seperateString(clause, clauseDelimiter);

                List<int> returnTokens = new List<int>();

                foreach (string str in tokensAsString)
                {
                    if (str != "!") // add try catch block later for stability
                    {
                        returnTokens.Add(Int32.Parse(str));
                    }

                    else
                    {
                        returnTokens.Add(0);
                    }
                }

                Tuple<Type, dynamic> returnTuple = new Tuple<Type, dynamic>(typeof(List<int>), returnTokens);
                Console.WriteLine("Entered neighbour's list with " + clause);
                return returnTuple;
            }

            else if (
                alphaNumericRegEx.IsMatch(clause))
            {
                clause = clause.Trim();
                Owner myOwner = new Owner();
                myOwner.set(clause);

                Tuple<Type, dynamic> returnTuple = new Tuple<Type, dynamic>(typeof(Owner), myOwner);
                Console.WriteLine("Entered owner clause with " + clause);
                return returnTuple;
            }

            else
            {
                Tuple<Type, dynamic> voidTuple = new Tuple<Type, dynamic>(typeof(void), -1);
                Console.WriteLine("Return void with " + clause);
                return voidTuple;
            }
        }

        static void Main(string[] args)
        {
            string fName = "";

            string path = Directory.GetCurrentDirectory();
            Console.WriteLine("Enter the .txt conversation file you wish to step through. It must be in the current directory. \r\n The current directory is: \"{0}\"", path);

            fName = Console.ReadLine();

            SortedDictionary<Tuple<int, int>, string> clauseMap = Parser(fName, path); // this passes back "two dimensional data"

            Console.WriteLine("Done seperating tokens.");

            foreach (KeyValuePair<Tuple<int, int>, string> entry in clauseMap) // Going through all entries in grid pattern but the first entry should be a number.
            {

                if (entry.Key.Item2 == 0) //if it's the first thing in the statement
                {
                    try
                    {
                        int number = Int32.Parse(entry.Value); //right now works only for numbers. Later, we will allow labels. 
                    }
                    catch (System.FormatException fe)
                    {
                        Console.WriteLine(fe);
                    }

                }

                else
                {
                    Tuple<Type, dynamic> returnTuple = new Tuple<Type, dynamic>(typeof(void), -1);
                    returnTuple = getSemantics(entry.Value); //we can put the clause in any order
                                                             //getSemantics("(I don't understand!)");

                    Console.WriteLine("Item 1 " + returnTuple.Item1);
                    Console.WriteLine("Item 2 " + returnTuple.Item2);
                }
            }
            Console.WriteLine("Completed");

        }





        /*private static void ParseLine(ref string[] line) //pass string by reference so we can delete as we symbolize.
        {
            SortedDictionary<int, string> allClauseMap = new SortedDictionary<int, string>();
            int key = 0;//iterator for outer loop.

            foreach (string line in lines)//read each line
            {
                
                List<string> clauseList = new List<string>();

                foreach (string currStr in line) //populate clauseList for each line.
                {
                    if (currStr != ";")
                    {
                        clauseList.Add(currStr);
                        allClauseMap.Add(key, currStr);
                    }
                }

                foreach (string currStr in stringsToParse)
                {
                    Char[] clauseDelimiter = { ';' };
                    string[] clauses = currStr.Split(clauseDelimiter);
                }

                key++;
            }
        }*/
    }
}
