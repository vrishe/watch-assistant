using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;

namespace watch_assistant.Model.Search
{
    class InterviewAggregator : IInterviewer
    {
        #region Fields

        private DataTable _interviewResult;

        //private AOSInterviewer _aosInterviewer = new AOSInterviewer();
        //private ASeeInterviewer _aseeInterviewer = new ASeeInterviewer();
        //private FilminInterviewer _filminInterviewer = new FilminInterviewer();
        //private TVBestInterviewer _tvbestInterviewer = new TVBestInterviewer();

        //private Thread _aosThread;
        //private Thread _aseeThread;
        //private Thread _filminThread;
        //private Thread _tvbestThread;

        private readonly List<KeyValuePair<InterviewerBase, Thread>> _interviewers = new List<KeyValuePair<InterviewerBase,Thread>>();

        #endregion (Fields)

        #region IInterviewer implementation

        /// <summary>
        /// Gets DataTable with concern search results 
        /// </summary>
        public DataTable InterviewResult
        {
            get
            {
                if (_interviewResult != null)
                    return _interviewResult;
                _interviewResult = new DataTable("InterviewResults");
                return _interviewResult;
            }
        }

        /// <summary>
        /// Fill InterviewResult DataTable with concern search results
        /// </summary>
        /// <param name="query">Strings for server to find</param>
        public virtual void ConductInterview(string[] query)
        {
            FormInterviewers();

            foreach (var interviewer in _interviewers) interviewer.Value.Start(query);
            foreach (var interviewer in _interviewers) 
                if (interviewer.Value.IsAlive)
                    interviewer.Value.Join();

            AggregateResults();
            MessageBox.Show(_interviewResult.Rows.Count.ToString());
            _interviewers.Clear();
        }

        /// <summary>
        /// Clear past interviews results
        /// </summary>
        public void ClearInterviewResults()
        {
            _interviewResult = null;
        }

        #endregion (IInterviewer implementation)

        #region Methods

        private void AggregateResults()
        {
            this.ClearInterviewResults();
            this.FormNewResultTable();

            Dictionary<string, bool[]> rowsVisited = new Dictionary<string, bool[]>();

            foreach (var interview in _interviewers)
            {
                if (!rowsVisited.ContainsKey(interview.Key.InterviewResult.TableName))
                    rowsVisited.Add(
                        interview.Key.InterviewResult.TableName, 
                        new bool[interview.Key.InterviewResult.Rows.Count]
                        );

                foreach (DataRow row in interview.Key.InterviewResult.Rows)
                {
                    if (rowsVisited[interview.Key.InterviewResult.TableName]
                                   [interview.Key.InterviewResult.Rows.IndexOf(row)])
                        continue;

                    foreach (var otherInterview in _interviewers)
                    {
                        if (interview.Key.InterviewResult.TableName ==
                            otherInterview.Key.InterviewResult.TableName)
                            continue;

                        if (!rowsVisited.ContainsKey(otherInterview.Key.InterviewResult.TableName))
                            rowsVisited.Add(
                                otherInterview.Key.InterviewResult.TableName,
                                new bool[otherInterview.Key.InterviewResult.Rows.Count]
                                );
                    
                        foreach (DataRow otherRow in otherInterview.Key.InterviewResult.Rows)
                        {
                            if (rowsVisited[otherInterview.Key.InterviewResult.TableName]
                                       [otherInterview.Key.InterviewResult.Rows.IndexOf(otherRow)])
                                continue;

                            // Fastest cheks to find out if rows are different
                            if (row["RussianAudio"].ToString() != otherRow["RussianAudio"].ToString())
                                continue;
                            if (row["RussianSub"].ToString() != otherRow["RussianSub"].ToString())
                                continue;
                            if (!String.IsNullOrEmpty(row["Year"].ToString()) &&
                                !String.IsNullOrEmpty(otherRow["Year"].ToString()))
                                if (row["Year"].ToString() != otherRow["Year"].ToString())
                                    continue;

                            // Check and (maybe)merge Name here
                            string[] names = GetNames((string)row["Name"]);
                            string[] otherNames = GetNames((string)otherRow["Name"]);
                            if (names[0] != otherNames[0] || names[1] != otherNames[1])
                                continue;

                            // Check and merge Genre here
                            bool matched = (String.IsNullOrEmpty(row["Genre"].ToString()) ||
                                String.IsNullOrEmpty(otherRow["Genre"].ToString()) ? true : false);
                            Match genre = Regex.Match(row["Genre"].ToString(), "([^,]*)(?:,?)");
                            while (!matched && genre.Success)
                            {
                                Match otherGenre = Regex.Match(otherRow["Genre"].ToString(), "([^,]*)(?:,?)");
                                while (!matched && otherGenre.Success)
                                {
                                    if (genre.Groups[1].ToString() == otherGenre.Groups[1].ToString())
                                        matched = true;
                                    else
                                        otherGenre.NextMatch();
                                }
                                genre.NextMatch();
                            }
                            if (!matched)
                                continue;
                            if (row["Genre"].ToString().Length < otherRow["Genre"].ToString().Length)
                                row["Genre"] = new string((otherRow["Genre"].ToString().ToCharArray()));

                            // Merge other info if we got here
                            string[] href = new string[((string[])row["HRef"]).Length + 1];
                            for (int i = 0; i < href.Length - 1; i++)
                                href[i] = new string(((string[])row["HRef"])[i].ToCharArray());
                            href[href.Length - 1] = new string(((string[])otherRow["HRef"])[0].ToCharArray());

                            row["HRef"] = href;
                            if (String.IsNullOrEmpty(row["Description"].ToString()) &&
                                !String.IsNullOrEmpty(otherRow["Description"].ToString()))
                                row["Description"] = otherRow["Description"].ToString();

                            // Mark otherRow as visited because we merge it with row
                            rowsVisited[otherInterview.Key.InterviewResult.TableName]
                                       [otherInterview.Key.InterviewResult.Rows.IndexOf(otherRow)] = true;                            
                        }
                    }
                    _interviewResult.ImportRow(row);
                    rowsVisited[interview.Key.InterviewResult.TableName]
                               [interview.Key.InterviewResult.Rows.IndexOf(row)] = true;
                }
            }                
        }

        private static string[] GetNames(string fullName)
        {
            for (int removeStart = fullName.IndexOf('['); removeStart >= 0; removeStart = fullName.IndexOf('['))
                fullName = fullName.Remove(removeStart, fullName.IndexOf(']') - removeStart + 1);
            for (int removeStart = fullName.IndexOf('('); removeStart >= 0; removeStart = fullName.IndexOf('('))
                fullName = fullName.Remove(removeStart, fullName.IndexOf(')') - removeStart + 1);
            for (Match removeStart = Regex.Match(fullName, @"The([\s]|$)"); 
                removeStart.Success;
                removeStart = Regex.Match(fullName, @"The([\s]|$)"))
                fullName = fullName.Remove(removeStart.Index, removeStart.Length);
            char[] a = fullName.ToCharArray();
            Match nameRus = Regex.Match(fullName, @"([\p{IsCyrillic}]+[0-9\s\W]*)+", RegexOptions.Singleline);
            while (nameRus.Success && nameRus.Value.ToString().Length < 1)
                nameRus.NextMatch();
            string nameEng = fullName.Remove(nameRus.Index, nameRus.Length);

            return (new string[] 
                { nameRus.Value.ToString().Trim(' ', '/', '.', ',').ToLower(), nameEng.Trim(' ', '/', '.', ',').ToLower() });
        }

        private void FormInterviewers()
        {
            _interviewers.Add(new KeyValuePair<InterviewerBase, Thread>
                (new AOSInterviewer(), new Thread((object query) =>
                {
                    _interviewers[0].Key.ConductInterview((string[])query);
                    MessageBox.Show("AOS Results Number: " + _interviewers[0].Key.InterviewResult.Rows.Count.ToString());
                })));
            _interviewers.Add(new KeyValuePair<InterviewerBase, Thread>(
                new ASeeInterviewer(), new Thread((object query) =>
                {
                    _interviewers[1].Key.ConductInterview((string[])query);
                    MessageBox.Show("ASee Results Number: " + _interviewers[1].Key.InterviewResult.Rows.Count.ToString());
                })));
            _interviewers.Add(new KeyValuePair<InterviewerBase, Thread>(
                new TVBestInterviewer(), new Thread((object query) =>
                {
                    _interviewers[2].Key.ConductInterview((string[])query);
                    MessageBox.Show("TVBest Results Number: " + _interviewers[2].Key.InterviewResult.Rows.Count.ToString());
                })));
            _interviewers.Add(new KeyValuePair<InterviewerBase, Thread>(
                new FilminInterviewer(), new Thread((object query) =>
                {
                    _interviewers[3].Key.ConductInterview((string[])query);
                    MessageBox.Show("Filmin Results Number: " + _interviewers[3].Key.InterviewResult.Rows.Count.ToString());
                })));
        }

        private void FormNewResultTable()
        {
            _interviewResult = new DataTable("Agregated results");

            _interviewResult.Columns.Add("Name", typeof(String));
            _interviewResult.Columns.Add("HRef", typeof(String[]));
            _interviewResult.Columns.Add("Poster", typeof(String));
            _interviewResult.Columns.Add("Genre", typeof(String));
            _interviewResult.Columns.Add("Year", typeof(Int32));
            _interviewResult.Columns.Add("Description", typeof(String));
            _interviewResult.Columns.Add("VideoQuality", typeof(String));
            _interviewResult.Columns.Add("RussianAudio", typeof(Boolean));
            _interviewResult.Columns.Add("RussianSub", typeof(Boolean));
        }

        #endregion (Methods)
    }
}
